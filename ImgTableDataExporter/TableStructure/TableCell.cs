using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using ImgTableDataExporter.DataStructures;
using ImgTableDataExporter.TableContent;
using ImgTableDataExporter.TableContent.ContentStructure;

namespace ImgTableDataExporter.TableStructure
{
	public partial class TableCell
	{
		public static Size DefaultCellSize = new Size(100, 28);
		public static Color DefaultBG = Color.White;
		public static ItemAlignment DefaultContentAlignment = ItemAlignment.CentreLeft;

		public Vector2I TablePosition
		{
			get => _tablePosition;
			set
			{
				CellPositionChanged?.Invoke(this, new TableStructureChangedEventArgs(Parent, _tablePosition, value));
				_tablePosition = value;
			}
		}
		public TableGenerator Parent { get; internal set; }
		public ITableContent Content { get; set; }
		public Size CellSize { get; set; } = DefaultCellSize;
		public Color BG { get; set; } = DefaultBG;
		public ItemAlignment ContentAlignment { get; set; } = DefaultContentAlignment;

		private Vector2I _tablePosition;
		public event TableStructureChangedEventHandler CellPositionChanged;

		internal TableCell(TableGenerator parent)
		{
			ResetSettings(resetSize: true);
			Parent = parent;
			CellPositionChanged += parent.TableStructureChanged_Event;
		}

		internal TableCell(TableGenerator parent, Vector2I tablePosition, ITableContent data, ItemAlignment? contentAlignment = null, Size? cellSize = null, Color? BG = null)
		{
			Parent = parent;
			TablePosition = tablePosition;
			Content = data;
			ContentAlignment = contentAlignment is null ? DefaultContentAlignment : contentAlignment.Value;
			CellSize = cellSize is null ? DefaultCellSize : cellSize.Value;
			this.BG = BG is null ? DefaultBG : BG.Value;
			CellPositionChanged += parent.TableStructureChanged_Event;
		}

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
