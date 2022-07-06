using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;

namespace ImgTableDataExporter.Utilities
{
	public static class Utilities
	{
		private static readonly Graphics graphics;
		static Utilities()
		{
			Bitmap utilityBitmap = new Bitmap(1, 1);
			graphics = Graphics.FromImage(utilityBitmap);
		}

		// https://stackoverflow.com/questions/1879395/how-do-i-generate-a-stream-from-a-string
		public static Stream GenerateStreamFromString(string s)
		{
			var stream = new MemoryStream();
			var writer = new StreamWriter(stream);
			writer.Write(s);
			writer.Flush();
			stream.Position = 0;
			return stream;
		}

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

		public static string PerLineTrim(this string str) => PerLineTrimBase(str, x => x.Trim());
		public static string PerLineTrimStart(this string str) => PerLineTrimBase(str, x => x.TrimStart());
		public static string PerLineTrimEnd(this string str) => PerLineTrimBase(str, x => x.TrimEnd());
		public static SizeF MeasureString(string text, Font font, SizeF layoutArea) => graphics.MeasureString(text, font, layoutArea);
		public static SizeF MeasureString(string text, Font font) => graphics.MeasureString(text, font);
	}
}
