using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.Drawing;
using TableToImageExport.Utilities;
using SixLabors.ImageSharp.PixelFormats;

namespace TableToImageExport.TableContent
{
	/// <summary>
	/// Represents the content of one cell. This content type holds a <see cref="DateTime"/> object.
	/// </summary>
	public class DateContent : ITableContent, ITextContent
	{
		/// <summary>
		/// Default Property: Used on property <see cref="OutputFormat"/> when no value is provided.
		/// </summary>
		public static string DefaultDateFormat = "d";
		/// <summary>
		/// Default Property: Used on property <see cref="Culture"/> when no value is provided.
		/// </summary>
		public static CultureInfo DefaultCulture = CultureInfo.CurrentCulture;
		/// <summary>
		/// The time which this object stores, will be written in the standard time format according to the culture <see cref="Culture"/>.
		/// </summary>
		public DateTime Content { get; set; }
		/// <summary>
		/// How the date will be represented, by default it is "d" (<see cref="DefaultDateFormat"/>) which will produce DD/MM/YYYY (order depends on <see cref="DefaultCulture"/>) refer to https://docs.microsoft.com/en-us/dotnet/api/system.datetime.tostring?view=net-6.0 to find out how dates can be formatted. 
		/// </summary>
		public string OutputFormat { get; set; } = DefaultDateFormat;
		/// <summary>
		/// The culture/region which the date will be formatted by, en-US for example is: MM/DD/YYYY and de-DE is: DD.MM.YYYY.
		/// </summary>
		public CultureInfo Culture { get; set; } = DefaultCulture;
		/// <summary>
		/// The font which will be used to write the date onto the table, this stores both the font family and the size.
		/// </summary>
		public Font Font { get; set; } = ITextContent.DefaultFont;
		/// <summary>
		/// The colour the font will be rendered in.
		/// </summary>
		public Color TextBG { get; set; } = ITextContent.DefaultTextBG;
		/// <summary>
		/// Creates a new content object with the specified date, equivelant to setting <see cref="Content"/>.
		/// </summary>
		/// <param name="content">The text to load.</param>
		public DateContent(DateTime content = new DateTime()) => Content = content;
		/// <summary>
		/// Writes the date onto the table using the specified settings <see cref="Font"/> and <see cref="TextBG"/>.
		/// </summary>
		public void WriteContentToImage(IImageProcessingContext graphics, RectangleF layout)
		{
			TextOptions options = new(Font)
			{
				WrappingLength = layout.Width,
				Origin = new PointF(layout.Left, layout.Top)
			};

			graphics.DrawText(options, Content.ToString(OutputFormat, Culture), TextBG);
		}
		/// <summary>
		/// Creates a html snippet with this date with the specified settings <see cref="Font"/> and <see cref="TextBG"/>.
		/// </summary>
		/// <param name="resourcePath">The folder location for resources.</param>
		/// <returns>A html snippet string.</returns>
		public string WriteContentToHtml(string resourcePath = null)
		{
			Argb32 colour = TextBG;
			return $"<p style=\"font-family: {Font.Name}; font-size: {Font.Size}px; color: rgb({colour.R}, {colour.G}, {colour.B});\">{Content.ToString(OutputFormat, Culture)}</p>";
		}
		/// <summary>
		/// Returns the size of the text in pixels when drawn using the specfied <see cref="Font"/> of this object.
		/// </summary>
		/// <param name="graphics"></param>
		/// <returns>The size of the text when the date is written.</returns>
		public SizeF GetContentSize(Size? sizeOfCell)
		{
			TextOptions options = new(Font);

			if (sizeOfCell is not null)
			{
				options.WrappingLength = sizeOfCell.Value.Width;
			}

			return TextMeasurer.Measure(Content.ToString(OutputFormat, Culture), options).Size();
		}
	}
}
