using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ImgTableDataExporter.Utilities;

namespace ImgTableDataExporter.TableContent
{
	/// <summary>
	/// This type of content stores and creates an image inside a cell.
	/// </summary>
	public class ImageContent : ITableContent
	{
		public Image Content { get; set; }
		/// <summary>
		/// Loads an image. Equivelant to setting <see cref="Content"/>.
		/// </summary>
		public ImageContent(Image image = null) => Content = image;
		/// <summary>
		/// Loads an image from an image file.
		/// </summary>
		/// <param name="filename">The location of the file.</param>
		public ImageContent(string filename) => Content = Image.FromFile(filename);
		/// <summary>
		/// Draws an image onto a table at the specified position.
		/// </summary>
		public void WriteContent(Graphics graphics, RectangleF position) => graphics.DrawImage(Content, position.TopLeftPoint());
		/// <summary>
		/// Gets the size of the image in pixels.
		/// </summary>
		/// <param name="graphics"></param>
		/// <returns>The size of the image.</returns>
		public SizeF GetContentSize(Graphics graphics = null) => Content.Size;
	}
}
