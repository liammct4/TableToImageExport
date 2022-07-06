using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ImgTableDataExporter.DataStructures;

namespace ImgTableDataExporter.ImageData
{
	/// <summary>
	/// Provides extension methods for the <see cref="Graphics"/> class to provide extra missing functionality.
	/// </summary>
	public static class ImageExtensions
	{
		/// <summary>
		/// Draws a rectangular box with rounded corners specified by <paramref name="data"/>.
		/// </summary>
		/// <param name="data">The rectangle area to fill.</param>
		/// <param name="colour">The background colour of the rectangle.</param>
		/// <param name="corners">The radius of each of the four corners of the rectangle.</param>
		/// <param name="border">The border colour of the rectangle.</param>
		public static void DrawRoundedBox(this Graphics graphics, Rectangle data, Color colour, Bounds corners, Color? border = null)
		{
			if (!border.HasValue)
			{
				border = Color.Black;
			}

			Pen pen = new Pen(new SolidBrush(border.Value));

			graphics.FillRectangles(new SolidBrush(colour), rects: new Rectangle[]
			{
				// Left Rectangle
				new Rectangle()
				{
					X = data.X + 1,
					Y = data.Y + corners.TopLeft,
					Width = corners.TopLeft > corners.BottomLeft ? corners.TopLeft : corners.BottomLeft,
					Height = data.Height - (corners.TopLeft + corners.BottomLeft)
				},
				// Top Rectangle
				new Rectangle()
				{
					X = data.X + corners.TopLeft,
					Y = data.Y + 1,
					Width = data.Width - (corners.TopLeft + corners.TopRight),
					Height = corners.TopLeft > corners.TopRight ? corners.TopLeft : corners.TopRight
				},
				// Right Rectangle
				new Rectangle()
				{
					X = (data.X + data.Width) - (corners.TopRight > corners.BottomRight ? corners.TopRight : corners.BottomRight),
					Y = data.Y + corners.TopRight,
					Width = corners.TopRight > corners.BottomRight ? corners.TopRight : corners.BottomRight,
					Height = data.Height - (corners.TopRight + corners.BottomRight)
				},
				// Bottom Rectangle
				new Rectangle()
				{
					X = data.X + corners.BottomLeft,
					Y = (data.Y + data.Height) - (corners.BottomLeft > corners.BottomRight ? corners.BottomLeft : corners.BottomRight),
					Width = data.Width - (corners.BottomLeft + corners.BottomRight),
					Height = corners.BottomLeft > corners.BottomRight ? corners.BottomLeft : corners.BottomRight
				},
				// Fill Rectangle
				new Rectangle()
				{
					X = data.X + (corners.TopLeft > corners.BottomLeft ? corners.TopLeft : corners.BottomLeft),
					Y = data.Y + (corners.TopLeft > corners.TopRight ? corners.TopLeft : corners.TopRight),
					Width = data.Width - ((corners.TopLeft > corners.BottomLeft ? corners.TopLeft : corners.BottomLeft) + (corners.TopRight > corners.BottomRight ? corners.TopRight : corners.BottomRight)),
					Height = data.Height - ((corners.TopLeft > corners.TopRight ? corners.TopLeft : corners.TopRight) + (corners.BottomLeft > corners.BottomRight ? corners.BottomLeft : corners.BottomRight))

				}
			});

			// Horizontal Lines.
			graphics.DrawLine(pen,
				new Point(data.X + corners.TopLeft, data.Y),
				new Point(data.X + (data.Width - corners.TopRight), data.Y));
			graphics.DrawLine(pen,
				new Point(data.X + corners.BottomLeft, data.Y + data.Height),
				new Point(data.X + (data.Width - corners.BottomRight), data.Y + data.Height));

			// Vertical Lines.
			graphics.DrawLine(pen,
				new Point(data.X, data.Y + corners.TopLeft),
				new Point(data.X, data.Y + (data.Height - corners.BottomLeft)));
			graphics.DrawLine(pen,
				new Point(data.X + data.Width, data.Y + corners.TopRight),
				new Point(data.X + data.Width, data.Y + (data.Height - corners.BottomRight)));

			// Corners
			if (corners.TopLeft > 0)
			{
				graphics.FillArc(pen, colour, new Rectangle(data.X, data.Y, corners.TopLeft * 2, corners.TopLeft * 2), 180, 90);
			}

			if (corners.TopRight > 0)
			{
				graphics.FillArc(pen, colour, new Rectangle(data.X + data.Width - (corners.TopRight * 2), data.Y, corners.TopRight * 2, corners.TopRight * 2), 270, 90);
			}

			if (corners.BottomLeft > 0)
			{
				graphics.FillArc(pen, colour, new Rectangle(data.X, data.Y + (data.Height - (corners.BottomLeft * 2)), corners.BottomLeft * 2, corners.BottomLeft * 2), 90, 90);
			}

			if (corners.BottomRight > 0)
			{
				graphics.FillArc(pen, colour, new Rectangle(data.X + (data.Width - (corners.BottomRight * 2)), data.Y + (data.Height - (corners.BottomRight * 2)), corners.BottomRight * 2, corners.BottomRight * 2), 0, 90);
			}
		}

		/// <summary>
		/// Draws and fills in an arc.
		/// </summary>
		/// <param name="pen">Used in drawing the arc outline.</param>
		/// <param name="color">The fill colour of the arc.</param>
		/// <param name="rect">The area which the arc occupies.</param>
		/// <param name="startAngle">Start Angle</param>
		/// <param name="sweepAngle">How many degrees the arc will extend.</param>
		public static void FillArc(this Graphics graphics, Pen pen, Color color, Rectangle rect, int startAngle, int sweepAngle)
		{
			graphics.FillPie(new SolidBrush(color), rect, startAngle, sweepAngle);
			graphics.DrawArc(pen, rect, startAngle, sweepAngle);
		}
	}
}
