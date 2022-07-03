using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImgTableDataExporter.Utilities
{
	public static class Extensions
	{
		public static void ForEach<T>(this IEnumerable<T> collection, Action<T> action)
		{
			foreach (T item in collection)
			{
				action(item);
			}
		}

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

		public static T FindLargest<T>(this ICollection<T> collection, Comparison<T> criteria)
		{
			return FindLargest(collection.AsEnumerable(), criteria);
		}

		public static T FindLargest<T>(this IEnumerable<T> collection, Comparison<T> criteria)
		{
			T largest = collection.First();

			for (int i = 0; i < collection.Count() - 1; i++)
			{
				if (criteria(collection.ElementAt(i), collection.ElementAt(i + 1)) > 0)
				{
					largest = collection.ElementAt(i);
				}
			}

			return largest;
		}
	}
}
