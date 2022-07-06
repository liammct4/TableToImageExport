using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImgTableDataExporter.DataStructures
{
	/// <summary>
	/// Represents a vector with 2 integer values.
	/// </summary>
	public struct Vector2I
	{
		/// <summary>
		/// Gets a new vector which has both its values set to 0.
		/// </summary>
		public static Vector2I Empty => new Vector2I(0);
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
