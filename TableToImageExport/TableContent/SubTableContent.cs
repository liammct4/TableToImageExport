using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TableToImageExport.TableStructure;

namespace TableToImageExport.TableContent
{
	/// <summary>
	/// Content type for sub tables within a cell.
	/// </summary>
	public class SubTableContent : Table<SubTableCell>, ITableContent
	{
		/// <summary>
		/// The size of the table in pixels. The table will be divided into equal columns and rows according to <see cref="Table{SubTableCell}.TableSize"/>.
		/// 
		/// This will be ignored if <see cref="AutoSize"/> is set to <see langword="true"/>.
		/// </summary>
		public Size TableDimensions { get; set; }

		/// <summary>
		/// Determines whether the table will automatically scale to the size of the cell when exporting, if <see langword="true"/>, <see cref="TableDimensions"/> will be ignored.
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
			TableDimensions = parentCell.CellSize;
			AutoSize = false;
		}

		/// <summary>
		/// Creates a new sub table. This constructor is recommended if you want to manually set the size of the table.
		/// </summary>
		/// <param name="tableDimensions">The size of the table in pixels.</param>
		public SubTableContent(Size tableDimensions)
		{
			TableDimensions = tableDimensions;
			AutoSize = false;
		}

		/// <summary>
		/// Gets the size of the table, returns <see cref="TableDimensions"/> if <see cref="AutoSize"/> is <see langword="false"/>, otherwise <paramref name="sizeOfCell"/>.
		/// <br/>
		/// <br/>
		/// If <paramref name="sizeOfCell"/> is <see langword="null"/>, <see cref="TableDimensions"/> is returned instead.
		/// </summary>
		/// <returns><paramref name="sizeOfCell"/> if <see cref="AutoSize"/> is <see langword="true"/>, otherwise <see cref="TableDimensions"/>.</returns>
		public SizeF GetContentSize(Size? sizeOfCell = null) => AutoSize ? sizeOfCell.GetValueOrDefault(TableDimensions) : TableDimensions;

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
			throw new NotImplementedException();
		}
	}
}
