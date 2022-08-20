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
		/// Image resources need to be uniquely identifable, this is a set of auto generated names which are identifiers for files.
		/// </summary>
		internal static HashSet<string> AutoNames = new();
		/// <summary>
		/// The random object used to generate auto names.
		/// </summary>
		internal static Random NameRandom = new();
		/// <summary>
		/// Gets or sets the image format used when images are exported to a resource directory.
		/// </summary>
		public static string ImageFormat
		{
			get => '.' + _imageFormat;
			set
			{
				if (value is null)
				{
					_imageFormat = "png";
				}
				else
				{
					_imageFormat = value.Replace(".", "").ToLowerInvariant();
				}
			}
		}
		private static string _imageFormat;
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
		public void WriteContentToImage(IImageProcessingContext graphics, RectangleF position)
		{
			Image resizedClone = Content.Clone(i => i.Resize(imageSize.Width, imageSize.Height));
			graphics.DrawImage(resizedClone, new Point((int)position.X, (int)position.Y), 1);
		}
		/// <summary>
		/// Creates a html snippet of an image. The image source will be located in <paramref name="resourcePath"/>.
		/// </summary>
		/// <param name="resourcePath">The folder location for resources.</param>
		/// <returns>A html snippet string.</returns>
		public string WriteContentToHtml(string resourcePath = null)
		{
			if (!Directory.Exists(resourcePath))
			{
				throw new DirectoryNotFoundException($"While trying to add the image resources to a directory, the directory {resourcePath} does not exist.");
			}

			string uniqueName = GenerateRandomName(ImageFormat);
			string path = System.IO.Path.Combine(resourcePath, uniqueName);

			Content.Save(path);

			return $"<img width={imageSize.Width} height={imageSize.Height} src={path}/>";
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
		
		/// <summary>
		/// Generates a unique random name for image resources.
		/// </summary>
		/// <returns>A unique random name containing letters and characters.</returns>
		internal static string GenerateRandomName(string suffix = "")
		{
			const int NAME_LENGTH = 12;
			const string characterRange = "QWERTYUIOPASDFGHJKLZXCVBNMqwertyuiopasdfghjklzxcvbnm1234567890";

			string randomName;

			do
			{
				char[] numbers = new char[NAME_LENGTH];

				for (int i = 0; i < NAME_LENGTH; i++)
				{
					int randomChar = NameRandom.Next(0, characterRange.Length);
					numbers[i] = characterRange[randomChar];
				}

				randomName = new string(numbers) + suffix;
			}
			while (AutoNames.Contains(randomName)); // Name HAVE to be random, so check it isn't in the list of auto generated names.

			// Make sure the same name cannot be generated again.
			AutoNames.Add(randomName);

			return randomName;
		}
	}
}
