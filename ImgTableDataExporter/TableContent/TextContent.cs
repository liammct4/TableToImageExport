using System;
using System.Collections.Generic;
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
		public string Content { get; set; }

		public void WriteContent()
		{
			
		}
	}
}
