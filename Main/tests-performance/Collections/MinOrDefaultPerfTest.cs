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
	public class MinOrDefaultPerfTest
	{
		#region PerfTest helpers
		private static TSource MinOrDefaultComparer<TSource>(
			IEnumerable<TSource> source, IComparer<TSource> comparer, TSource defaultValue)
			where TSource : struct
		{
			Code.NotNull(source, nameof(source));
			comparer = comparer ?? Comparer<TSource>.Default;

			using (var enumerator = source.GetEnumerator())
			{
				if (!enumerator.MoveNext())
					return defaultValue;

				var result = enumerator.Current;
				while (enumerator.MoveNext())
				{
					var candidate = enumerator.Current;
					if (comparer.Compare(result, candidate) > 0)
						result = candidate;
				}
				return result;
			}
		}

		private static int MinOrDefaultHardcoded(int[] source, int defaultValue)
		{
			Code.NotNull(source, nameof(source));

			if (source.Length == 0)
				return defaultValue;

			var result = source[0];
			for (int i = 1; i < source.Length; i++)
			{
				var candidate = source[i];
				if (result > candidate)
					result = candidate;
			}
			return result;
		}

		private static T MinByOrDefaultGeneric<T, T2>(T[] data, Func<T, T2> selector) =>
			data.MinByOrDefault(selector);
		#endregion

		private int[] _data;

		[Setup]
		public void Setup()
		{
			var rnd = new Random(0);
			_data = Enumerable.Range(1, 20).OrderBy(i => rnd.Next()).ToArray();
		}

		[Test]
		public void RunMinOrDefaultPerfTest() => Competition.Run(this);

		[CompetitionBaseline]
		[GcAllocations(32, BinarySizeUnit.Byte)]
		public int LinqMin() => _data.Min();

		[CompetitionBenchmark(1.07, 1.48)]
		[GcAllocations(32, BinarySizeUnit.Byte)]
		public int MinOrDefault() => _data.MinOrDefault();

		[CompetitionBenchmark(1.20, 1.46)]
		[GcAllocations(32, BinarySizeUnit.Byte)]
		public int MinOrDefaultComparer() => MinOrDefaultComparer(_data, null, 0);

		[CompetitionBenchmark(1.67, 1.96)]
		[GcAllocations(32, BinarySizeUnit.Byte)]
		public int MinByOrDefaultGeneric() => MinByOrDefaultGeneric(_data, i => i);

		[CompetitionBenchmark(0.077, 0.152)]
		[GcAllocations(0)]
		public int MinOrDefaultHardcoded() => MinOrDefaultHardcoded(_data, 0);

		[CompetitionBenchmark(1.02, 1.28)]
		[GcAllocations(32, BinarySizeUnit.Byte)]
		public int MinByOrDefaultHardcoded() => _data.MinByOrDefault(i => i);
	}
}