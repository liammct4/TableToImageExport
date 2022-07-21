using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TableToImageExport.TableStructure;
using TableToImageExport.Utilities;

namespace TableToImageExport.TableContent
{
	/// <summary>
	/// This type of content stores and creates an image inside a cell.
	/// </summary>
	public class ImageContent : ITableContent
	{
		/// <summary>
		/// The image which this object stores.
		/// </summary>
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
		public void WriteContent(Graphics graphics, RectangleF position) => graphics.DrawImage(Content, position.X + 1, position.Y + 1, imageSize.Width, imageSize.Height); // TODO: Convert to ImageSharp.
		/// <summary>
		/// Gets the size of the image in pixels.
		/// </summary>
		/// <returns>The size of the image.</returns>
		public SizeF GetContentSize(Graphics graphics = null, Size? sizeOfCell = null) => imageSize; // TODO: Convert to ImageSharp.
		/// <summary>
		/// Changes the size of the image when rendered onto a table. This does NOT change the original size of the image in <see cref="Content"/>, this will only change the rendered size.
		/// </summary>
		/// <param name="size">The new size of the image.</param>
		public void StretchImageToSize(Size size) => imageSize = size - new Size(1, 1);
		/// <summary>
		/// Resizes the image to the specified width while still keeping the original aspect ratio. This does NOT change the original size of the image in <see cref="Content"/>, this will only change the rendered size.
		/// </summary>
		/// <param name="width">The new width the image will be rendered as.</param>
		public void ScaleImageToWidth(int width) => imageSize = new Size(width, (int)((width / (double)imageSize.Width) * imageSize.Height));
		/// <summary>
		/// Resizes the image to the specified height while still keeping the original aspect ratio. This does NOT change the original size of the image in <see cref="Content"/>, this will only change the rendered size.
		/// </summary>
		/// <param name="height">The new height the image will be rendered as.</param>
		public void ScaleImageToHeight(int height) => imageSize = new Size((int)((height / (double)imageSize.Height) * imageSize.Width), height);
	}
}
