using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TableToImageExport.DataStructures;
using TableToImageExport.ImageData;
using TableToImageExport.TableStructure;

namespace TableToImageExport.TableContent
{
	/// <summary>
	/// Content type for sub tables within a cell.
	/// </summary>
	public class SubTableContent : Table<SubTableCell>, ITableContent
	{
		/// <summary>
		/// The size of the table in pixels. The table will be divided into equal columns and rows according to <see cref="Table{SubTableCell}.TableArea"/>.
		/// 
		/// This will be ignored if <see cref="AutoSize"/> is set to <see langword="true"/>.
		/// </summary>
		public Size TableSize { get; set; }

		/// <summary>
		/// Determines whether the table will automatically scale to the size of the cell when exporting, if <see langword="true"/>, <see cref="TableSize"/> will be ignored.
		/// </summary>
		public bool AutoSize { get; set; }

		/// <summary>
		/// Creates a new sub table which will be autoscaled to the size of the parent cell when exporting.
		/// 
		/// Recommended over manually setting the table dimensions to the parent cell size.
		/// </summary>
		public SubTableContent()
		{
			AutoSize = true;
		}

		/// <summary>
		/// Creates a new sub table which will be the size of <paramref name="parentCell"/>.CellSize.
		/// </summary>
		/// <param name="parentCell">The parent cell of this new table.</param>
		public SubTableContent(TableCell parentCell)
		{
			TableSize = parentCell.CellSize;
			AutoSize = false;
		}

		/// <summary>
		/// Creates a new sub table. This constructor is recommended if you want to manually set the size of the table.
		/// </summary>
		/// <param name="tableDimensions">The size of the table in pixels.</param>
		public SubTableContent(Size tableDimensions)
		{
			TableSize = tableDimensions;
			AutoSize = false;
		}

		/// <summary>
		/// Gets the size of the table, returns <see cref="TableSize"/> if <see cref="AutoSize"/> is <see langword="false"/>, otherwise <paramref name="sizeOfCell"/>.
		/// <br/>
		/// <br/>
		/// If <paramref name="sizeOfCell"/> is <see langword="null"/>, <see cref="TableSize"/> is returned instead.
		/// </summary>
		/// <returns><paramref name="sizeOfCell"/> if <see cref="AutoSize"/> is <see langword="true"/>, otherwise <see cref="TableSize"/>.</returns>
		public SizeF GetContentSize(Size? sizeOfCell = null) => AutoSize ? sizeOfCell.GetValueOrDefault(TableSize) : TableSize;

		/// <summary>
		/// Creates a html snippet of this table.
		/// </summary>
		/// <param name="resourcePath">The folder path which will store resources necessary such as images.</param>
		/// <returns>A html snippet of this table.</returns>
		public string WriteContentToHtml(string resourcePath)
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// Draws this table onto a cell according to <see cref="TableSize"/>.
		/// </summary>
		/// <param name="graphics">The image to draw on.</param>
		/// <param name="position">The position where the table will start at.</param>
		public void WriteContentToImage(IImageProcessingContext graphics, RectangleF position)
		{
			if (!ValidateCells())
			{
				throw new InvalidOperationException(ERR_UNINITIALIZED_CELL_MESSAGE);
			}

			Section tableSize = TableArea;
			int columnCount = Math.Abs(tableSize.Left - tableSize.Right) + 1;
			int rowCount = Math.Abs(tableSize.Top - tableSize.Bottom) + 1;
			int tableRowCount = tableSize.Bottom - tableSize.Top;
			int tableColumnCount = tableSize.Right - tableSize.Left;

			RectangleF subTableArea = new()
			{
				X = position.X,
				Y = position.Y,
				Width = AutoSize ? position.Width - 1 : TableSize.Width,
				Height = AutoSize ? position.Height - 1 : TableSize.Height
			};

			int baseColumnWidth = (int)(subTableArea.Width / columnCount);
			int baseRowHeight = (int)(subTableArea.Height / rowCount);

			// Sort the cells into a 2d array.
			List<List<SubTableCell>> rowCols = new List<List<SubTableCell>>();

			for (int r = tableSize.Top; r <= tableSize.Bottom; r++)
			{
				rowCols.Add(new List<SubTableCell>());
			}

			foreach (SubTableCell cell in Cells)
			{
				rowCols[cell.TablePosition.Y - tableSize.Top].Add(cell);
			}

			int accumulatedHeight = 0;

			for (int r = 0; r < rowCols.Count; r++)
			{
				int accumulatedWidth = 0;
				float remainingPixelHeight = subTableArea.Height - accumulatedHeight;
				int remainingRowCount = tableRowCount - (r - 1);
				int rowHeight = (int)(remainingPixelHeight / remainingRowCount);

				if (rowHeight == int.MinValue)
				{
					rowHeight = baseRowHeight;
				}

				for (int c = 0; c < rowCols[r].Count; c++)
				{
					SubTableCell cell = rowCols[r][c];
					float remainingPixelWidth = (subTableArea.Width - accumulatedWidth);
					float remainingColumnCount = (tableColumnCount - (c - 1));
					int columnWidth = (int)(remainingPixelWidth / remainingColumnCount);

					if (columnWidth == int.MinValue)
					{
						columnWidth = baseColumnWidth;
					}

					Rectangle cellArea = new()
					{
						X = (int)(subTableArea.X + accumulatedWidth),
						Y = (int)(subTableArea.Y + accumulatedHeight),
						Width = columnWidth + 1,
						Height = rowHeight + 1
					};

					graphics.DrawRoundedBox(cellArea, new Argb32(0, 0, 0, 0), new Bounds(0), BorderColour);
					cell.Content.WriteContentToImage(graphics, cellArea);

					accumulatedWidth += columnWidth;
				};

				accumulatedHeight += rowHeight;
			}
		}
	}
}
