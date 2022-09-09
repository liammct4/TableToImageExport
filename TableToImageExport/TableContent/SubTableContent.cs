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
		/// The size of the table in pixels. The table will be divided into equal columns and rows according to <see cref="Table{SubTableCell}.TableSize"/>
		/// </summary>
		public Size TableDimensions { get; set; }

		/// <summary>
		/// Gets the size of the table, returns <see cref="TableSize"/>.
		/// </summary>
		/// <returns><see cref="TableDimensions"/></returns>
		public SizeF GetContentSize(Size? sizeOfCell = null)
		{
			throw new NotImplementedException();
		}

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
