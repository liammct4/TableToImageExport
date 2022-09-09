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
		/// Creates a new empty table cell, call <see cref="Initialize(Table{TableCell})"/> to 
		/// </summary>
		public TableCell()
		{

		}

		protected internal new TableCell Initialize(Table<TableCell> parent)
		{
			ResetSettings(resetSize: true);
			Parent = parent;
			CellPositionChanged += parent.TableStructureChanged_Event;
			IsInitialized = true;

			return this;
		}

		protected internal TableCell Initialize(Table<TableCell> parent, Vector2I tablePosition, ITableContent data, ItemAlignment? contentAlignment = null, Size? cellSize = null, Color? BG = null)
		{
			Parent = parent;
			TablePosition = tablePosition;
			Content = data;
			ContentAlignment = contentAlignment is null ? DefaultContentAlignment : contentAlignment.Value;
			CellSize = cellSize is null ? DefaultCellSize : cellSize.Value;
			this.BG = BG is null ? DefaultBG : BG.Value;
			CellPositionChanged += parent.TableStructureChanged_Event;
			IsInitialized = true;

			return this;
		}

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
