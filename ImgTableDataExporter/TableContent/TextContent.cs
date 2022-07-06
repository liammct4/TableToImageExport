using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImgTableDataExporter.TableContent
{
	/// <summary>
	/// Represents the content of one cell. This content type holds a string used to write plain text inside of a cell.
	/// </summary>
	public class TextContent : ITableContent
	{
		public static readonly Font DefaultFont = new Font("Helvetica", 15);
		public static readonly Color DefaultTextBG = Color.Black;
		/// <summary>
		/// The text which this object stores.
		/// </summary>
		public string Content { get; set; }
		/// <summary>
		/// The font which will be used to draw the text onto the table, this stores both the font family and the size.
		/// </summary>
		public Font Font { get; set; } = DefaultFont;
		/// <summary>
		/// The colour the font will be rendered in.
		/// </summary>
		public Color TextBG { get; set; } = DefaultTextBG;
		/// <summary>
		/// Creates a new content object with the specified text, equivelant to setting <see cref="Content"/>.
		/// </summary>
		/// <param name="content">The text to load.</param>
		public TextContent(string content = "") => Content = content;
		/// <summary>
		/// Draws the text onto the table using the specified settings <see cref="Font"/> and <see cref="TextBG"/>.
		/// </summary>
		public void WriteContent(Graphics graphics, Point position) => graphics.DrawString(Content, Font, new SolidBrush(TextBG), position);
		/// <summary>
		/// Returns the size of the text in pixels when drawn using the specfied <see cref="Font"/> of this object.
		/// </summary>
		/// <param name="graphics"></param>
		/// <returns>The size of the text.</returns>
		public SizeF GetContentSize(Graphics graphics) => graphics.MeasureString(Content, Font);
	}
}
