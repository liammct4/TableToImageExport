using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace TableToImageExport.TableContent
{
	/// <summary>
	/// Represents a type of content which will render some form of text/string data.
	/// </summary>
	public interface ITextContent : ITableContent
	{
		/// <summary>
		/// Default Property: Used on properties <see cref="TextContent.Font"/> and <see cref="DateContent.Font"/> when no value is provided.
		/// </summary>
		public static Font DefaultFont = new(SystemFonts.Get("Times New Roman"), 15);
		/// <summary>
		/// Default Property: Used on properties <see cref="TextContent.TextBG"/> and <see cref="DateContent.TextBG"/> when no value is provided.
		/// </summary>
		public static Color DefaultTextBG = new Argb32(0, 0, 0);
		/// <summary>
		/// The font which this content's text will be written in. Includes font family, size and style.
		/// </summary>
		public Font Font { get; set; }
		/// <summary>
		/// The colour which this content's text will be written in.
		/// </summary>
		public Color TextBG { get; set; }
	}
}
