using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImgTableDataExporter.TableContent
{
	/// <summary>
	/// This interface represents one type of content located within a cell.
	/// </summary>
	public interface ITableContent
	{
		void WriteContent();
	}
}
