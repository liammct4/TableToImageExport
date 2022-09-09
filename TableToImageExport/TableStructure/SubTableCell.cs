using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TableToImageExport.TableContent;

namespace TableToImageExport.TableStructure
{
	/// <summary>
	/// The cell type used within a sub table (<see cref="SubTableContent"/>).
	/// </summary>
	public class SubTableCell : Cell
	{
		/// <summary>
		/// Do NOT use this method to create a cell, instead use the relevant CreateNewCell method from a <see cref="SubTableContent"/> object. 
		/// </summary>
		public SubTableCell() { }
	}
}
