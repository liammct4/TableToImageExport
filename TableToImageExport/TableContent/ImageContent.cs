using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.Processing;
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
				imageSize = new Size(value.Width, value.Height);
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
		public ImageContent(string filename) => Content = Image.Load(filename);
		/// <summary>
		/// Draws an image onto a table at the specified position.
		/// </summary>
		public void WriteContent(IImageProcessingContext graphics, RectangleF position)
		{
			Image resizedClone = Content.Clone(i => i.Resize(imageSize.Width, imageSize.Height));
			graphics.DrawImage(resizedClone, new Point((int)position.X, (int)position.Y), 1);
		}
		/// <summary>
		/// Gets the size of the image in pixels.
		/// </summary>
		/// <returns>The size of the image.</returns>
		public SizeF GetContentSize(Size? sizeOfCell = null) => imageSize;
		/// <summary>
		/// Changes the size of the image when rendered onto a table. This does NOT change the original size of the image in <see cref="Content"/>, this will only change the rendered size.
		/// </summary>
		/// <param name="size">The new size of the image.</param>
		public void StretchImageToSize(Size size) => imageSize = size - new Size(2, 2);
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
