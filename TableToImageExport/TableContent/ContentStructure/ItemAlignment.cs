using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SixLabors.ImageSharp;
using TableToImageExport.DataStructures;

namespace TableToImageExport.TableContent.ContentStructure
{
	/// <summary>
	/// Stores the relative position of an object within the bounds of a provided container.
	/// </summary>
	public struct ItemAlignment
	{
		/// <summary>
		/// Gets a new alignment in the top left corner.
		/// </summary>
		public static ItemAlignment TopLeft => new ItemAlignment(HorizontalAlign.Left, VerticalAlign.Top);
		/// <summary>
		/// Gets a new alignment in the top right corner.
		/// </summary>
		public static ItemAlignment TopRight => new ItemAlignment(HorizontalAlign.Right, VerticalAlign.Top);
		/// <summary>
		/// Gets a new alignment in the bottom right corner.
		/// </summary>
		public static ItemAlignment BottomRight => new ItemAlignment(HorizontalAlign.Right, VerticalAlign.Bottom);
		/// <summary>
		/// Gets a new alignment in the bottom left corner.
		/// </summary>
		public static ItemAlignment BottomLeft => new ItemAlignment(HorizontalAlign.Left, VerticalAlign.Bottom);
		/// <summary>
		/// Gets a new alignment at the top and horizontally centred.
		/// </summary>
		public static ItemAlignment TopCentre => new ItemAlignment(HorizontalAlign.Centre, VerticalAlign.Top);
		/// <summary>
		/// Gets a new alignment at the right and vertically centred.
		/// </summary>
		public static ItemAlignment CentreRight => new ItemAlignment(HorizontalAlign.Right, VerticalAlign.Centre);
		/// <summary>
		/// Getsa new alignment at the bottom and horizontally centred.
		/// </summary>
		public static ItemAlignment BottomCentre => new ItemAlignment(HorizontalAlign.Centre, VerticalAlign.Bottom);
		/// <summary>
		/// Gets a new alignment on the left and vertically centred.
		/// </summary>
		public static ItemAlignment CentreLeft => new ItemAlignment(HorizontalAlign.Left, VerticalAlign.Centre);
		/// <summary>
		/// Gets a new alignment which is both horizontally and vertically centred.
		/// </summary>
		public static ItemAlignment Centre => new ItemAlignment(HorizontalAlign.Centre, VerticalAlign.Centre);

		/// <summary>
		/// Determines the horizontal position of an object, used to calculate in the <see cref="Align(SizeF, SizeF)"/> method. 
		/// </summary>
		public HorizontalAlign Horizontal;
		/// <summary>
		/// Determines the vertical position of an object, used to calculate in the <see cref="Align(SizeF, SizeF)"/> method. 
		/// </summary>
		public VerticalAlign Vertical;
		/// <summary>
		/// When aligned against the edge of a container, an object can have a margin from the container edge.<br/><br/>
		/// The <see cref="Vector2I.X"/> determines the horizontal margin.<br/>
		/// The <see cref="Vector2I.Y"/> determiens the vertical margin.<br/><br/>
		/// This does not affect the object when it is centred.
		/// </summary>
		public Vector2I Margin;
		
		public ItemAlignment(HorizontalAlign horizontal, VerticalAlign vertical, Vector2I? margin = null)
		{
			Horizontal = horizontal;
			Vertical = vertical;
			Margin = margin is null ? new Vector2I(2) : margin.Value;
		}

		/// <summary>
		/// Gets the relative position of an object within the container according to the horizontal and vertical alignment aswell as the margin (<see cref="Margin"/>).<br/> 
		/// </summary>
		/// <param name="containerSize">Size of the container.</param>
		/// <param name="objectSize">Size of the object.</param>
		/// <returns>The relative position of the object within the container.</returns>
		public Point Align(SizeF containerSize, SizeF objectSize) => new()
		{
			X = (int)GetHorizontal(objectSize.Width, containerSize.Width - 1),
			Y = (int)GetVertical(objectSize.Height, containerSize.Height - 1)
		};

		private float GetHorizontal(float objectWidth, float containerWidth) => Horizontal switch
		{
			HorizontalAlign.Left => 0 + Margin.X,
			HorizontalAlign.Centre => (containerWidth / 2) - (objectWidth / 2),
			HorizontalAlign.Right => containerWidth - (objectWidth + Margin.X),
			_ => 0,
		};

		private float GetVertical(float objectHeight, float containerHeight) => Vertical switch
		{
			VerticalAlign.Top => 0 + Margin.Y,
			VerticalAlign.Centre => (containerHeight / 2) - (objectHeight / 2),
			VerticalAlign.Bottom => containerHeight - (objectHeight + Margin.Y),
			_ => 0,
		};
	}
}
