using System;
using System.Collections.Generic;
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
		public ObservableCollection<TableCell> Cells
		{
			get => _cells;
			set
			{
				if (value is null)
				{
					throw new ArgumentNullException("The table cannot be set to null.");
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

		private void Cells_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
		{
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
		/// 
		/// </summary>
		/// <param name="cells"></param>
		public void Load(TableCell[] cells)
		{
			Cells = new ObservableCollection<TableCell>(cells);
		}
		
		public TableRow GetRow(int index) => TableRow.FromTable(this, index);
		public TableColumn GetColumn(int index) => TableColumn.FromTable(this, index);

		public void ExpandGaps()
		{
			Size tableSize = TableSize;

			for (int c = 0; c <= tableSize.Width; c++)
			{
				TableColumn column = GetColumn(c);

				if (column.Count() == tableSize.Height)
				{
					continue;
				}

				for (int r = 0; r <= tableSize.Height; r++)
				{
					int index = column.ToList().FindIndex(x => x.TablePosition.Y == r);

					if (index == -1)
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
					}
				}
			}
		}

		public void ExpandContentToColumns(int overflow = 5)
		{
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

		public TableCell this[Vector2I position]
		{
			get => Cells.ElementAt(Cells.FindIndex(x => x.TablePosition == position));
		}

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

		public void TableStructureChanged_Event(TableCell cellChanged, TableStructureChangedEventArgs e)
		{
			TableCollectionInvalidated?.Invoke();
		}
	}
}
