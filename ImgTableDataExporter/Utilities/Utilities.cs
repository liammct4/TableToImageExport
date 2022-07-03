using System;
using System.Collections.Generic;
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
	}
}
