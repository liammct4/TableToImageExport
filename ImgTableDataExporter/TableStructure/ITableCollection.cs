using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ImgTableDataExporter;
using ImgTableDataExporter.TableStructure;

namespace ImgTableDataExporter.TableStructure
{
	public interface ITableCollection : IEnumerable<TableCell>, IDisposable
	{
		ReadOnlyCollection<TableCell> Cells { get; }
		TableGenerator Parent { get; }
		void Refresh();
	}
}
