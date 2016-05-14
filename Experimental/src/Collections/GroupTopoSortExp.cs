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
			var dependants = LazyDictionary.Create(k => new List<T>(), equalityComparer, false);
			var workArray = new TopoSortWorkItem<T>[source.Count];
			var indexes = new Dictionary<T, int>();
			foreach (var item in source.WithIndex())
			{
				var count = 0;
				foreach (var dep in dependsOnGetter(item.Item))
				{
					dependants[dep].Add(item.Item);
					count++;
				}
				workArray[item.Index] = new TopoSortWorkItem<T>(item.Item, count);
				indexes.Add(item.Item, item.Index);
			}

			var completedCounter = 0;

			while (completedCounter < workArray.Length)
			{
				var level = new List<T>();

				for (var i = 0; i < workArray.Length; i++)
					if (workArray[i].Processed == 0)
						workArray[i].Processed = null;

				var lastCounter = completedCounter;
				for (var i = 0; i < workArray.Length; i++)
					if (workArray[i].Processed == null)
					{
						level.Add(workArray[i].Item);

						foreach (var dep in dependants[workArray[i].Item])
							workArray[indexes[dep]].Processed--;

						workArray[i].Processed = -1;
						completedCounter++;
					}
				if (completedCounter == lastCounter)
					throw CodeExceptions.Argument(nameof(source), "Cycle detected.");

				yield return level.ToArray();
			}
		}

		private struct TopoSortWorkItem<T>
		{
			public TopoSortWorkItem(T item, int? processed) : this()
			{
				Item = item;
				Processed = processed;
			}

			public T Item { get; }
			public int? Processed { get; set; }
		}
	}
}