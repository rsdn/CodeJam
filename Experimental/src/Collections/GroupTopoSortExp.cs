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
			this IEnumerable<T> source,
			Func<T, IEnumerable<T>> dependsOnGetter,
			IEqualityComparer<T> equalityComparer)
		{
			var workArray =
				source
					.Select((item, idx) => new TopoSortWorkItem<T>(idx, item, dependsOnGetter(item).ToArray()))
					.ToArray();
			var indexes =
				workArray
					.ToDictionary(i => i.Item, i => i.Index, equalityComparer);
			foreach (var item in workArray)
				foreach (var child in item.Children)
					workArray[indexes[child]].Processed++;

				var completedCounter = 0;

			while (completedCounter != workArray.Length)
			{
				var level = new List<T>();

				// Во избежание обработки вершин, с нулевой
				// степенью захода, возникших по ходу следующего цикла,
				// помечаем их заранее.
				for (var i = 0; i < workArray.Length; i++)
					if (workArray[i].Processed == 0)
						workArray[i].Processed = null;

				for (var i = 0; i < workArray.Length; i++)
					if (workArray[i].Processed == null)
					{
						// Если вершину следует обработать, помещаем её
						// В соответствующий ей уровень и корректируем
						// Массив степеней захода остальных вершин
						level.Add(workArray[i].Item);

						foreach (var node in workArray[i].Children)
						{
							var linkedNode = indexes[node];
							workArray[linkedNode].Processed--;
						}

						workArray[i].Processed = -1; // Помечаем вершину как обработанную

						completedCounter++;
					}

				yield return level.ToArray();
			}
		}

		private struct TopoSortWorkItem<T>
		{
			public TopoSortWorkItem(int index, T item, T[] children) : this()
			{
				Children = children;
				Index = index;
				Item = item;
				Processed = 0;
			}

			public int Index { get; }
			public T Item { get; }
			public T[] Children { get; }
			public int? Processed { get; set; }
		}
	}
}