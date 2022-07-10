using TableToImageExport.TableStructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TableToImageExport
{
	/// <summary>
	/// An exception thrown when a <see cref="TableCell"/> object was attempted to be added to a <see cref="TableGenerator"/> object where the <see cref="TableCell.Parent"/> is a different table.
	/// </summary>
	[Serializable]
	public class TableMismatchException : Exception
	{
		internal const string DEFAULT_MESSAGE = "An attempt was made to add a cell to a table when the cell belonged to another table.";
		/// <summary>
		/// The cell that was tried to be added to <see cref="AttemptedTable"/>.
		/// </summary>
		public TableCell MismatchedCell { get; internal set; }
		/// <summary>
		/// The table which had an attempt to add a cell to which belongs to another table.
		/// </summary>
		public TableGenerator AttemptedTable { get; internal set; }

		public TableMismatchException(string message = DEFAULT_MESSAGE) : base(message) { }
		public TableMismatchException(string message, Exception inner) : base(message, inner) { }

		public TableMismatchException(TableCell mismatchedCell, TableGenerator attemptedTable, string message = DEFAULT_MESSAGE) : base(message)
		{
			MismatchedCell = mismatchedCell;
			AttemptedTable = attemptedTable;
		}
	}
}
