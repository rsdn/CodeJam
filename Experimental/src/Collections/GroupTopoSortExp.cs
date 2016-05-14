using System;
using System.Collections.Generic;
using System.Linq;

using JetBrains.Annotations;

namespace CodeJam.Collections
{
	[PublicAPI]
	public static class GroupTopoSortExp
	{
		public static IEnumerable<T[]> GroupTopoSort<T>(
				this IEnumerable<T> source,
				Func<T, IEnumerable<T>> dependsOnGetter) =>
			GroupTopoSort(source, dependsOnGetter, EqualityComparer<T>.Default);

		public static IEnumerable<T[]> GroupTopoSort<T>(
				this ICollection<T> source,
				Func<T, IEnumerable<T>> dependsOnGetter) =>
			GroupTopoSort(source, dependsOnGetter, EqualityComparer<T>.Default);

		public static IEnumerable<T[]> GroupTopoSort<T>(
				this IEnumerable<T> source,
				Func<T, IEnumerable<T>> dependsOnGetter,
				IEqualityComparer<T> equalityComparer) =>
			GroupTopoSort(source.ToArray(), dependsOnGetter, equalityComparer);

		public static IEnumerable<T[]> GroupTopoSort<T>(
			this ICollection<T> source,
			Func<T, IEnumerable<T>> dependsOnGetter,
			IEqualityComparer<T> equalityComparer)
		{
			if (source.Count == 0)
			{
				yield break;
			}
			var dependants = LazyDictionary.Create(k => new List<T>(), equalityComparer, false);
			var workArray = new int[source.Count];
			var indexes = new Dictionary<T, int>();
			var level = new List<T>();
			foreach (var item in source.WithIndex())
			{
				var count = 0;
				foreach (var dep in dependsOnGetter(item.Item))
				{
					dependants[dep].Add(item.Item);
					count++;
				}
				if (count == 0)
				{
					level.Add(item.Item);
				}
				else
				{
					workArray[item.Index] = count;
				}
				indexes.Add(item.Item, item.Index);
			}
			if (level.Count == 0)
			{
				throw CodeExceptions.Argument(nameof(source), "Cycle detected.");
			}

			var pendingCount = workArray.Length;
			for (;;)
			{
				var nextLevel = new Lazy<List<T>>(() => new List<T>());
				foreach (var item in level)
				{
					foreach (var dep in dependants[item])
					{
						var pending = --workArray[indexes[dep]];
						if (pending == 0)
						{
							nextLevel.Value.Add(dep);
						}
					}
				}
				yield return level.ToArray();
				pendingCount -= level.Count;
				if (pendingCount == 0)
				{
					yield break;
				}
				if (!nextLevel.IsValueCreated)
				{
					throw CodeExceptions.Argument(nameof(source), "Cycle detected.");
				}
				level = nextLevel.Value;
			}
		}
	}
}