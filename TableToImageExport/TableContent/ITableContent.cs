using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Drawing.Processing;

namespace TableToImageExport.TableContent
{
	/// <summary>
	/// This interface represents one type of content located within a cell.
	/// </summary>
	public interface ITableContent
	{
		/// <summary>
		/// Draws the content onto the image at the position given.
		/// </summary>
		/// <param name="graphics">The image to draw on.</param>
		/// <param name="position">The position and area of the content.</param>
		void WriteContent(IImageProcessingContext graphics, RectangleF position);
		/// <summary>
		/// Gets the size of the content item in pixels.
		/// </summary>
		/// <param name="sizeOfCell">The size of the parent cell which this content belongs to.</param>
		/// <returns>The size of the content in pixels.</returns>
		SizeF GetContentSize(Size? sizeOfCell = null);
	}
}
