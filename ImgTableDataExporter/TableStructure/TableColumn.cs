using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace ImgTableDataExporter.TableStructure
{
	public class TableColumn : ITableCollection
	{
		public ReadOnlyCollection<TableCell> Cells => _cells.AsReadOnly();
		public TableGenerator Parent { get; internal set; }
		public int ColumnNumber { get; internal set; }
		public int Width
		{
			get
			{
				// Gets the cell with the maximum width;
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
		private List<TableCell> _cells;
		private bool disposedValue;

		private TableColumn(TableGenerator table)
		{
			Parent = table;
			Parent.TableCollectionInvalidated += Refresh;
		}

		public static TableColumn FromTable(TableGenerator parent, int columnNumber)
		{
			TableColumn column = new TableColumn(parent)
			{
				ColumnNumber = columnNumber,
			};

			column.Refresh();

			return column;
		}

		public void Refresh()
		{
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

		public void Dispose()
		{
			Dispose(disposing: true);
			GC.SuppressFinalize(this);
		}

		public TableCell this[int index]
		{
			get
			{
				if (index < Cells.Count)
				{
					return Cells.ElementAt(index);
				}

				return null;
			}
		}
	}
}
