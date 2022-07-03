using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using ImgTableDataExporter.DataStructures;

namespace ImgTableDataExporter.TableStructure
{
	public partial class TableCell
	{
		public static readonly Size DefaultCellSize = new Size(100, 40);
		public static readonly Font DefaultFont = new Font("Helvetica", 15);
		public static readonly Color DefaultTextBG = Color.Black;
		public static readonly Color DefaultBG = Color.White;

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
		public string Content { get; set; }
		public Size CellSize { get; set; } = DefaultCellSize;
		public Font Font { get; set; } = DefaultFont;
		public Color TextBG { get; set; } = DefaultTextBG;
		public Color BG { get; set; } = DefaultBG;
		private Vector2I _tablePosition;
		public event TableStructureChangedEventHandler CellPositionChanged;

		internal TableCell(TableGenerator parent)
		{
			ResetSettings(resetSize: true);
			Parent = parent;
			CellPositionChanged += parent.TableStructureChanged_Event;
		}

		internal TableCell(TableGenerator parent, Vector2I tablePosition, string data, Size? cellSize = null, Font font = null, Color? textBG = null, Color? BG = null)
		{
			Parent = parent;
			TablePosition = tablePosition;
			Content = data;
			CellSize = cellSize is null ? new Size() : cellSize.Value;
			Font = font is null ? new Font("Arial", 13) : font;
			TextBG = textBG is null ? Color.Black : textBG.Value;
			this.BG = BG is null ? Color.Transparent : BG.Value;
			CellPositionChanged += parent.TableStructureChanged_Event;
		}

		public void ResetSettings(bool resetSize = false)
		{
			Font = DefaultFont;
			TextBG = DefaultTextBG;
			BG = DefaultBG;

			if (resetSize)
			{
				CellSize = DefaultCellSize;
			}
		}
	}
}
