using SixLabors.ImageSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TableToImageExport.TableContent
{
	/// <summary>
	/// Provides a set of internal utilities for manipulating content types.
	/// </summary>
	internal static class Utilities
	{
		/// <summary>
		/// Takes an object of any type and converts it to the relevant content type. E.g. A <see cref="DateTime"/> provided will be converted to a <see cref="DateContent"/>.
		/// </summary>
		/// <param name="item">The item to convert.</param>
		/// <returns>The converted table content type.</returns>
		internal static ITableContent GetContentFromObject(object item) => item switch
		{
			Image image => new ImageContent(image),
			DateTime date => new DateContent(date),
			_ => new TextContent(item.ToString())
		};
	}
}
