﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ImageSharp;

namespace TableToImageExport.TableContent
{
	/// <summary>
	/// Represents the content of one cell. This content type holds a string used to write plain text inside of a cell.
	/// </summary>
	public class TextContent : ITableContent
	{
		public static SKFont DefaultFont = new(SKTypeface.FromFamilyName("Times New Roman"), 15);
		public static SKColor DefaultTextBG = new(0, 0, 0);
		/// <summary>
		/// The text which this object stores.
		/// </summary>
		public string Content { get; set; }
		/// <summary>
		/// The font which will be used to draw the text onto the table, this stores both the font family and the size.
		/// </summary>
		public SKFont Font { get; set; } = DefaultFont; // TODO: Convert to ImageSharp.
		/// <summary>
		/// The colour the font will be rendered in.
		/// </summary>
		public SKColor TextBG { get; set; } = DefaultTextBG;
		/// <summary>
		/// Creates a new content object with the specified text, equivelant to setting <see cref="Content"/>.
		/// </summary>
		/// <param name="content">The text to load.</param>
		public TextContent(string content = "") => Content = content;
		/// <summary>
		/// Draws the text onto the table using the specified settings <see cref="Font"/> and <see cref="TextBG"/>.
		/// </summary>
		public void WriteContent(Graphics graphics, RectangleF layout) => graphics.DrawString(Content, Font, new SolidBrush(TextBG), layout);
		/// <summary>
		/// Returns the size of the text in pixels when drawn using the specfied <see cref="Font"/> of this object.
		/// </summary>
		/// <param name="graphics"></param>
		/// <returns>The size of the text.</returns>
		public SizeF GetContentSize(Graphics graphics, Size? sizeOfCell)
		{
			if (!sizeOfCell.HasValue)
			{
				return graphics.MeasureString(Content, Font);
			}

			return graphics.MeasureString(Content, Font, sizeOfCell.Value);
		}
	}
}
