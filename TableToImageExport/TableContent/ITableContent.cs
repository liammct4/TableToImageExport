using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
		void WriteContent(Graphics graphics, RectangleF position); // TODO: Convert to ImageSharp.
		/// <summary>
		/// Gets the size of the content item in pixels.
		/// </summary>
		/// <param name="graphics">Needed (depending on content type) to measure the size of the content.</param>
		/// <returns>The size of the content in pixels.</returns>
		SizeF GetContentSize(Graphics graphics = null, Size? sizeOfCell = null); // TODO: Convert to ImageSharp.
	}
}
