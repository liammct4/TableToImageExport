using SixLabors.ImageSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TableToImageExport.DataStructures;
using TableToImageExport.TableContent;
using TableToImageExport.TableContent.ContentStructure;

namespace TableToImageExport.TableStructure
{
	/// <summary>
	/// Base class for cell types which fit into a <see cref="Table"/> type.
	/// </summary>
	public abstract class Cell
	{
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
		private Vector2I _tablePosition;
		/// <summary>
		/// The table which this cell belongs to.
		/// </summary>
		public Table Parent { get; internal set; }
		/// <summary>
		/// The content of this cell, when the table is rendered (using the <see cref="TableGenerator.ExportTableToHtml()"/>) the content will be rendered within the cell.<br/>
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
		/// <summary>
		/// If this cell has been initialized. Set after <see cref="Initialize(Table)"/> is called, a cell can only be initialized once.
		/// </summary>
		protected private bool IsInitialized { get; set; }
		protected internal Cell Initialize(Table parent)
		{
			Parent = parent;
			CellPositionChanged += parent.TableStructureChanged_Event;
			IsInitialized = true;

			return this;
		}
		public event TableStructureChangedEventHandler CellPositionChanged;
	}
}
