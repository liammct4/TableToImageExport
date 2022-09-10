using CsvHelper;
using CsvHelper.Configuration;
using SixLabors.ImageSharp;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TableToImageExport.DataStructures;
using TableToImageExport.TableContent;
using TableToImageExport.TableContent.ContentStructure;
using TableToImageExport.Utilities;

namespace TableToImageExport.TableStructure
{
	/// <summary>
	/// Base class for a table which is completely agnostic regarding cells and cell types.
	/// </summary>
	public abstract class Table
	{
		/// <summary>
		/// When the table structure has changed, this event will be invoked which will update all <see cref="ITableCollection"/> objects.
		/// </summary>
		public event Action TableCollectionInvalidated;
		/// <summary>
		/// Updates each <see cref="TableRow"/> and <see cref="TableColumn"/> by their .Refresh() methods (provided in <see cref="ITableCollection.Refresh()"/>)
		/// </summary>
		/// <param name="cellChanged"></param>
		/// <param name="e"></param>
		protected internal void TableStructureChanged_Event(Cell cellChanged, TableStructureChangedEventArgs e)
		{
			TableCollectionInvalidated?.Invoke();
		}
		/// <summary>
		/// Because events cannot be invoked from their own class (even when inherited)
		/// </summary>
		protected internal void InvokeTableCollectionInvalidate()
		{
			TableCollectionInvalidated?.Invoke();
		}
	}

	/// <summary>
	/// Base class which defines a table including layout/structure.
	/// </summary>
	public abstract class Table<TCell> : Table where TCell : Cell, new()
	{
		internal const string ERR_UNINITIALIZED_CELL_MESSAGE = "A cell within this table was uninitialized, meaning that it was manually created from a constructor, cells cannot be manually created from a constructor and should instead be created by the CreateNewCell() method.";

		/// <summary>
		/// Default Property: Used on property <see cref="BorderColour"/> when no value is provided.
		/// </summary>
		public static Color DefaultBorderColour = Color.Black;

		/// <summary>
		/// Stores every cell within the table, the table is structured per cell (see <see cref="TableCell.TablePosition"/>) so the order of the list does not matter. Changes made to this list will update any <see cref="TableColumn"/> and <see cref="TableRow"/> automatically.<br/><br/>
		/// Do not add to this manually if there are many <see cref="ITableCollection"/> objects, use other methods such as <see cref="Load(ICollection{TableCell})"/> or 
		/// 
		/// Cells should only be added to this if the cell belongs to this table. (Where <see cref="TableCell.Parent"/> is this <see cref="TableGenerator"/> instance).<br/><br/>
		/// 
		/// The exception <see cref="TableMismatchException"/> is thrown where any value in the provided list contains a cell which belongs to a different table.
		/// </summary>
		public ObservableCollection<TCell> Cells
		{
			get => _cells;
			set
			{
				if (value is null)
				{
					throw new ArgumentNullException(nameof(Cells), "The table cannot be set to null.");
				}

				// Check that the cell belongs to this table.
				foreach (TCell cell in value)
				{
					if (!cell.IsInitialized)
					{
						throw new InvalidOperationException(ERR_UNINITIALIZED_CELL_MESSAGE);
					}
					else if (!cell.Parent.Equals(this))
					{
						throw new TableMismatchException<TCell>(cell, this);
					}
				}

				_cells = value;
				_cells.CollectionChanged += Cells_CollectionChanged;

				InvokeTableCollectionInvalidate();
			}
		}

		/// <summary>
		/// Creates a new empty table.
		/// </summary>
		public Table()
		{
			Cells = new ObservableCollection<TCell>();
			Cells.CollectionChanged += Cells_CollectionChanged;
		}

		/// <summary>
		/// Gets the width and length of the table in terms of the number of rows and columns.
		/// </summary>
		public Section TableArea
		{
			get
			{
				Vector2I min = new(int.MaxValue, int.MaxValue);
				Vector2I max = new(0, 0);

				foreach (TCell cell in Cells)
				{
					min.X = cell.TablePosition.X < min.X ? cell.TablePosition.X : min.X;
					min.Y = cell.TablePosition.Y < min.Y ? cell.TablePosition.Y : min.Y;

					max.X = cell.TablePosition.X > max.X ? cell.TablePosition.X : max.X;
					max.Y = cell.TablePosition.Y > max.Y ? cell.TablePosition.Y : max.Y;
				}

				return new Section()
				{
					Top = min.Y,
					Right = max.X,
					Bottom = max.Y,
					Left = min.X
				};
			}
		}

		/// <summary>
		/// When exporting the table, this will determine the colour of the borders of each cell.
		/// </summary>
		public Color BorderColour { get; set; } = DefaultBorderColour;

		/// <summary>
		/// Returns the cell according to the <paramref name="row"/> and <paramref name="column"/>.<br/><br/>
		/// 
		/// When setting the value at the index, the cell at <paramref name="column"/> and <paramref name="row"/> will be replaced with the value given, otherwise, it is just inserted into the table. 
		/// </summary>
		/// <param name="column">The column number.</param>
		/// <param name="row">The row number.</param>
		/// <returns>The cell in column <paramref name="column"/> and row <paramref name="row"/>.</returns>
		public TCell this[int column, int row]
		{
			get => this[new Vector2I(column, row)];
			set => this[new Vector2I(column, row)] = value;
		}

		/// <summary>
		/// Returns the cell according to <paramref name="position"/>.<br/><br/>
		/// 
		/// When setting the value at the index, the cell at <paramref name="position"/> will be replaced with the value given, otherwise, it is just inserted into the table. 
		/// </summary>
		/// <param name="position">The position of the desired cell.</param>
		/// <returns>A cell at position <paramref name="position"/>.</returns>
		public TCell this[Vector2I position]
		{
			get
			{
				int index = Cells.FindIndex(x => x.TablePosition == position);

				if (index == -1)
				{
					return null;
				}
				else
				{
					return Cells.ElementAt(index);
				}
			}
			set
			{
				// Indexing will directly replace a cell if there is already a cell at the position, otherwise, just insert it as normal.
				int index = Cells.FindIndex(x => x.TablePosition == position);

				value.TablePosition = position;

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
		private ObservableCollection<TCell> _cells;
		internal bool suppressRefresh;

		/// <summary>
		/// Loads data from a CSV/TSV file.
		/// </summary>
		/// <param name="filename">The path of the CSV/TSV file.</param>
		/// <param name="format">The format which the data is in, can be CSV or TSV.</param>
		public void Load(string filename, DataFormats format = DataFormats.CSV)
		{
			using FileStream fs = File.OpenRead(filename);
			Load(fs, format);
		}

		/// <summary>
		/// Loads data from a stream containing either CSV or TSV data (specified by <paramref name="format"/>).
		/// </summary>
		/// <param name="dataStream">The data to load from.</param>
		/// <param name="format">The data format which the data is stored as.</param>
		public void Load(Stream dataStream, DataFormats format = DataFormats.CSV)
		{
			using StreamReader reader = new(dataStream);
			string delimiter = ",";

			if (format is DataFormats.TSV)
			{
				delimiter = "\t";
			}

			CsvConfiguration parserConfiguration = new(CultureInfo.InvariantCulture)
			{
				Delimiter = delimiter
			};

			using CsvParser csv = new(reader, parserConfiguration);
			// TODO: Allow data loaded to be added onto existing data.
			List<TCell> newCells = new();
			while (csv.Read())
			{
				// Each string in a row represents the value of one cell, give each cell a default configuration. (Default cell values are static members of TableCell.
				string[] row = csv.Record;

				for (int i = 0; i < row.Length; i++)
				{
					string item = row[i].PerLineTrim();
					newCells.Add(CreateNewCell(new Vector2I(i, csv.Row - 1), new TextContent(item)));
				}
			}

			Cells = new ObservableCollection<TCell>(newCells);
		}

		/// <summary>
		/// Loads a list of created cells. This does NOT append the cells onto the existing collection of cells, this will wipe every cell and add the new cells. Use <see cref="AddInBulkCells(IEnumerable{TableCell})"/> for adding many cells.
		/// </summary>
		/// <param name="cells">The cells to load, the only cells which will be present in the table.</param>
		/// <exception cref="InvalidOperationException">Thrown if a cell is uninitialized.</exception>
		public void Load(ICollection<TCell> cells)
		{
			Cells = new ObservableCollection<TCell>(cells);

			if (!ValidateCells())
			{
				throw new InvalidOperationException(ERR_UNINITIALIZED_CELL_MESSAGE);
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
		public void LoadFromObjects<T>(ICollection<T> objects, string order, Vector2I startAt)
		{
			string[] properties = order.Split('.');
			Type objectType = typeof(T);

			List<TCell> cells = new();

			for (int r = 0; r < objects.Count; r++)
			{
				T item = objects.ElementAt(r);
				int column = 0;

				foreach (string property in properties)
				{
					object propertyObject = objectType.GetProperty(property).GetValue(item, null);

					ITableContent cellContent = TableContent.Utilities.GetContentFromObject(propertyObject);

					TCell cell = CreateNewCell(new Vector2I(column + startAt.X, r + startAt.Y), cellContent);

					cells.Add(cell);
					column++;
				}
			}

			cells.AddRange(Cells);

			Cells = new ObservableCollection<TCell>(cells);
		}

		/// <summary>
		/// Adds cells in bulk to <see cref="Cells"/>, use this instead of manually adding items to <see cref="Cells"/> for better performance.
		/// </summary>
		/// <param name="cells">The cells to add.</param>
		public void AddInBulkCells(IEnumerable<TCell> cells)
		{
			// Causes table collections (TableRow and TableColumn) to temporarily pause refreshing to improve performance.
			suppressRefresh = true;

			// Add each cell in bulk.
			cells.ForEach(x => Cells.Add(x));

			// Now resume as normal and refresh every ITableCollection.
			suppressRefresh = false;
			TableStructureChanged_Event(null, null);
		}

		/// <summary>
		/// Creates a new cell located within this table, with no parameters the cell is given default values as static members of <see cref="TableCell"/>.<br/><br/>
		/// The cell has to be manually added to the table via <see cref="Cells"/>, consider using the <see cref="Load(ICollection{TableCell})"/> method to load a list of newly created cells.
		/// </summary>
		/// <returns>A new cell with the given content which has a set of default configuration values.</returns>
		public TCell CreateNewCell(Vector2I position, ITableContent content = null)
		{
			TCell newCell = new TCell()
			{
				TablePosition = position,
				Content = content
			};
			
			newCell.Initialize(this);

			return newCell;
		}

		/// <summary>
		/// Creates a new cell located within this table, with no parameters the cell is given default values as static members of <typeparamref name="TCell"/> and <see cref="Cell"/>.<br/><br/>
		/// The cell has to be manually added to the table via <see cref="Cells"/>, consider using the <see cref="Load(ICollection{TCell})"/> method to load a list of newly created cells.
		/// 
		/// This will set the content to the provided string.
		/// </summary>
		/// <returns>A new cell with the given content which has a set of default configuration values.</returns>
		public TCell CreateNewCell(Vector2I position, string content) => CreateNewCell(position, new TextContent(content));

		/// <summary>
		/// Creates a new cell located within this table.<br/><br/>
		/// The cell has to be manually added to the table via <see cref="Cells"/>, consider using the <see cref="Load(ICollection{TCell})"/> method to load a list of newly created cells.
		/// </summary>
		/// <returns>A new cell located within the table.</returns>
		public TCell CreateNewCell(Vector2I tablePosition, ITableContent data, ItemAlignment? contentAlignment = null, Color? BG = null)
		{
			TCell newCell = new TCell()
			{
				TablePosition = tablePosition,
				Content = data,
				ContentAlignment = contentAlignment.GetValueOrDefault(ItemAlignment.CentreLeft),
				BG = BG.GetValueOrDefault(Cell.DefaultBG)
			};

			newCell.Initialize(this);

			return newCell;
		}

		/// <summary>
		/// Determines if every cell within the table has been initialized. Returns <see langword="true"/> if every cell has been initialized, otherwise <see langword="false"/>.
		/// </summary>
		/// <returns><see langword="true"/> if every cell has been initialized, otherwise <see langword="false"/></returns>
		protected private bool ValidateCells()
		{
			foreach (TCell cell in Cells)
			{
				if (!cell.IsInitialized)
				{
					return false;
				}
			}

			return true;
		}

		/// <summary>
		/// Updates each <see cref="TableRow"/> and <see cref="TableColumn"/> object linked to this table as well as checking if the newly added cells
		/// </summary>
		/// <exception cref="TableMismatchException{T}">Thrown when any newly added items to the list <see cref="Cells"/> belongs to the wrong table.</exception>
		/// <exception cref="InvalidOperationException">Thrown if a cell is uninitialized.</exception>
		internal void Cells_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			// Check that the new cells belong to this table.
			if (e.Action == NotifyCollectionChangedAction.Add || e.Action == NotifyCollectionChangedAction.Replace)
			{
				foreach (object item in e.NewItems)
				{
					TCell cell = (TCell)item;

					if (!cell.IsInitialized)
					{
						throw new InvalidOperationException(ERR_UNINITIALIZED_CELL_MESSAGE);
					}
					else if (!cell.Parent.Equals(this))
					{
						throw new TableMismatchException<TCell>(cell, this);
					}
				}
			}

			// Unnecessarily refreshing each ITableCollection can become very performance intensive so whenever data needs to be added in bulk, wait until the data has been added then refresh.
			if (!suppressRefresh)
			{
				// Updates each linked TableRow and TableColumn linked to the table. This will notify each TableRow and TableColumn object to call their own Refresh() methods. (Provided in the interface ITableCollection interface).
				TableStructureChanged_Event(null, null);
			}
		}
	}
}
