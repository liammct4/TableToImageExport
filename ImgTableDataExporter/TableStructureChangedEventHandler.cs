using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ImgTableDataExporter;
using ImgTableDataExporter.DataStructures;
using ImgTableDataExporter.TableStructure;

namespace ImgTableDataExporter
{
	public delegate void TableStructureChangedEventHandler(TableCell cell, TableStructureChangedEventArgs e);
	public class TableStructureChangedEventArgs : EventArgs
	{
		public TableGenerator TableChanged { get; private set; }
		public Vector2I OldPosition { get; private set; }
		public Vector2I NewPosition { get; private set; }
		public TableStructureChangedEventArgs(TableGenerator table, Vector2I oldPos, Vector2I newPos)
		{
			TableChanged = table;
			OldPosition = oldPos;
			NewPosition = newPos;
		}
	}
}
