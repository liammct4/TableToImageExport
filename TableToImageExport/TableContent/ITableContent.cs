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
		void WriteContentToImage(IImageProcessingContext graphics, RectangleF position);
		/// <summary>
		/// Produces an HTML snippet of this content.
		/// </summary>
		/// <param name="resourcePath">The path location to the folder for resources (e.g. Image Files).</param>
		/// <returns>A HTML snippet representing this content.</returns>
		string WriteContentToHtml(string resourcePath);
		/// <summary>
		/// Gets the size of the content item in pixels.
		/// </summary>
		/// <param name="sizeOfCell">The size of the parent cell which this content belongs to.</param>
		/// <returns>The size of the content in pixels.</returns>
		SizeF GetContentSize(Size? sizeOfCell = null);
	}
}
