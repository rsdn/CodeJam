using System;
using System.Collections.Generic;
using System.Linq;

using BenchmarkDotNet.Attributes;

using CodeJam.PerfTests;
using CodeJam.PerfTests.Configs;

using JetBrains.Annotations;

using NUnit.Framework;

namespace CodeJam.Collections
{
	[CompetitionMeasureAllocations]
	public class MinOrDefaultWithSelectorPerfTest
	{
		public class Holder<T>
		{
			public Holder(T item)
			{
				Value = item;
			}

			public T Value { get; }
		}

		private static T MinOrDefaultStruct<TSource, T>(
			IEnumerable<TSource> source, Func<TSource, T> selector)
			where T : struct =>
				MinOrDefaultStruct(source, selector, null, default(T));

		private static T MinOrDefaultStruct<TSource, T>(
			[NotNull, InstantHandle] IEnumerable<TSource> source,
			[NotNull, InstantHandle] Func<TSource, T> selector,
			[CanBeNull]IComparer<T> comparer, T defaultValue)
			where T : struct
		{
			Code.NotNull(source, nameof(source));
			Code.NotNull(selector, nameof(selector));

			comparer = comparer ?? Comparer<T>.Default;

			using (var enumerator = source.GetEnumerator())
			{
				if (!enumerator.MoveNext())
					return defaultValue;

				var result = selector(enumerator.Current);
				while (enumerator.MoveNext())
				{
					var candidate = selector(enumerator.Current);
					if (comparer.Compare(result, candidate) > 0)
						result = candidate;
				}
				return result;
			}
		}

		private Holder<int>[] _data;

		[Setup]
		public void Setup()
		{
			var rnd = new Random(0);
			_data = Enumerable.Range(1, 20).Select(i => new Holder<int>(i)).OrderBy(i => rnd.Next()).ToArray();
		}

		[Test]
		public void RunMinOrDefaultWithSelectorPerfTest() => Competition.Run(this);

		[CompetitionBaseline]
		[GcAllocations(56, BinarySizeUnit.Byte)]
		public int LinqMin() => _data.Min(i => i.Value);

		[CompetitionBenchmark(1.07, 1.15)]
		[GcAllocations(32, BinarySizeUnit.Byte)]
		public int MinOrDefault() => _data.MinOrDefault(i => i.Value);

		[CompetitionBenchmark(1.07, 1.15)]
		[GcAllocations(32, BinarySizeUnit.Byte)]
		public int MinOrDefaultStruct() => MinOrDefaultStruct(_data, i => i.Value);
		
		[CompetitionBenchmark(0.83, 0.87)]
		[GcAllocations(32, BinarySizeUnit.Byte)]
		public Holder<int> MinByOrDefault() => _data.MinByOrDefault(i => i.Value);
	}
}