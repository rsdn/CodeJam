using System;
using System.Collections.Generic;

using JetBrains.Annotations;

namespace CodeJam.Collections
{
	partial class EnumerableExtensions
	{
		/// <summary>
		/// Returns a flattened sequence from a graph or hierarchy of elements by using the specified children selector.
		/// </summary>
		/// <param name="source">The source hierarchy to flatten.</param>
		/// <param name="childrenSelector">A function used to retrieve the children of an element.</param>
		/// <returns>
		/// A flat sequence of elements produced from the elements in the source hierarchy.
		/// </returns>
		[NotNull, Pure]
		public static IEnumerable<T> Flatten<T>(
			[NotNull] this IEnumerable<T> source,
			[NotNull] Func<T, IEnumerable<T>> childrenSelector)
		{
			if (source == null)
				throw new ArgumentNullException(nameof (source));

			if (childrenSelector == null)
				throw new ArgumentNullException(nameof (childrenSelector));

			return FlattenImpl(source, childrenSelector);
		}

		private static IEnumerable<T> FlattenImpl<T>(this IEnumerable<T> source, Func<T, IEnumerable<T>> childrenSelector)
		{
			foreach (var root in source)
			{
				var list = new Node<T>(root, null);
				while (list != null)
				{
					var currentNode = list;
					var nextNode = list.Next;

					yield return currentNode.Item;

					Node<T> tmpList = null;
					foreach (var childItem in childrenSelector(currentNode.Item))
						tmpList = new Node<T>(childItem, tmpList);

					for (; tmpList != null; tmpList = tmpList.Next)
						nextNode = new Node<T>(tmpList.Item, nextNode);

					list = nextNode;
				}
			}
		}

		#region Inner type: Node
		private sealed class Node<T>
		{
			public readonly T Item;
			public readonly Node<T> Next;

			public Node(T item, Node<T> next)
			{
				Item = item;
				Next = next;
			}
		}
		#endregion
	}
}
