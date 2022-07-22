using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TableToImageExport.DataStructures;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Drawing;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;

namespace TableToImageExport.ImageData
{
	/// <summary>
	/// Provides extension methods for the <see cref="IImageProcessingContext"/> class to provide extra missing functionality.
	/// </summary>
	public static class ImageExtensions
	{
		/// <summary>
		/// Draws a rectangular box with rounded corners specified by <paramref name="data"/>.
		/// </summary>
		/// <param name="graphics">The process to draw on.</param>
		/// <param name="data">The rectangle area to fill.</param>
		/// <param name="colour">The background colour of the rectangle.</param>
		/// <param name="corners">The radius of each of the four corners of the rectangle.</param>
		/// <param name="border">The border colour of the rectangle.</param>
		public static void DrawRoundedBox(this IImageProcessingContext graphics, Rectangle data, Color colour, Bounds corners, Color? border = null)
		{
			if (!border.HasValue)
			{
				border = Color.Black;
			}

			DrawingOptions options = new()
			{
				GraphicsOptions = new GraphicsOptions()
				{
					Antialias = false
				}
			};

			ILineSegment topLeft = new EllipticalArcLineSegment(data.X + corners.TopLeft - 1, data.Y + corners.TopLeft - 2, corners.TopLeft, corners.TopLeft, 0, 180, 90, new System.Numerics.Matrix3x2(1, 0, 0, 1, 0, 1));
			ILineSegment bottomLeft = new EllipticalArcLineSegment(data.X + corners.BottomLeft - 1, data.Y + data.Height - corners.BottomLeft - 2, corners.BottomLeft, corners.BottomLeft, 0, 270, 90, new System.Numerics.Matrix3x2(1, 0, 0, 1, 0, 1));
			ILineSegment bottomRight = new EllipticalArcLineSegment(data.X + data.Width - corners.BottomRight - 1, data.Y + data.Height - corners.BottomRight - 2, corners.BottomRight, corners.BottomRight, 0, 0, 90, new System.Numerics.Matrix3x2(1, 0, 0, 1, 0, 1));
			ILineSegment topRight = new EllipticalArcLineSegment(data.X + data.Width - corners.TopRight - 1, data.Y + corners.TopRight - 2, corners.TopRight, corners.TopRight, 0, 90, 90, new System.Numerics.Matrix3x2(1, 0, 0, 1, 0, 1));

			Polygon boxFill = new(
				topLeft,
				bottomLeft,
				bottomRight,
				topRight
			);

			IPath path = new SixLabors.ImageSharp.Drawing.Path(
				// Top Left
				topLeft,
				// Bottom Left
				new EllipticalArcLineSegment(data.X + corners.BottomLeft - 1, data.Y + data.Height - corners.BottomLeft - 3, corners.BottomLeft, corners.BottomLeft, 0, 270, 90, new System.Numerics.Matrix3x2(1, 0, 0, 1, 0, 1)),
				// Bottom Right
				new EllipticalArcLineSegment(data.X + data.Width - corners.BottomRight - 2, data.Y + data.Height - corners.BottomRight - 3, corners.BottomRight, corners.BottomRight, 0, 0, 90, new System.Numerics.Matrix3x2(1, 0, 0, 1, 0, 1)),
				// Top Right
				new EllipticalArcLineSegment(data.X + data.Width - corners.TopRight - 2, data.Y + corners.TopRight - 2, corners.TopRight, corners.TopRight, 0, 90, 90, new System.Numerics.Matrix3x2(1, 0, 0, 1, 0, 1))
			);

			graphics
				.Fill(colour, boxFill)
				.Draw(new Pen(border.Value, 1), path)
				.DrawLines(options, new Pen(border.Value, 1), new PointF(data.X + corners.TopLeft - 1, data.Y - 1), new PointF(data.X + data.Width - corners.TopRight - 2, data.Y - 1));
		}
	}
}
