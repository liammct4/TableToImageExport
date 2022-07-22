using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TableToImageExport;
using TableToImageExport.TableStructure;

namespace TableToImageExport.TableStructure
{
	/// <summary>
	/// Defines a collection of cells linked to a table.
	/// </summary>
	public interface ITableCollection : IEnumerable<TableCell>, IDisposable
	{
		/// <summary>
		/// The cells which the collection will store. <br/><br/>
		/// 
		/// This should in no way hold the structure of the table, instead, it should only capture some cells within the table. This is why it is readonly.
		/// </summary>
		ReadOnlyCollection<TableCell> Cells { get; }
		/// <summary>
		/// The table which this collection belongs to.
		/// </summary>
		TableGenerator Parent { get; }
		/// <summary>
		/// Returns the number of cells inside this collection.
		/// </summary>
		public int CellCount => Cells.Count;
		/// <summary>
		/// Updates <see cref="Cells"/> based on a condition.
		/// </summary>
		void Refresh();
	}
}
