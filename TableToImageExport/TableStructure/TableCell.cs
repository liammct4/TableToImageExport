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
	public class TableCell
	{
		public static Size DefaultCellSize = new(100, 28);
		public static Color DefaultBG = Color.White;
		public static ItemAlignment DefaultContentAlignment = ItemAlignment.CentreLeft;

		/// <summary>
		/// Coordinates of this cell within the table. When changed, table collection objects (e.g. <see cref="TableRow"/>) will be automatically updated.
		/// </summary>
		public Vector2I TablePosition
		{
			get => _tablePosition;
			set
			{
				CellPositionChanged?.Invoke(this, new TableStructureChangedEventArgs(Parent, _tablePosition, value));
				_tablePosition = value;
			}
		}
		/// <summary>
		/// The table which this cell belongs to.
		/// </summary>
		public TableGenerator Parent { get; internal set; }
		/// <summary>
		/// The content of this cell, when the table is rendered (using the <see cref="TableGenerator.ExportTable()"/>) the content will be rendered within the cell.<br/>
		/// This is a generic interface as there are multiple content types (e.g. <see cref="TextContent"/>, <see cref="ImageContent"/>). To access specific settings such as text colour or image size, cast this to its class implementation.<br/><br/>
		/// 
		/// The main content types are:
		/// <list type="bullet">
		///		<item><see cref="TextContent"/> Represents text.</item>
		///		<item><see cref="ImageContent"/> Represents images.</item>
		/// </list>
		/// You can create your own content type for your own purposes, see <see cref="ITableContent"/>.
		/// </summary>
		public ITableContent Content { get; set; }
		/// <summary>
		/// The size of this cell in pixels, generally, you shouldn't need to use this as you can use the universal properties <see cref="TableRow.Height"/> and <see cref="TableColumn.Width"/> to set the size of every cell.<br/><br/>
		/// 
		/// If this cell is wider then other cells in the same column, the other cells will still be aligned properly however, they will have a large bit of empty space between the columns. This also applies to rows.
		/// </summary>
		public Size CellSize { get; set; } = DefaultCellSize;
		/// <summary>
		/// The background colour of the cell.
		/// </summary>
		public Color BG { get; set; } = DefaultBG;
		/// <summary>
		/// Where the content will be displayed relative to the cell. See <see langword="static"/> properties of <see cref="ItemAlignment"/> to get a choice of alignments.<br/><br/>
		/// 
		/// You can also set the margin of the content in the cell through this, see <see cref="ItemAlignment.Margin"/>.
		/// 
		/// The alignment choices are below:
		/// <list type="bullet">
		///		<item><see cref="ItemAlignment.TopLeft"/></item>
		///		<item><see cref="ItemAlignment.TopRight"/></item>
		///		<item><see cref="ItemAlignment.BottomRight"/></item>
		///		<item><see cref="ItemAlignment.BottomLeft"/></item>
		///		<item><see cref="ItemAlignment.CentreLeft"/></item>
		///		<item><see cref="ItemAlignment.TopCentre"/></item>
		///		<item><see cref="ItemAlignment.CentreRight"/></item>
		///		<item><see cref="ItemAlignment.BottomCentre"/></item>
		///		<item><see cref="ItemAlignment.Centre"/></item>
		/// </list>
		/// </summary>
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
