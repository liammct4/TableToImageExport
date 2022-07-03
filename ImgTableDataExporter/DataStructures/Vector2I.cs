using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImgTableDataExporter.DataStructures
{
	public struct Vector2I
	{
		public int X;
		public int Y;

		public Vector2I(int x, int y)
		{
			X = x;
			Y = y;
		}

		public Vector2I(int value)
		{
			X = value;
			Y = value;
		}

		public static bool operator ==(Vector2I a, Vector2I b) => a.X == b.X && a.Y == b.Y;
		public static bool operator !=(Vector2I a, Vector2I b) => !(a == b);
	}
}
