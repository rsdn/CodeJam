using System;
using System.Collections.Generic;
using System.Linq;

using BenchmarkDotNet.Attributes;

using CodeJam.PerfTests;
using CodeJam.PerfTests.Configs;

using NUnit.Framework;

namespace CodeJam.Collections
{
	[CompetitionMeasureAllocations]
	public class MinOrDefaultWithSelectorPerfTest
	{
		#region PerfTest helpers
		public class Holder<T>
		{
			public Holder(T item)
			{
				Value = item;
			}

			public T Value { get; }
		}

		private static T MinOrDefaultComparer<TSource, T>(
			IEnumerable<TSource> source, Func<TSource, T> selector,
			IComparer<T> comparer, T defaultValue)
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

		private static int MinOrDefaultHardcoded(Holder<int>[] source, int defaultValue)
		{
			Code.NotNull(source, nameof(source));

			if (source.Length == 0)
				return defaultValue;

			var result = source[0].Value;
			for (int i = 1; i < source.Length; i++)
			{
				var candidate = source[i].Value;
				if (result > candidate)
					result = candidate;
			}
			return result;
		}


		private static T MinByOrDefaultGeneric<T, T2>(T[] data, Func<T, T2> selector) =>
			data.MinByOrDefault(selector);
		#endregion

		private Holder<int>[] _data;

		[GlobalSetup]
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

		[CompetitionBenchmark(0.59, 1.27)]
		[GcAllocations(32, BinarySizeUnit.Byte)]
		public int MinOrDefault() => _data.MinOrDefault(i => i.Value);

		[CompetitionBenchmark(0.56, 1.33)]
		[GcAllocations(32, BinarySizeUnit.Byte)]
		public int MinOrDefaultComparer() => MinOrDefaultComparer(_data, i => i.Value, null, 0);

		[CompetitionBenchmark(0.55, 1.41)]
		[GcAllocations(32, BinarySizeUnit.Byte)]
		public Holder<int> MinByOrDefaultGeneric() => MinByOrDefaultGeneric(_data, i => i.Value);

		[CompetitionBenchmark(0.038, 0.129)]
		[GcAllocations(0)]
		public int MinOrDefaultHardcoded() => MinOrDefaultHardcoded(_data, 0);

		[CompetitionBenchmark(0.43, 1.41)]
		[GcAllocations(32, BinarySizeUnit.Byte)]
		public Holder<int> MinByOrDefaultHardcoded() => _data.MinByOrDefault(i => i.Value);
	}
}