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
	/// <remarks>
	/// The TSV format is not supported so it has been temporarily removed. 
	/// </remarks>
	public enum DataFormats
	{
		CSV,
		/* This has been removed until a library is found which supports both CSV and TSV formats in one. */
		// TSV
	}
}
