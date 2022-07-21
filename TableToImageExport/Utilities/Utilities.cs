using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;

namespace TableToImageExport.Utilities
{
	/// <summary>
	/// Provides generic utility methods for any purpose.
	/// </summary>
	public static class Utilities
	{
		private static readonly Graphics graphics;
		static Utilities()
		{
			Bitmap utilityBitmap = new Bitmap(1, 1);
			graphics = Graphics.FromImage(utilityBitmap);
		}

		// Taken from: https://stackoverflow.com/questions/1879395/how-do-i-generate-a-stream-from-a-string
		public static Stream GenerateStreamFromString(string s)
		{
			var stream = new MemoryStream();
			var writer = new StreamWriter(stream);
			writer.Write(s);
			writer.Flush();
			stream.Position = 0;
			return stream;
		}

		/// <summary>
		/// Iterates through lines within a string providing access for trimming.<br/><br/>
		/// 
		/// This has a generic acton parameter to reuse code, instead of repeating this method 3 more times for trimming the start, end or both, the action can just be a function called. 
		/// </summary>
		/// <param name="str">The string to trim.</param>
		/// <param name="trimAction">The action to be performed, this is an action to reuse code.</param>
		/// <returns>A string which has had the action <paramref name="trimAction"/> performed per line.</returns>
		private static string PerLineTrimBase(string str, Func<string, string> trimAction)
		{
			string[] lines = str.Split('\n');
			string[] trimmedLines = new string[lines.Length];

			for (int i = 0; i < lines.Length; i++)
			{
				string line = trimAction(lines[i]);

				trimmedLines[i] = line;
			}

			return string.Join(Environment.NewLine, trimmedLines);
		}

		/// <summary>
		/// Trims each line within a string.
		/// </summary>
		/// <param name="str">The string to trim.</param>
		/// <returns>A string where each line has been trimmed.</returns>
		public static string PerLineTrim(this string str) => PerLineTrimBase(str, x => x.Trim());
		/// <summary>
		/// Trims the start of each line within a string.
		/// </summary>
		/// <param name="str">The string to trim.</param>
		/// <returns>A string where the start of each line has been trimmed.</returns>
		public static string PerLineTrimStart(this string str) => PerLineTrimBase(str, x => x.TrimStart());
		/// <summary>
		/// Trims the end of each line within a string.
		/// </summary>
		/// <param name="str">The string to trim.</param>
		/// <returns>A string where the end of each line has been trimmed.</returns>
		public static string PerLineTrimEnd(this string str) => PerLineTrimBase(str, x => x.TrimEnd());
		public static SizeF MeasureString(string text, Font font, SizeF layoutArea) => graphics.MeasureString(text, font, layoutArea); // TODO: Convert to SkiaSharp.
		public static SizeF MeasureString(string text, Font font) => graphics.MeasureString(text, font);
	}
}
