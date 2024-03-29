﻿using SixLabors.Fonts;
using SixLabors.ImageSharp;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TableToImageExport.TableContent;
using TableToImageExport.TableContent.ContentStructure;

namespace TableToImageExport.TableStructure
{
	/// <summary>
	/// Represents one column within a table, provides easy access to cells in a certain column, this will be updated automatically in the event the table changes.<br/><br/>
	/// 
	/// Additionally, you can set universal properties which will apply to every cell in the column. E.g. you can set the width of each cell in the row by setting <see cref="Width"/>.<br/><br/>
	/// </summary>
	/// <remarks>
	/// Try reusing each object as much as possible or calling the <see cref="Dispose()"/> method when finished as many objects may cause performance issues when updating the table.
	/// </remarks>
	public class TableColumn : ITableCollection
	{
		/// <summary>
		/// The cells contained in this column according to <see cref="ColumnNumber"/>, this is a readonly collection, to modify the table structure, e.g. adding cells, modify the table directly.
		/// </summary>
		public ReadOnlyCollection<TableCell> Cells => _cells.AsReadOnly();
		/// <summary>
		/// The table which this column is located in; does not change.
		/// </summary>
		public TableGenerator Parent { get; internal set; }
		/// <summary>
		/// The number of this column, set only once when this row is created. Every cell will have their X position match this.
		/// </summary>
		public int ColumnNumber { get; internal set; }
		/// <summary>
		/// Returns the number of cells inside this column.
		/// </summary>
		public int CellCount => Cells.Count;
		/// <summary>
		/// When getting, this will return the width of the widest cell in this row.
		/// 
		/// When setting, this will set the width of each cell to the value provided.
		/// </summary>
		public int Width
		{
			get
			{
				// Gets the cell with the maximum width.
				int maxWidth = 0;

				foreach (TableCell cell in Cells)
				{
					if (cell.CellSize.Width > maxWidth)
					{
						maxWidth = cell.CellSize.Width;
					}
				}

				return maxWidth;
			}
			set
			{
				foreach (TableCell cell in Cells)
				{
					cell.CellSize = new Size(value, cell.CellSize.Height);
				}
			}
		}
		/// <summary>
		/// Universal property for setting the font of each cell within this column.
		/// 
		/// Changes properties: All types of <see cref="ITextContent"/> with <see cref="ITextContent.Font"/>.
		/// </summary>
		public Font ColumnFont
		{
			set
			{
				foreach (TableCell cell in Cells)
				{
					if (cell.Content is ITextContent content)
					{
						content.Font = value;
					}
				}
			}
		}
		/// <summary>
		/// Universal property for setting the text colour of each cell within this column.<br/><br/>
		/// 
		/// Changes properties: All types of <see cref="ITextContent"/> with <see cref="ITextContent.TextBG"/>.
		/// </summary>
		public Color ColumnTextBG
		{
			set
			{
				foreach (TableCell cell in Cells)
				{
					if (cell.Content is ITextContent content)
					{
						content.TextBG = value;
					}
				}
			}
		}
		/// <summary>
		/// Universal property for setting the background colour of each cell within this column.<br/><br/>
		/// 
		/// Changes property: <see cref="TableCell.BG"/>.
		/// </summary>
		public Color ColumnBG
		{
			set
			{
				foreach (TableCell cell in Cells)
				{
					cell.BG = value;
				}
			}
		}
		/// <summary>
		/// Universal property for setting the content alignment of each cell within this column.<br/><br/>
		/// 
		/// Changes property: <see cref="TableCell.ContentAlignment"/>.
		/// </summary>
		public ItemAlignment ColumnContentAlignment
		{
			set
			{
				foreach (TableCell cell in Cells)
				{
					cell.ContentAlignment = value;
				}
			}
		}
		private List<TableCell> _cells;
		private bool disposedValue;

		private TableColumn(TableGenerator table)
		{
			Parent = table;
			Parent.TableCollectionInvalidated += Refresh;
		}

		/// <summary>
		/// Gets a new column with the specified column number from a table.<br/><br/>
		/// 
		/// This is identical to calling <see cref="TableGenerator.GetColumn(int)"/>.
		/// </summary>
		/// <param name="parent">The table to get the column from.</param>
		/// <param name="columnNumber">The number of the column.</param>
		/// <returns>A new column which contains all the cells on the column <see cref="ColumnNumber"/>.</returns>
		public static TableColumn FromTable(TableGenerator parent, int columnNumber)
		{
			TableColumn column = new(parent)
			{
				ColumnNumber = columnNumber,
			};

			column.Refresh();

			return column;
		}

		/// <summary>
		/// Updates the collection of cells, when cells are added or removed from the table, the collection of cells will have to be updated to retrieve/remove any cells which are in this column.<br/><br/>
		/// 
		/// You will not have to call this method at all as the cell collection will be updated automatically whenever the table has changed.
		/// </summary>
		/// <exception cref="InvalidOperationException"/>
		public void Refresh()
		{
			if (disposedValue)
			{
				throw new InvalidOperationException("This table column has been disposed.");
			}

			_cells = Parent.Cells.Where(x => x.TablePosition.X == ColumnNumber).ToList();
			_cells.Sort((a, b) => a.TablePosition.Y - b.TablePosition.Y);
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
		/// Returns the cell on the row <paramref name="index"/>.
		/// </summary>
		/// <param name="index">The row number.</param>
		/// <returns>Returns the cell in row <paramref name="index"/> of this column. If none is found, returns <see langword="null"/>.</returns>
		public TableCell this[int index]
		{
			get
			{
				foreach (TableCell cell in Cells)
				{
					if (cell.TablePosition.Y == index)
					{
						return cell;
					}
				}

				return null;
			}
		}
	}
}
