using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;

namespace ImgTableDataExporter.TableStructure
{
	public class TableRow : ITableCollection
	{
		public ReadOnlyCollection<TableCell> Cells => _cells.AsReadOnly();
		public TableGenerator Parent { get; internal set; }
		public int RowNumber { get; internal set; }
		public int Height
		{
			get
			{
				// Gets the cell with the maximum height;
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
		public Font RowFont
		{
			set
			{
				foreach (TableCell cell in Cells)
				{
					cell.Font = value;
				}
			}
		}
		public Color RowTextBG
		{
			set
			{
				foreach (TableCell cell in Cells)
				{
					cell.TextBG = value;
				}
			}
		}
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

		public static TableRow FromTable(TableGenerator parent, int rowNumber)
		{
			TableRow row = new TableRow(parent)
			{
				RowNumber = rowNumber,
			};

			row.Refresh();

			return row;
		}

		public void Refresh()
		{
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

		public void Dispose()
		{
			// Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
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
