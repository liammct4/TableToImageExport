﻿using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;
using System.Diagnostics;
using System.Collections.ObjectModel;
using System.IO;
using TableToImageExport;
using TableToImageExport.DataStructures;
using TableToImageExport.ImageData;
using TableToImageExport.TableStructure;
using TableToImageExport.Utilities;
using TableToImageExport.TableContent;
using TableToImageExport.TableContent.ContentStructure;
using SixLabors.ImageSharp.Drawing;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Drawing.Processing;

namespace TableToImageExport
{
	/// <summary>
	/// Class for manipulating tablular data and producing images. Can also process CSV and TSV data.
	/// </summary>
	public class TableGenerator
	{
		/// <summary>
		/// Default Property: Used on property <see cref="CornerRadius"/> when no value is provided.
		/// </summary>
		public static uint DefaultCornerRadius = 8;
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
		public ObservableCollection<TableCell> Cells
		{
			get => _cells;
			set
			{
				if (value is null)
				{
					throw new ArgumentNullException(nameof(Cells), "The table cannot be set to null.");
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
				_cells.CollectionChanged += Cells_CollectionChanged;
				TableCollectionInvalidated?.Invoke();
			}
		}

		/// <summary>
		/// When exporting the table, this will determine how rounded the corners will be, set to 0 to get a square table/grid and to any positive 
		/// </summary>
		public uint CornerRadius { get; set; } = DefaultCornerRadius;
		/// <summary>
		/// When exporting the table, this will determine the colour of the borders of each cell.
		/// </summary>
		public Color BorderColour { get; set; } = DefaultBorderColour;
		/// <summary>
		/// When the table structure has changed, this event will be invoked which will update all <see cref="ITableCollection"/> objects.
		/// </summary>
		public event Action TableCollectionInvalidated;
		/// <summary>
		/// Gets the width and length of the table in terms of the number of rows and columns.
		/// </summary>
		public Section TableSize
		{
			get
			{
				Vector2I min = new(int.MaxValue, int.MaxValue);
				Vector2I max = new(0, 0);

				foreach (TableCell cell in Cells)
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
				Section tableSize = TableSize;
				SizeF dimensions = SizeF.Empty;

				for (int c = tableSize.Left; c <= tableSize.Right; c++)
				{
					TableColumn column = GetColumn(c);
					dimensions.Width += column.Width - 1;

					// The column collection only needs to be used once so dispose when done.
					column.Dispose();
				}

				for (int r = tableSize.Top; r <= tableSize.Bottom; r++)
				{
					TableRow row = GetRow(r);
					dimensions.Height += row.Height - 1;

					// The row collection only needs to be used once so dispose when done.
					row.Dispose();
				}

				return dimensions;
			}
		}
		private ObservableCollection<TableCell> _cells;
		private bool suppressRefresh;
		
		/// <summary>
		/// Creates a new empty table. Use the load methods or manually add data to populate this table.
		/// </summary>
		public TableGenerator()
		{
			Cells = new ObservableCollection<TableCell>();
			Cells.CollectionChanged += Cells_CollectionChanged;
		}

		/// <summary>
		/// Loads data from a CSV/TSV file located at the file path.
		/// </summary>
		public TableGenerator(string filename, DataFormats format = DataFormats.CSV) : this()
		{
			using Stream fs = File.OpenRead(filename);
			Load(fs, format);
		}

		/// <summary>
		/// Loads data from a stream containing CSV/TSV data.
		/// </summary>
		public TableGenerator(Stream dataStream, DataFormats format = DataFormats.CSV) : this()
		{
			Load(dataStream, format);
		}

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
			List<TableCell> newCells = new();
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

			Cells = new ObservableCollection<TableCell>(newCells);
		}

		/// <summary>
		/// Loads a list of created cells. This does NOT append the cells onto the existing collection of cells, this will wipe every cell and add the new cells. Use <see cref="AddInBulkCells(IEnumerable{TableCell})"/> for adding many cells.
		/// </summary>
		/// <param name="cells">The cells to load, the only cells which will be present in the table.</param>
		public void Load(ICollection<TableCell> cells)
		{
			Cells = new ObservableCollection<TableCell>(cells);
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

			List<TableCell> cells = new();

			for (int r = 0; r < objects.Count; r++)
			{
				T item = objects.ElementAt(r);
				int column = 0;

				foreach (string property in properties)
				{
					string value = objectType.GetProperty(property).GetValue(item, null).ToString();

					TableCell cell = new(this)
					{
						TablePosition = new Vector2I(column + startAt.X, r + startAt.Y),
						Content = new TextContent(value)
					};

					cells.Add(cell);
					column++;
				}
			}

			cells.AddRange(Cells);

			Cells = new ObservableCollection<TableCell>(cells);
		}

		/// <summary>
		/// Adds cells in bulk to <see cref="Cells"/>, use this instead of manually adding items to <see cref="Cells"/> for better performance.
		/// </summary>
		/// <param name="cells">The cells to add.</param>
		public void AddInBulkCells(IEnumerable<TableCell> cells)
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
		public TableCell CreateNewCell(Vector2I position, ITableContent content = null) => new(this)
		{
			TablePosition = position,
			Content = content
		};

		/// <summary>
		/// Creates a new cell located within this table, with no parameters the cell is given default values as static members of <see cref="TableCell"/>.<br/><br/>
		/// The cell has to be manually added to the table via <see cref="Cells"/>, consider using the <see cref="Load(ICollection{TableCell})"/> method to load a list of newly created cells.
		/// 
		/// This will set the content to the provided string.
		/// </summary>
		/// <returns>A new cell with the given content which has a set of default configuration values.</returns>
		public TableCell CreateNewCell(Vector2I position, string content) => CreateNewCell(position, new TextContent(content));

		/// <summary>
		/// Creates a new cell located within this table.<br/><br/>
		/// The cell has to be manually added to the table via <see cref="Cells"/>, consider using the <see cref="Load(ICollection{TableCell})"/> method to load a list of newly created cells.
		/// </summary>
		/// <returns>A new cell located within the table.</returns>
		public TableCell CreateNewCell(Vector2I tablePosition, ITableContent data, Size cellSize, ItemAlignment? contentAlignment = null, Color? BG = null) => new(this, tablePosition, data, contentAlignment, cellSize, BG);

		/// <summary>
		/// Gets a collection of cells apart of this table where the cells are on row <paramref name="rowNumber"/>.<br/>
		/// The <see cref="TableRow.Cells"/> list is a read only collection of cells from this table which are on the row specified by <see cref="TableRow.RowNumber"/>, (set to <paramref name="rowNumber"/>).<br/><br/>
		/// 
		/// Whenever the table has been changed (cells added, removed or positions modified) every <see cref="TableColumn"/> linked to this table will be updated.<br/><br/>
		/// This is useful as if you want to set a universal property for every cell on a row (such as <see cref="TableCell.BG"/>) you can set <see cref="TableRow.RowBG"/> which will update each cell's BG property on that row.
		/// </summary>
		/// <param name="rowNumber">The row to retrieve.</param>
		/// <returns>An iterable collection of cells where each cell is on the row according to <see cref="TableRow.RowNumber"/></returns>
		public TableRow GetRow(int rowNumber) => TableRow.FromTable(this, rowNumber);

		/// <summary>
		/// Gets a collection of cells apart of this table where the cells are in column <paramref name="columnNumber"/>.<br/>
		/// The <see cref="TableColumn.Cells"/> list is a read only collection of cells from this table which are in the column specified by <see cref="TableColumn.ColumnNumber"/>, (set to <paramref name="columnNumber"/>).<br/><br/>
		/// 
		/// Whenever the table has been changed (cells added, removed or positions modified) every <see cref="TableColumn"/> linked to this table will be updated.<br/><br/>
		/// This is useful as if you want to set a universal property for every cell in a column (such as <see cref="TableCell.CellSize"/> width) you can set <see cref="TableColumn.Width"/> which will update each cell's width to be the value provided.
		/// </summary>
		/// <param name="columnNumber">The column to retrieve</param>
		/// <returns>An iterable collection of cells where each cell is in the column according to <see cref="TableColumn.ColumnNumber"/></returns>
		public TableColumn GetColumn(int columnNumber) => TableColumn.FromTable(this, columnNumber);

		/// <summary>
		/// Removes missing spaces in the table. If there are gaps within the table, empty cells will be added so that there is a visible cell at each position.
		/// </summary>
		/// <returns>A list which contains all of the newly added cells.</returns>
		public ICollection<TableCell> FillMissingGaps()
		{
			// This will fill in all spaces between column 0 to column at tableSize.X and row 0 to row at tableSize.Y.
			Section tableSize = TableSize;
			List<TableCell> addedCells = new();

			for (int c = tableSize.Left; c <= tableSize.Right; c++)
			{
				TableColumn column = GetColumn(c);

				// If the column has no missing spaces, the loop can be skipped to the next column.
				if (column.Count() == tableSize.Bottom)
				{
					// If the column isn't being used anymore, make sure it is disposed of.
					column.Dispose();
					continue;
				}

				for (int r = tableSize.Top; r <= tableSize.Bottom; r++)
				{
					// Check if a cell exists in row r.
					TableCell cell = column[r];

					// Important to remember: indexing an ITableCollection will not throw an IndexOutOfRangeException, it will only return null if none was found.
					if (cell == null)
					{
						TableCell fillerCell = new(this)
						{
							TablePosition = new Vector2I(c, r),
							CellSize = new Size(column.Width, GetRow(r).Height),
							BG = Color.Transparent,
							Content = null
						};
						Cells.Add(fillerCell);
						addedCells.Add(fillerCell);
					}
				}

				// The column is now redundant and can be disposed of.
				column.Dispose();
			}

			return addedCells;
		}

		/// <summary>
		/// Resizes each column to fit the content of each cell. Calling this method will ensure that all the content is not cut off/overlapping other cells.
		/// </summary>
		/// <param name="overflow">How many extra pixels each column should be extended by.</param>
		/// <param name="minimumWidth">Ensures that each column is atleast a certain width when resized.</param>
		public void ExpandColumnsToContent(int overflow = 5, int minimumWidth = 0)
		{
			if (overflow < 0 || minimumWidth < 0)
			{
				throw new FormatException($"An argument provided contained an argument which was negative. Only positive numbers are allowed. {(overflow < 0 ? $"{nameof(overflow)} was {overflow}" : $"{nameof(minimumWidth)} was {minimumWidth}.")}");
			}

			Section tableSize = TableSize;

			for (int c = tableSize.Left; c <= tableSize.Right; c++)
			{
				TableColumn column = GetColumn(c);
				float maxWidth = 0;
				column.Select(x => x.Content.GetContentSize().Width).ForEach(x => maxWidth = x > maxWidth ? x : maxWidth);

				if (maxWidth < minimumWidth)
				{
					maxWidth = minimumWidth;
				}

				column.Width = (int)(maxWidth + overflow);

				// Remove each column used as they are now redundant.
				column.Dispose();
			}
		}

		/// <summary>
		/// Resizes each row to fit the content of each cell. Calling this method will ensure that all the content is not cut off/overlapping other cells.
		/// </summary>
		/// <param name="overflow">How many extra pixels each row should be extended by.</param>
		/// <param name="minimumHeight">Ensures that each row is atleast a certain height when resized.</param>
		public void ExpandRowsToContent(int overflow = 5, int minimumHeight = 0)
		{
			if (overflow < 0 || minimumHeight < 0)
			{
				throw new FormatException($"An argument provided contained an argument which was negative. Only positive numbers are allowed. {(overflow < 0 ? $"{nameof(overflow)} was {overflow}" : $"{nameof(minimumHeight)} was {minimumHeight}.")}");
			}

			Section tableSize = TableSize;

			for (int r = tableSize.Top; r <= tableSize.Bottom; r++)
			{
				TableRow row = GetRow(r);
				float maxHeight = 0;
				row.Select(x => x.Content.GetContentSize(new Size(x.CellSize.Width, int.MaxValue)).Height).ForEach(x => maxHeight = x > maxHeight ? x : maxHeight);

				if (maxHeight < minimumHeight)
				{
					maxHeight = minimumHeight;
				}

				row.Height = (int)maxHeight + 2 + overflow;

				// Remove each row used as they are now redundant.
				row.Dispose();
			}
		}

		/// <summary>
		/// Creates a stripe effect for every row in the table using the primary and secondary colours.
		/// </summary>
		/// <param name="primary">This will be the colour of the first, third, fifth, etc. row</param>
		/// <param name="secondary">This will be the colour of the second, fourth, sixth, etc. row</param>
		/// <param name="rowStartAt">Which row the stripe effect will start at.</param>
		public void AddStripeRibbonsToRows(Color? primary = null, Color? secondary = null, int rowStartAt = 1)
		{
			// Default options in case no colours were specified.
			primary = primary is null ? new Argb32(255, 255, 255) : primary;
			secondary = secondary is null ? new Argb32(250, 250, 255) : secondary;

			// Precache the table size.
			Section tableSize = TableSize;

			for (int i = rowStartAt; i <= tableSize.Bottom; i++)
			{
				TableRow row = GetRow(i);

				// Account for the offset of the "rowStartAt" so take it away in the calculations.
				if ((i - rowStartAt) % 2 == 0)
				{
					row.RowBG = primary.Value;
				}
				else
				{
					row.RowBG = secondary.Value;
				}

				// Remove each row used as they are now redundant.
				row.Dispose();
			}
		}

		/// <summary>
		/// Produces an image of the table in full. Returns a <see cref="Image"/> object which can then be used for any other purpose such as saving directly to a file or placing onto an existing image.
		/// </summary>
		/// <returns>A bitmap object which is the table visualized as an image.</returns>
		public Image<Argb32> ExportTableToImage()
		{
			// Precache the appropriate information about the table as these are very complicated. Use instead of directly accessing property.
			Section tableSize = TableSize;
			SizeF tableDimensions = TableDimensions;

			// Since we know the direct pixel dimensions of the table, the bitmap can be created right away and be written to using the Graphics class.
			Image<Argb32> image = new((int)tableDimensions.Width + 1, (int)tableDimensions.Height + 1);

			// Precache all the rows of the table as these only need to be got once instead of through every column iteration.
			TableRow[] rows = Enumerable.Range(0, TableSize.Bottom + 1).Select(i => GetRow(i)).ToArray();
			
			int accumulatedWidth = 0;
			for (int c = tableSize.Left; c <= tableSize.Right; c++)
			{
				TableColumn column = GetColumn(c);

				int addedRow = 0;
				foreach (TableCell cell in column)
				{
					int accumulatedHeight = 0;

					for (int rh = 0; rh < cell.TablePosition.Y; rh++)
					{
						accumulatedHeight += rows[rh].Height;
					}

					// Establish the basic information about the cell on the image used later to draw.
					Point cellPixelCoordinates = new(accumulatedWidth + 1 - c, accumulatedHeight + 1 - addedRow++);
					Rectangle cellBounds = new(cellPixelCoordinates, cell.CellSize);
					
					Bounds corners = new()
					{
						TopLeft = cell.TablePosition.X == tableSize.Left && cell.TablePosition.Y == tableSize.Top ? (int)CornerRadius : 0,
						TopRight = cell.TablePosition.X == tableSize.Right && cell.TablePosition.Y == tableSize.Top ? (int)CornerRadius : 0,
						BottomLeft = cell.TablePosition.X == tableSize.Left && cell.TablePosition.Y == tableSize.Bottom ? (int)CornerRadius : 0,
						BottomRight = cell.TablePosition.X == tableSize.Right && cell.TablePosition.Y == tableSize.Bottom ? (int)CornerRadius : 0
					};

					image.Mutate(g => g.DrawRoundedBox(cellBounds, cell.BG, corners, BorderColour));
					accumulatedHeight += cell.CellSize.Height;

					if (cell.Content is null)
					{
						continue;
					}

					// Positioning of content.
					SizeF contentSize = cell.Content.GetContentSize(cell.CellSize);
					Point relativeCellPosition = cell.ContentAlignment.Align(cell.CellSize, contentSize);
					Point contentPosition = new()
					{
						X = cellPixelCoordinates.X + relativeCellPosition.X,
						Y = cellPixelCoordinates.Y + relativeCellPosition.Y
					};

					// Now just simply draw the content onto the image.
					image.Mutate(g => cell.Content.WriteContentToImage(g, new RectangleF(contentPosition, cell.CellSize)));
				}

				accumulatedWidth += column.Width;

				// Dispose of the column now that it isn't needed anymore for performance.
				column.Dispose();
			}

			// Don't forget to release each table collection used.
			rows.ForEach(r => r.Dispose());

			return image;
		}

		/// <summary>
		/// Produces a HTML snippet of a table along with the styling. The table will be named according to <paramref name="tableClassName"/>.
		/// </summary>
		/// <param name="tableClassName">The class name which the produced table will be named as.</param>
		/// <returns>The raw html snippet.</returns>
		public string ExportTableToHtml(string tableClassName, string resourcePath, int indentLevel)
		{
			if (!Directory.Exists(resourcePath))
			{
				throw new DirectoryNotFoundException($"The resource path ({resourcePath}) provided does not exist.");
			}

			// Creates a string which is indentLevel characters long of tab separators.
			string indentBase = new(Enumerable.Range(0, indentLevel).Select(x => '\t').ToArray());
			Argb32 borderColour = BorderColour;

			string cssTableIdentifier = $".{tableClassName}";

			if (tableClassName == string.Empty)
			{
				cssTableIdentifier = "table";
			}

			string baseStyle =
				$"{indentBase}<style>\n" +
				$"{indentBase}\tth, td {{\n" +
				$"{indentBase}\t\tborder-width: 1px;\n" +
				$"{indentBase}\t\tborder-left: 0px;\n" +
				$"{indentBase}\t\tborder-top: 0px;\n" +
				$"{indentBase}\t\tborder-color: rgb({borderColour.R}, {borderColour.G}, {borderColour.B});\n" +
				$"{indentBase}\t\tborder-style: solid;\n" +
				$"{indentBase}\t\tborder-collapse: collapse;\n" +
				$"{indentBase}\t\tpadding: 0;\n" + 
				$"{indentBase}\t}}\n" +
				$"{indentBase}\t{cssTableIdentifier} {{\n" +
				$"{indentBase}\t\tborder-spacing: 0px;\n" +
				$"{indentBase}\t}}\n";
			
			StringBuilder styleSb = new(baseStyle);
			StringBuilder tableSb = new($"{indentBase}<table class=\"{tableClassName}\">\n");

			// Precache the table size since this is a time consuming operation.
			Section tableSize = TableSize;

			for (int r = tableSize.Top; r <= tableSize.Bottom; r++)
			{
				TableRow row = GetRow(r);

				tableSb.AppendLine($"{indentBase}\t<tr>");

				for (int c = 0; c < row.CellCount; c++)
				{
					TableCell cell = row[c];
					Argb32 colour = cell.BG;

					string cellStyling = $"{cell.ContentAlignment.ToString(FormatType.CSS)} width: {cell.CellSize.Width - (cell.ContentAlignment.Margin.X * 2)}px; height: {cell.CellSize.Height - (cell.ContentAlignment.Margin.Y * 2)}px; background-color: rgb({colour.R}, {colour.G}, {colour.B}); ";

					if (cell.TablePosition.X == tableSize.Left)
					{
						cellStyling += "border-left-width: 1px; ";
					}

					if (cell.TablePosition.Y == tableSize.Top)
					{
						cellStyling += "border-top-width: 1px; ";
					}

					Bounds corners = new()
					{
						TopLeft = cell.TablePosition.X == tableSize.Left && cell.TablePosition.Y == tableSize.Top ? (int)CornerRadius : 0,
						TopRight = cell.TablePosition.X == tableSize.Right && cell.TablePosition.Y == tableSize.Top ? (int)CornerRadius : 0,
						BottomLeft = cell.TablePosition.X == tableSize.Left && cell.TablePosition.Y == tableSize.Bottom ? (int)CornerRadius : 0,
						BottomRight = cell.TablePosition.X == tableSize.Right && cell.TablePosition.Y == tableSize.Bottom ? (int)CornerRadius : 0
					};
					
					if (!corners.Equals(new Bounds(0)))
					{
						cellStyling += $"border-radius: {corners.TopLeft}px {corners.TopRight}px {corners.BottomRight}px {corners.BottomLeft}px; ";
					}

					if (cell is null)
					{
						continue;
					}

					string htmlRow = $"<td style=\"{cellStyling.TrimEnd()}\">\n{indentBase}\t\t\t{cell.Content.WriteContentToHtml(resourcePath)}\n{indentBase}\t\t</td>";
					tableSb.AppendLine($"{indentBase}\t\t{htmlRow}");
				}

				tableSb.AppendLine($"{indentBase}\t</tr>");
			}

			styleSb.AppendLine($"{indentBase}</style>");
			tableSb.AppendLine($"{indentBase}</table>");
			StringBuilder joinedHtml = styleSb.Append(tableSb);

			return joinedHtml.ToString();
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

		/// <summary>
		/// Returns the cell according to the <paramref name="row"/> and <paramref name="column"/>.<br/><br/>
		/// 
		/// When setting the value at the index, the cell at <paramref name="column"/> and <paramref name="row"/> will be replaced with the value given, otherwise, it is just inserted into the table. 
		/// </summary>
		/// <param name="column">The column number.</param>
		/// <param name="row">The row number.</param>
		/// <returns>The cell in column <paramref name="column"/> and row <paramref name="row"/>.</returns>
		public TableCell this[int column, int row]
		{
			get => this[new Vector2I(column, row)];
			set => this[new Vector2I(column, row)] = value;
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

			// Unnecessarily refreshing each ITableCollection can become very performance intensive so whenever data needs to be added in bulk, wait until the data has been added then refresh.
			if (suppressRefresh)
			{
				// Updates each linked TableRow and TableColumn linked to the table. This will notify each TableRow and TableColumn object to call their own Refresh() methods. (Provided in the interface ITableCollection interface).
				TableStructureChanged_Event(null, null);
			}
		}

		/// <summary>
		/// Updates each <see cref="TableRow"/> and <see cref="TableColumn"/> by their .Refresh() methods (provided in <see cref="ITableCollection.Refresh()"/>)
		/// </summary>
		/// <param name="cellChanged"></param>
		/// <param name="e"></param>
		internal void TableStructureChanged_Event(TableCell cellChanged, TableStructureChangedEventArgs e)
		{
			TableCollectionInvalidated?.Invoke();
		}
	}
}
