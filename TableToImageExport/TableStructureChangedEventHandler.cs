using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TableToImageExport;
using TableToImageExport.DataStructures;
using TableToImageExport.TableStructure;

namespace TableToImageExport
{
	/// <summary>
	/// Invoked when the structure of a table has been changed, applies when cells are added, removed or when their position has changed.
	/// </summary>
	public delegate void TableStructureChangedEventHandler(TableCell cell, TableStructureChangedEventArgs e);
	/// <summary>
	/// Details about table changes, includes the table changed and the change made (if applicable).
	/// </summary>
	public class TableStructureChangedEventArgs : EventArgs
	{
		public Table TableChanged { get; private set; }
		/// <summary>
		/// The old position of the cell which had its position changed.
		/// </summary>
		public Vector2I OldPosition { get; private set; }
		/// <summary>
		/// The new position of the cell which has had its position changed.
		/// </summary>
		public Vector2I NewPosition { get; private set; }
		public TableStructureChangedEventArgs(Table table, Vector2I oldPos, Vector2I newPos)
		{
			TableChanged = table;
			OldPosition = oldPos;
			NewPosition = newPos;
		}
	}
}
