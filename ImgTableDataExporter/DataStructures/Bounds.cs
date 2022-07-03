using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImgTableDataExporter.DataStructures
{
	public struct Bounds
	{
		public int TopLeft;
		public int TopRight;
		public int BottomLeft;
		public int BottomRight;

		public Bounds(int topLeft, int topRight, int bottomLeft, int bottomRight)
		{
			TopLeft = topLeft;
			TopRight = topRight;
			BottomLeft = bottomLeft;
			BottomRight = bottomRight;
		}

		public Bounds(int value)
		{
			TopLeft = value;
			TopRight = value;
			BottomLeft = value;
			BottomRight = value;
		}
	}
}
