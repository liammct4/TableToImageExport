using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;
using ImgTableDataExporter;
using ImgTableDataExporter.DataStructures;
using ImgTableDataExporter.ImageData;
using ImgTableDataExporter.TableStructure;
using ImgTableDataExporter.Utilities;
using System.Diagnostics;
using System.Collections.ObjectModel;
using System.IO;

namespace ImgTableDataExporter
{
	/// <summary>
	/// Class for manipulating tablular data and producing images. Can also process CSV data.
	/// </summary>
	public class TableGenerator
	{
		/// <summary>
		/// Stores every cell within the table, the table is structured per cell (see <see cref="TableCell.TablePosition"/>) so the order of the list does not matter. Changes made to this list will update any <see cref="TableColumn"/> and <see cref="TableRow"/> automatically.<br/><br/>
		/// 
		/// Cells should only be added to this if the cell belongs to this table. (Where <see cref="TableCell.Parent"/> is this <see cref="TableGenerator"/> instance).<br/><br/>
		/// 
		/// The exception <see cref="TableMismatchException"/> is thrown where any value in the provided list contains a cell which belongs to a different table.
		/// </summary>
		public ObservableCollection<TableCell> Cells
		{
			get => _cells;
			set
			{
				if (value is null)
				{
					throw new ArgumentNullException("The table cannot be set to null.");
				}

				// Check that the cell belongs to this table.
				foreach (TableCell cell in value)
				{
					if (!cell.Parent.Equals(this))
					{
						throw new TableMismatchException(cell, this); 
					}
				}

				_cells = value;
				TableCollectionInvalidated?.Invoke();
			}
		}
		/// <summary>
		/// When exporting the table, this will determine how rounded the corners will be, set to 0 to get a square table/grid and to any positive 
		/// </summary>
		public uint CornerRadius { get; set; }
		public event Action TableCollectionInvalidated;
		/// <summary>
		/// Gets the width and length of the table in terms of the number of rows and columns.
		/// </summary>
		public Size TableSize
		{
			get
			{
				Size max = Size.Empty;

				foreach (TableCell cell in Cells)
				{
					max.Width = cell.TablePosition.X > max.Width ? cell.TablePosition.X : max.Width;
					max.Height = cell.TablePosition.Y > max.Height ? cell.TablePosition.Y : max.Height;
				};

				return max;
			}
		}

		/// <summary>
		/// Gets the width and length of the table in pixels in terms of <see cref="TableCell.CellSize"/>.
		/// <br/>
		/// <br/>
		/// The total width is determined by the widest cell in each column.<br/>
		/// The total height is determined by the tallest cell in each row.
		/// </summary>
		public SizeF TableDimensions
		{
			get
			{
				Size tableSize = TableSize;
				SizeF dimensions = SizeF.Empty;

				for (int c = 0; c <= tableSize.Width; c++)
				{
					TableColumn column = GetColumn(c);
					dimensions.Width += column.Width;
				}

				for (int r = 0; r <= tableSize.Height; r++)
				{
					TableRow row = GetRow(r);
					dimensions.Height += row.Height;
				}

				return dimensions;
			}
		}
		private ObservableCollection<TableCell> _cells;

		public TableGenerator()
		{
			Cells = new ObservableCollection<TableCell>();
			Cells.CollectionChanged += Cells_CollectionChanged;
		}

		public TableGenerator(string filename) : this()
		{
			Load(filename);
		}

		public TableGenerator(Stream csvStream) : this()
		{
			Load(csvStream);
		}

		/// <summary>
		/// Updates each <see cref="TableRow"/> and <see cref="TableColumn"/> object linked to this table as well as checking if the newly added cells
		/// </summary>
		/// <exception cref="TableMismatchException">Thrown when any newly added items to the list <see cref="Cells"/> belongs to the wrong table.</exception>
		private void Cells_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			// Check that the new cells belong to this table.
			if (e.Action == NotifyCollectionChangedAction.Add || e.Action == NotifyCollectionChangedAction.Replace)
			{
				foreach (object item in e.NewItems)
				{
					TableCell cell = (TableCell)item;

					if (!cell.Parent.Equals(this))
					{
						throw new TableMismatchException(cell, this);
					}
				}
			}			

			// Updates each linked TableRow and TableColumn linked to the table. This will notify each TableRow and TableColumn object to call their own Refresh() methods. (Provided in the interface ITableCollection interface).
			TableStructureChanged_Event(null, null);
		}

		/// <summary>
		/// Loads data from a CSV file.
		/// </summary>
		/// <param name="filename">The path of the CSV file.</param>
		public void Load(string filename)
		{
			using (FileStream fs = File.OpenRead(filename))
			{
				Load(fs);
			}
		}

		/// <summary>
		/// Loads data from a stream containing CSV data.
		/// </summary>
		/// <param name="csvStream">The stream to load from.</param>
		public void Load(Stream csvStream)
		{
			using (StreamReader reader = new StreamReader(csvStream))
			{
				using (CsvParser csv = new CsvParser(reader, CultureInfo.InvariantCulture))
				{
					// Clear previous session.
					// TODO: Allow data loaded to be added onto existing data. For now just clear the table completely.
					Cells.Clear();

					List<string[]> rows = new List<string[]>();

					while (csv.Read())
					{
						// Each string in a row represents the value of one cell, give each cell a default configuration. (Default cell values are static members of TableCell.
						string[] row = csv.Record;

						for (int i = 0; i < row.Length; i++)
						{
							string item = row[i];
							Cells.Add(CreateNewCell(new Vector2I(i, csv.Row), item, new Size(80, 20)));
						}
					}
				}
			}
		}

		/// <summary>
		/// Creates a new cell located within this table, with no parameters the cell is given default values as static members of <see cref="TableCell"/>.<br/><br/>
		/// The cell has to be manually added to the table via <see cref="Cells"/>, consider using the <see cref="Load(TableCell[])"/> method to load a list of newly created cells.
		/// </summary>
		/// <returns>A new cell with the given content which has a set of default configuration values.</returns>
		public TableCell CreateNewCell(Vector2I position, string content = "") => new TableCell(this)
		{
			TablePosition = position,
			Content = content
		};

		/// <summary>
		/// Creates a new cell located within this table.<br/><br/>
		/// The cell has to be manually added to the table via <see cref="Cells"/>, consider using the <see cref="Load(TableCell[])"/> method to load a list of newly created cells.
		/// </summary>
		/// <returns>A new cell located within the table.</returns>
		public TableCell CreateNewCell(Vector2I tablePosition, string data, Size cellSize, Font font = null, Color? textBG = null, Color? BG = null) => new TableCell(this, tablePosition, data, cellSize, font, textBG, BG);

		/// <summary>
		/// Loads a list of created cells.
		/// </summary>
		/// <param name="cells"></param>
		public void Load(TableCell[] cells)
		{
			Cells = new ObservableCollection<TableCell>(cells);
		}
		
		/// <summary>
		/// Gets a collection of cells apart of this table where the cells are on row <paramref name="index"/>.<br/>
		/// The <see cref="TableRow.Cells"/> list is a read only collection of cells from this table which are on the row specified by <see cref="TableRow.RowNumber"/>, (set to <paramref name="index"/>).<br/><br/>
		/// 
		/// Whenever the table has been changed (cells added, removed or positions modified) every <see cref="TableColumn"/> linked to this table will be updated.<br/><br/>
		/// This is useful as if you want to set a universal property for every cell on a row (such as <see cref="TableCell.BG"/>) you can set <see cref="TableRow.RowBG"/> which will update each cell's BG property on that row.
		/// </summary>
		/// <param name="index">The row to retrieve.</param>
		/// <returns>An iterable collection of cells where each cell is on the row according to <see cref="TableRow.RowNumber"/></returns>
		public TableRow GetRow(int index) => TableRow.FromTable(this, index);

		/// <summary>
		/// Gets a collection of cells apart of this table where the cells are in column <paramref name="index"/>.<br/>
		/// The <see cref="TableColumn.Cells"/> list is a read only collection of cells from this table which are in the column specified by <see cref="TableColumn.ColumnNumber"/>, (set to <paramref name="index"/>).<br/><br/>
		/// 
		/// Whenever the table has been changed (cells added, removed or positions modified) every <see cref="TableColumn"/> linked to this table will be updated.<br/><br/>
		/// This is useful as if you want to set a universal property for every cell in a column (such as <see cref="TableCell.CellSize"/> width) you can set <see cref="TableColumn.Width"/> which will update each cell's width to be the value provided.
		/// </summary>
		/// <param name="index">The column to retrieve</param>
		/// <returns>An iterable collection of cells where each cell is in the column according to <see cref="TableColumn.RowNumber"/></returns>
		public TableColumn GetColumn(int index) => TableColumn.FromTable(this, index);

		/// <summary>
		/// Removes missing spaces in the table. If there are gaps within the table, empty cells will be added so that there is a visible cell at each position.
		/// </summary>
		/// <returns>A list which contains all of the newly added cells.</returns>
		public IList<TableCell> FillMissingGaps()
		{
			// This will fill in all spaces between column 0 to column at tableSize.X and row 0 to row at tableSize.Y.
			Size tableSize = TableSize;
			List<TableCell> addedCells = new List<TableCell>();

			for (int c = 0; c <= tableSize.Width; c++)
			{
				TableColumn column = GetColumn(c);

				// If the column has no missing spaces, the loop can be skipped to the next column.
				if (column.Count() == tableSize.Height)
				{
					continue;
				}

				for (int r = 0; r <= tableSize.Height; r++)
				{
					// Check if a cell exists in row r.
					TableCell cell = column[r];

					// Important to remember: indexing an ITableCollection will not throw an IndexOutOfRangeException, it will only return null if none was found.
					if (cell == null)
					{
						TableCell fillerCell = new TableCell(this)
						{
							TablePosition = new Vector2I(c, r),
							CellSize = new Size(column.Width, GetRow(r).Height),
							BG = Color.Transparent,
							TextBG = Color.Black,
							Content = string.Empty
						};
						Cells.Add(fillerCell);
						addedCells.Add(fillerCell);
					}
				}
			}

			return addedCells;
		}

		/// <summary>
		/// Resizes each column to fit the content of each cell. Calling this method will ensure that all the content is not cut off/overlapping other cells.
		/// </summary>
		/// <param name="overflow">How many extra pixels each column should be extended by.</param>
		public void ExpandContentToColumns(int overflow = 5)
		{
			// TODO: Make an equivelant for rows. (When alignment features are added.)
			Graphics graphics = Graphics.FromImage(new Bitmap(1, 1));
			Size tableSize = TableSize;

			for (int c = 0; c <= tableSize.Width; c++)
			{
				TableColumn column = GetColumn(c);
				float maxWidth = 0;
				column.Select(x => graphics.MeasureString(x.Content.ToString(), x.Font).Width).ForEach(x => maxWidth = x > maxWidth ? x : maxWidth);

				column.Width = (int)maxWidth + overflow;
			}
		}

		/// <summary>
		/// Produces an image of the table in full. Returns a <see cref="Bitmap"/> object which can then be used for any other purpose such as saving directly to a file or overlapping to an existing image.
		/// </summary>
		/// <returns>A bitmap object which is the table visualized as an image.</returns>
		public Bitmap ExportTable()
		{
			Size tableSize = TableSize;
			SizeF tableDimensions = TableDimensions;

			TableRow topRow = GetRow(0);
			TableRow bottomRow = GetRow(tableSize.Height);

			int[] cornerIndexes = new int[]
			{
				topRow.Cells.FindIndex(x => x.TablePosition.X == 0)
			};

			Bitmap image = new Bitmap((int)tableDimensions.Width + 1, (int)tableDimensions.Height + 1);
			Graphics graphics = Graphics.FromImage(image);
			graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;

			TableRow[] rows = new TableRow[TableSize.Height];

			for (int i = 0; i < rows.Length; i++)
			{
				rows[i] = GetRow(i);
			}

			int accumulatedWidth = 0;

			for (int c = 0; c <= tableSize.Width; c++)
			{
				TableColumn column = GetColumn(c);

				foreach (TableCell cell in column)
				{
					int accumulatedHeight = 0;

					for (int r = 0; r < cell.TablePosition.Y; r++)
					{
						accumulatedHeight += rows[r].Height;
					}

					Bounds corners = new Bounds(0);
					Point cellCoordinates = new Point(accumulatedWidth, accumulatedHeight);

					if (cell.TablePosition.X == 0)
					{
						if (cell.TablePosition.Y == 0)
						{
							corners.TopLeft = (int)CornerRadius;
						}
						if (cell.TablePosition.Y == tableSize.Height)
						{
							corners.BottomLeft = (int)CornerRadius;
						}
					}

					if (cell.TablePosition.X == tableSize.Width)
					{
						if (cell.TablePosition.Y == 0)
						{
							corners.TopRight = (int)CornerRadius;
						}

						if (cell.TablePosition.Y == tableSize.Height)
						{
							corners.BottomRight = (int)CornerRadius;
						}
					}

					Rectangle cellBounds = new Rectangle(cellCoordinates, cell.CellSize);
					graphics.DrawRoundedBox(cellBounds, cell.BG, corners);

					SizeF size = graphics.MeasureString(cell.Content, cell.Font);

					RectangleF position = new RectangleF()
					{
						X = cellBounds.X,
						Y = cellBounds.Y + ((cell.CellSize.Height / 2) - (size.Height / 2)),
						Width = cellBounds.Width,
						Height = cellBounds.Height
					};

					graphics.DrawString(cell.Content, cell.Font, new SolidBrush(cell.TextBG), position);

					accumulatedHeight += cell.CellSize.Height;
				}

				accumulatedWidth += column.Width;
			}

			return image;
		}

		/// <summary>
		/// Returns the cell according to <paramref name="position"/>.<br/><br/>
		/// 
		/// When setting the value at the index, the cell at <paramref name="position"/> will be replaced with the value given, otherwise, it is just inserted into the table. 
		/// </summary>
		/// <param name="position">The position of the desired cell.</param>
		/// <returns>A cell at position <paramref name="position"/>.</returns>
		public TableCell this[Vector2I position]
		{
			get => Cells.ElementAt(Cells.FindIndex(x => x.TablePosition == position));
			set
			{
				// Indexing will directly replace a cell if there is already a cell at the position, otherwise, just insert it as normal.
				int index = Cells.FindIndex(x => x.TablePosition == position);

				if (index == -1)
				{
					Cells.Add(value);
				}
				else
				{
					Cells[index] = value;
				}
			}
		}

		/// <summary>
		/// Goes through each object of <typeparamref name="T"/> and adds a new row according to properties specified in <paramref name="order"/>.
		/// Uses reflection.<br/><br/>
		/// 
		/// You can specify what properties to retrieve and the order to retrieve them by <paramref name="order"/> parameter.<br/><br/>
		/// E.g. A record with properties ID, Name, Phone Number and Address where the properties ID, Name and PhoneNumber are desired can be retrieved by setting order to <code>"ID.Name.PhoneNumber"</code><br/><br/>
		/// 
		/// This will retrieve the ID, Name and PhoneNumber properties from each object and add them as a row.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="objects">The collection of data to add which is of type <typeparamref name="T"/></param>
		/// <param name="order">The objects to retrieve and in what order separated by dots.<br/></param>
		/// <param name="startAt">The top left position where the newly added cells from the data should be added to.</param> 
		public void SerializeFromObjects<T>(ICollection<T> objects, string order, Vector2I startAt)
		{
			string[] properties = order.Split('.');
			Type objectType = typeof(T);

			List<TableCell> cells = new List<TableCell>();

			for (int r = startAt.Y; r < objects.Count; r++)
			{
				T item = objects.ElementAt(r);
				int column = 0;

				foreach (string property in properties)
				{
					string value = objectType.GetProperty(property).GetValue(item, null).ToString();

					TableCell cell = new TableCell(this)
					{
						TablePosition = new Vector2I(column++, r),
						Content = value
					};

					cells.Add(cell);
				}
			}

			foreach (TableCell cell in cells)
			{
				Cells.Add(cell);
			}
		}

		/// <summary>
		/// Updates each <see cref="TableRow"/> and <see cref="TableColumn"/> by their .Refresh() methods (provided in <see cref="ITableCollection.Refresh()"/>)
		/// </summary>
		/// <param name="cellChanged"></param>
		/// <param name="e"></param>
		public void TableStructureChanged_Event(TableCell cellChanged, TableStructureChangedEventArgs e)
		{
			TableCollectionInvalidated?.Invoke();
		}
	}
}
