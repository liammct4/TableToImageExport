using System;
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
	public class TableGenerator : Table<TableCell>
	{
		/// <summary>
		/// Default Property: Used on property <see cref="CornerRadius"/> when no value is provided.
		/// </summary>
		public static uint DefaultCornerRadius = 8;
		/// <summary>
		/// When exporting the table, this will determine how rounded the corners will be, set to 0 to get a square table/grid and to any positive 
		/// </summary>
		public uint CornerRadius { get; set; } = DefaultCornerRadius;
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
				Section tableSize = TableArea;
				SizeF dimensions = SizeF.Empty;

				for (int c = tableSize.Left; c <= tableSize.Right; c++)
				{
					int widestCellWidth = 0;

					foreach (TableCell cell in Cells)
					{
						if (cell.TablePosition.X == c && cell.CellSize.Width > widestCellWidth)
						{
							widestCellWidth = cell.CellSize.Width;
						}
					}
					dimensions.Width += widestCellWidth - 1;
				}

				for (int r = tableSize.Top; r <= tableSize.Bottom; r++)
				{
					int tallestCellHeight = 0;

					foreach (TableCell cell in Cells)
					{
						if (cell.TablePosition.Y == r && cell.CellSize.Height > tallestCellHeight)
						{
							tallestCellHeight = cell.CellSize.Height;
						}
					}
					dimensions.Height += tallestCellHeight - 1;
				}

				return dimensions;
			}
		}

		/// <summary>
		/// Creates a new empty table. Use the load methods or manually add data to populate this table.
		/// </summary>
		public TableGenerator()
		{
		
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
		/// Gets a collection of cells apart of this table where the cells are on row <paramref name="rowNumber"/>.<br/>
		/// The <see cref="TableRow.Cells"/> list is a read only collection of cells from this table which are on the row specified by <see cref="TableRow.RowNumber"/>, (set to <paramref name="rowNumber"/>).<br/><br/>
		/// 
		/// Whenever the table has been changed (cells added, removed or positions modified) every <see cref="TableColumn"/> linked to this table will be updated.<br/><br/>
		/// This is useful as if you want to set a universal property for every cell on a row (such as <see cref="TableCell.BG"/>) you can set <see cref="TableRow.RowBG"/> which will update each cell's BG property on that row.
		/// </summary>
		/// <param name="rowNumber">The row to retrieve.</param>
		/// <exception cref="InvalidOperationException">Thrown if a cell is uninitialized.</exception>
		/// <returns>An iterable collection of cells where each cell is on the row according to <see cref="TableRow.RowNumber"/></returns>
		public TableRow GetRow(int rowNumber)
		{
			if (!ValidateCells())
			{
				throw new InvalidOperationException(ERR_UNINITIALIZED_CELL_MESSAGE);
			}

			return TableRow.FromTable(this, rowNumber);
		}

		/// <summary>
		/// Gets a collection of cells apart of this table where the cells are in column <paramref name="columnNumber"/>.<br/>
		/// The <see cref="TableColumn.Cells"/> list is a read only collection of cells from this table which are in the column specified by <see cref="TableColumn.ColumnNumber"/>, (set to <paramref name="columnNumber"/>).<br/><br/>
		/// 
		/// Whenever the table has been changed (cells added, removed or positions modified) every <see cref="TableColumn"/> linked to this table will be updated.<br/><br/>
		/// This is useful as if you want to set a universal property for every cell in a column (such as <see cref="TableCell.CellSize"/> width) you can set <see cref="TableColumn.Width"/> which will update each cell's width to be the value provided.
		/// </summary>
		/// <param name="columnNumber">The column to retrieve</param>
		/// <exception cref="InvalidOperationException">Thrown if a cell is uninitialized.</exception>
		/// <returns>An iterable collection of cells where each cell is in the column according to <see cref="TableColumn.ColumnNumber"/></returns>
		public TableColumn GetColumn(int columnNumber)
		{
			if (!ValidateCells())
			{
				throw new InvalidOperationException(ERR_UNINITIALIZED_CELL_MESSAGE);
			}

			return TableColumn.FromTable(this, columnNumber);
		}

		/// <summary>
		/// Removes missing spaces in the table. If there are gaps within the table, empty cells will be added so that there is a visible cell at each position.
		/// </summary>
		/// <returns>A list which contains all of the newly added cells.</returns>
		public ICollection<TableCell> FillMissingGaps()
		{
			// This will fill in all spaces between column 0 to column at tableSize.X and row 0 to row at tableSize.Y.
			Section tableSize = TableArea;
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
						TableCell fillerCell = new()
						{
							TablePosition = new Vector2I(c, r),
							CellSize = new Size(column.Width, GetRow(r).Height),
							BG = Color.Transparent,
							Content = null
						};

						fillerCell.Initialize(this);

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

			Section tableSize = TableArea;

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

			Section tableSize = TableArea;

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
			Section tableSize = TableArea;

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
		/// <exception cref="InvalidOperationException">Thrown if a cell is uninitialized.</exception>
		/// <returns>A bitmap object which is the table visualized as an image.</returns>
		public Image<Argb32> ExportTableToImage()
		{
			if (!ValidateCells())
			{
				throw new InvalidOperationException(ERR_UNINITIALIZED_CELL_MESSAGE);
			}

			// Precache the appropriate information about the table as these are very complicated. Use instead of directly accessing property.
			Section tableSize = TableArea;
			SizeF tableDimensions = TableDimensions;

			// Since we know the direct pixel dimensions of the table, the bitmap can be created right away and be written to using the Graphics class.
			Image<Argb32> image = new((int)tableDimensions.Width + 1, (int)tableDimensions.Height + 1);

			// Precache all the rows of the table as these only need to be got once instead of through every column iteration.
			TableRow[] rows = Enumerable.Range(0, TableArea.Bottom + 1).Select(i => GetRow(i)).ToArray();
			
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
		/// <param name="resourcePath">The folder location where resources (such as images) will be stored.</param>
		/// <param name="indentLevel">The indent level which the table will start at.</param>
		/// <exception cref="InvalidOperationException">Thrown if a cell is uninitialized.</exception>
		/// <returns>The raw html snippet.</returns>
		public string ExportTableToHtml(string tableClassName, string resourcePath, int indentLevel = 0)
		{
			if (!ValidateCells())
			{
				throw new InvalidOperationException(ERR_UNINITIALIZED_CELL_MESSAGE);
			}

			if (!Directory.Exists(resourcePath))
			{
				throw new DirectoryNotFoundException($"The resource path ({resourcePath}) provided does not exist.");
			}

			// Creates a string which is indentLevel characters long of tab separators.
			string indentBase = new(Enumerable.Range(0, indentLevel).Select(x => '\t').ToArray());
			Argb32 borderColour = BorderColour;

			string cssTableIdentifier = $".{tableClassName}";

			// Incase there is no name provided, the styling will just target a generic table.
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
			Section tableSize = TableArea;

			for (int r = tableSize.Top; r <= tableSize.Bottom; r++)
			{
				TableRow row = GetRow(r);

				tableSb.AppendLine($"{indentBase}\t<tr>");

				for (int c = 0; c < row.CellCount; c++)
				{
					TableCell cell = row[c];
					if (cell is null)
					{
						continue;
					}

					Argb32 colour = cell.BG;

					string cellStyling = $"{cell.ContentAlignment.ToString(FormatType.CSS)} width: {cell.CellSize.Width - (cell.ContentAlignment.Margin.X * 2)}px; height: {cell.CellSize.Height - (cell.ContentAlignment.Margin.Y * 2)}px; background-color: rgb({colour.R}, {colour.G}, {colour.B}); ";

					// Borders between cells do NOT collpase in of themselves, so two side's (top, left) of every cell has to be 0 width.
					// Incase the cell is the most far left or most top, the border is readded.
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

					// If the cell has corners, if not, there is no need to specify them.
					if (!corners.Equals(new Bounds(0)))
					{
						cellStyling += $"border-radius: {corners.TopLeft}px {corners.TopRight}px {corners.BottomRight}px {corners.BottomLeft}px; ";
					}

					string htmlCell = $"<td style=\"{cellStyling.TrimEnd()}\">\n{indentBase}\t\t\t{cell.Content.WriteContentToHtml(resourcePath)}\n{indentBase}\t\t</td>";
					tableSb.AppendLine($"{indentBase}\t\t{htmlCell}");
				}

				tableSb.AppendLine($"{indentBase}\t</tr>");
			}

			styleSb.AppendLine($"{indentBase}</style>");
			tableSb.AppendLine($"{indentBase}</table>");
			StringBuilder joinedHtml = styleSb.Append(tableSb);

			return joinedHtml.ToString();
		}

		/// <summary>
		/// Creates a new cell located within this table.<br/><br/>
		/// The cell has to be manually added to the table via <see cref="Table{TableCell}.Cells"/>, consider using the <see cref="Table{TableCell}.Load(ICollection{TableCell})"/> method to load a list of newly created cells.
		/// </summary>
		/// <returns>A new cell located within the table.</returns>
		public TableCell CreateNewCell(Vector2I tablePosition, ITableContent data, Size cellSize, ItemAlignment? contentAlignment = null, Color? BG = null)
		{
			TableCell newCell = new()
			{
				TablePosition = tablePosition,
				Content = data,
				CellSize = cellSize,
				ContentAlignment = contentAlignment.GetValueOrDefault(ItemAlignment.CentreLeft),
				BG = BG.GetValueOrDefault(Cell.DefaultBG)
			};

			newCell.Initialize(this);

			return newCell;
		}
	}
}
