using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SixLabors.ImageSharp;
using SixLabors.Fonts;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Drawing;
using SixLabors.ImageSharp.Drawing.Processing;
using TableToImageExport.Utilities;

namespace TableToImageExport.TableContent
{
	/// <summary>
	/// Represents the content of one cell. This content type holds a string used to write plain text inside of a cell.
	/// </summary>
	public class TextContent : ITableContent, ITextContent
	{
		/// <summary>
		/// Determines the size of the space between the left or right edge when the text needs to wrap.
		/// </summary>
		internal const int EndMargin = 8;
		/// <summary>
		/// The text which this object stores.
		/// </summary>
		public string Content { get; set; }
		/// <summary>
		/// The font which will be used to draw the text onto the table, this stores both the font family and the size.
		/// </summary>
		public Font Font { get; set; } = ITextContent.DefaultFont;
		/// <summary>
		/// The colour the font will be rendered in.
		/// </summary>
		public Color TextBG { get; set; } = ITextContent.DefaultTextBG;
		/// <summary>
		/// Creates a new content object with the specified text, equivelant to setting <see cref="Content"/>.
		/// </summary>
		/// <param name="content">The text to load.</param>
		public TextContent(string content = "") => Content = content;
		/// <summary>
		/// Draws the text onto the table using the specified settings <see cref="Font"/> and <see cref="TextBG"/>.
		/// </summary>
		public void WriteContentToImage(IImageProcessingContext graphics, RectangleF layout)
		{
			TextOptions options = new(Font)
			{
				WrappingLength = layout.Width - EndMargin,
				Origin = new PointF(layout.Left, layout.Top)
			};

			graphics.DrawText(options, Content, TextBG);
		}
		/// <summary>
		/// Creates a html snippet representing this text data with the specified settings <see cref="Font"/> and <see cref="TextBG"/>.
		/// </summary>
		/// <param name="resourcePath">The folder location for resources.</param>
		/// <returns>A html snippet string.</returns>
		public string WriteContentToHtml(string resourcePath = null)
		{
			Argb32 colour = TextBG;
			return $"<p style=\"font-family: {Font.Name}; font-size: {Font.Size}px; color: rgb({colour.R}, {colour.G}, {colour.B});\">{Content}</p>";
		}
		/// <summary>
		/// Returns the size of the text in pixels when drawn using the specfied <see cref="Font"/> of this object.
		/// </summary>
		/// <param name="sizeOfCell">The size of the parent cell which this content belongs to.</param>
		/// <returns>The size of the text.</returns>
		public SizeF GetContentSize(Size? sizeOfCell)
		{
			TextOptions options = new(Font);

			if (sizeOfCell is not null)
			{
				options.WrappingLength = sizeOfCell.Value.Width;
			}

			return TextMeasurer.Measure(Content, options).Size();
		}
	}
}
