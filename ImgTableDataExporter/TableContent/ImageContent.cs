using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ImgTableDataExporter.TableStructure;
using ImgTableDataExporter.Utilities;

namespace ImgTableDataExporter.TableContent
{
	/// <summary>
	/// This type of content stores and creates an image inside a cell.
	/// </summary>
	public class ImageContent : ITableContent
	{
		public Image Content
		{
			get => _content;
			set
			{
				_content = value;
				imageSize = value.Size;
			}
		}
		private Size imageSize;
		private Image _content;
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
		public void WriteContent(Graphics graphics, Point position) => graphics.DrawImage(Content, position.X, position.Y, imageSize.Width, imageSize.Height);
		/// <summary>
		/// Gets the size of the image in pixels.
		/// </summary>
		/// <param name="graphics"></param>
		/// <returns>The size of the image.</returns>
		public SizeF GetContentSize(Graphics graphics = null) => new SizeF(imageSize.Width, imageSize.Height);
		/// <summary>
		/// Changes the size of the image when rendered onto a table. This does NOT change the original size of the image in <see cref="Content"/>, this will only change the rendered size.
		/// </summary>
		/// <param name="size">The new size of the image.</param>
		public void StretchImageToSize(Size size) => imageSize = size - new Size(1, 1);
	}
}
