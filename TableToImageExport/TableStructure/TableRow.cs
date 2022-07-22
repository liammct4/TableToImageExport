using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using TableToImageExport.TableContent;
using SixLabors.Fonts;
using SixLabors.ImageSharp;

namespace TableToImageExport.TableStructure
{
	/// <summary>
	/// Represents one row within a table, provides easy access to cells on a certain row, this will be updated automatically in the event the table changes.<br/><br/>
	/// 
	/// Additionally, you can set universal properties which will apply to every cell in the row. E.g. you can set the background of each cell in the row by setting <see cref="RowBG"/>.<br/><br/>
	/// </summary>
	/// <remarks>
	/// Try reusing each object as much as possible or calling the <see cref="Dispose()"/> method when finished as many objects may cause performance issues when updating the table.
	/// </remarks>
	public class TableRow : ITableCollection
	{
		/// <summary>
		/// The cells contained on this row according to <see cref="RowNumber"/>, this is a readonly collection, to modify the table structure, e.g. adding cells, modify the table directly.
		/// </summary>
		public ReadOnlyCollection<TableCell> Cells => _cells.AsReadOnly();
		/// <summary>
		/// The table which this row is located in; does not change.
		/// </summary>
		public TableGenerator Parent { get; internal set; }
		/// <summary>
		/// The number of this row, set only once when this row is created. Every cell will have their Y position match this.
		/// </summary>
		public int RowNumber { get; internal set; }
		/// <summary>
		/// Returns the number of cells inside this row.
		/// </summary>
		public int CellCount => Cells.Count;
		/// <summary>
		/// When getting, this will return the height of the tallest cell in this row.
		/// 
		/// When setting, this will set the height of each cell to the value provided.
		/// </summary>
		public int Height
		{
			get
			{
				// Gets the cell with the maximum height.
				int maxHeight = 0;

				foreach (TableCell cell in Cells)
				{
					if (cell.CellSize.Height > maxHeight)
					{
						maxHeight = cell.CellSize.Height;
					}
				}

				return maxHeight;
			}
			set
			{
				foreach (TableCell cell in Cells)
				{
					cell.CellSize = new Size(cell.CellSize.Width, value);
				}
			}
		}
		/// <summary>
		/// Universal property for setting the font of each cell within this row.
		/// </summary>
		public Font RowFont
		{
			set
			{
				foreach (TableCell cell in Cells)
				{
					if (cell.Content is TextContent content)
					{
						content.Font = value;
					}
				}
			}
		}
		/// <summary>
		/// Universal property for setting the text colour of each cell within this row.<br/><br/>
		/// 
		/// This will only be applied to cells with content of type <see cref="TextContent"/>.
		/// </summary>
		public Color RowTextBG
		{
			set
			{
				foreach (TableCell cell in Cells)
				{
					if (cell.Content is TextContent content)
					{
						content.TextBG = value;
					}
				}
			}
		}
		/// <summary>
		/// Universal property for setting the background colour of each cell within this row.
		/// </summary>
		public Color RowBG
		{
			set
			{
				foreach (TableCell cell in Cells)
				{
					cell.BG = value;
				}
			}
		}

		private List<TableCell> _cells;
		private bool disposedValue;

		private TableRow(TableGenerator table)
		{
			Parent = table;
			Parent.TableCollectionInvalidated += Refresh;
		}

		/// <summary>
		/// Gets a new row with the specified row number from a table.<br/><br/>
		/// 
		/// This is identical to calling <see cref="TableGenerator.GetRow(int)"/>.
		/// </summary>
		/// <param name="parent">The table to get the row from.</param>
		/// <param name="rowNumber">The number of the row.</param>
		/// <returns>A new row which contains all the cells on the row <see cref="RowNumber"/>.</returns>
		public static TableRow FromTable(TableGenerator parent, int rowNumber)
		{
			TableRow row = new(parent)
			{
				RowNumber = rowNumber,
			};

			row.Refresh();

			return row;
		}

		/// <summary>
		/// Updates the collection of cells, when cells are added or removed from the table, the collection of cells will have to be updated to retrieve/remove any cells which are on this row.<br/><br/>
		/// 
		/// You will not have to call this method at all as the cell collection will be updated automatically whenever the table has changed.
		/// </summary>
		/// <exception cref="InvalidOperationException"/>
		public void Refresh()
		{
			if (disposedValue)
			{
				throw new InvalidOperationException("This table row has been disposed.");
			}

			_cells = Parent.Cells.Where(x => x.TablePosition.Y == RowNumber).ToList();
			_cells.Sort((a, b) => a.TablePosition.X - b.TablePosition.X);	
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			foreach (TableCell cell in Cells)
			{
				yield return cell;
			}
		}

		public IEnumerator<TableCell> GetEnumerator()
		{
			foreach (TableCell cell in Cells)
			{
				yield return cell;
			}
		}

		protected virtual void Dispose(bool disposing)
		{
			if (!disposedValue)
			{
				if (disposing)
				{
					_cells.Clear();
					_cells = null;

					Parent.TableCollectionInvalidated -= Refresh;
				}

				disposedValue = true;
			}
		}

		/// <summary>
		/// Disconnects the object from the table. Releases all cells in <see cref="Cells"/>.
		/// </summary>
		public void Dispose()
		{
			Dispose(disposing: true);
			GC.SuppressFinalize(this);
		}

		/// <summary>
		/// Returns the cell in column <paramref name="index"/>.
		/// </summary>
		/// <param name="index">The column number.</param>
		/// <returns>Returns the cell in column <paramref name="index"/> of this row. If none is found, returns <see langword="null"/>.</returns>
		public TableCell this[int index]
		{
			get
			{
				foreach (TableCell cell in Cells)
				{
					if (cell.TablePosition.X == index)
					{
						return cell;
					}
				}

				return null;
			}
		}
	}
}
