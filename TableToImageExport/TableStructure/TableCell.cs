using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using TableToImageExport.DataStructures;
using TableToImageExport.TableContent;
using TableToImageExport.TableContent.ContentStructure;
using SixLabors.ImageSharp;

namespace TableToImageExport.TableStructure
{
	/// <summary>
	/// Represents one cell within a table, use the dedicated methods <see cref="TableGenerator.CreateNewCell(Vector2I, ITableContent)"/> to create a new cell in a table.
	/// </summary>
	public class TableCell : Cell
	{
		public static Size DefaultCellSize = new(100, 28);

		/// <summary>
		/// The size of this cell in pixels, generally, you shouldn't need to use this as you can use the universal properties <see cref="TableRow.Height"/> and <see cref="TableColumn.Width"/> to set the size of every cell.<br/><br/>
		/// 
		/// If this cell is wider then other cells in the same column, the other cells will still be aligned properly however, they will have a large bit of empty space between the columns. This also applies to rows.
		/// </summary>
		public Size CellSize { get; set; } = DefaultCellSize;
		/// <summary>
		/// Do NOT use this method to create a cell, instead use the relevant CreateNewCell method from a <see cref="TableGenerator"/> object. 
		/// </summary>
		public TableCell() { }
		/// <summary>
		/// Resets the settings of this cell to their default values.
		/// </summary>
		/// <param name="resetSize">Determines if the size of the cell will be reset.</param>
		public void ResetSettings(bool resetSize = false)
		{
			BG = DefaultBG;

			if (resetSize)
			{
				CellSize = DefaultCellSize;
			}
		}
	}
}
