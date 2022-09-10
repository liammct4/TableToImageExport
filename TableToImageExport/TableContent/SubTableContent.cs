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
			int columnCount = Math.Abs(TableArea.Left - tableSize.Right) + 1;
			int rowCount = Math.Abs(tableSize.Top - tableSize.Bottom) + 1;

			int columnWidth, rowHeight;

			if (AutoSize)
			{
				columnWidth = ((int)position.Width / columnCount) + (1);
				rowHeight = ((int)position.Height / rowCount) + (1);
			}
			else
			{
				columnWidth = TableSize.Width / columnCount;
				rowHeight = TableSize.Height / rowCount;
			}

			foreach (SubTableCell cell in Cells)
			{
				int columnMultiplier = cell.TablePosition.X - tableSize.Left;
				int rowMultiplier = cell.TablePosition.Y - tableSize.Top;

				Rectangle cellArea = new()
				{
					X = (int)(position.X + (columnMultiplier * columnWidth)) - (1 * columnMultiplier),
					Y = (int)(position.Y + (rowMultiplier * rowHeight)) - (1 * rowMultiplier),
					Width = columnWidth,
					Height = rowHeight
				};

				graphics.DrawRoundedBox(cellArea, new Argb32(0, 0, 0, 0), new Bounds(0), BorderColour);
				cell.Content.WriteContentToImage(graphics, cellArea);
			}
		}
	}
}
