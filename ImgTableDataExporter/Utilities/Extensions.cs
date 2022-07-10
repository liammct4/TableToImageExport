using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TableToImageExport.Utilities
{
	/// <summary>
	/// Provides extension methods for different classes/structs
	/// </summary>
	public static class Extensions
	{
		/// <summary>
		/// Performs the specified action on each element of the <see cref="IEnumerable{T}"/>
		/// </summary>
		public static void ForEach<T>(this IEnumerable<T> collection, Action<T> action)
		{
			foreach (T item in collection)
			{
				action(item);
			}
		}

		/// <summary>
		/// Searches for an element that matches the conditions defined by the specified predicate, and returns the zero-based index of the first occurrence within the entire <see cref="ICollection{T}"/>.
		/// </summary>
		public static int FindIndex<T>(this ICollection<T> collection, Predicate<T> criteria)
		{
			for (int i = 0; i < collection.Count; i++)
			{
				if (criteria(collection.ElementAt(i)))
				{
					return i;
					
				}
			}

			return -1;
		}
	}
}
