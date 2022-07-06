using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImgTableDataExporter
{
	/// <summary>
	/// Data formats compatible with the table generator, data in these formats can be loaded into a table using <see cref="TableGenerator.Load(System.IO.Stream)"/>
	/// </summary>
	public enum DataFormats
	{
		CSV,
		TSV,
	}
}
