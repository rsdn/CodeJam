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
	public class MinOrDefaultPerfTest
	{
		private static TSource MinOrDefaultStruct<TSource>(
			IEnumerable<TSource> source)
			where TSource : struct =>
				MinOrDefaultStruct(source, null, default(TSource));

		private static TSource MinOrDefaultStruct<TSource>(
			IEnumerable<TSource> source, [CanBeNull] IComparer<TSource> comparer, TSource defaultValue)
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

		[CompetitionBenchmark(1.25, 1.48)]
		[GcAllocations(32, BinarySizeUnit.Byte)]
		public int MinOrDefault() => _data.MinOrDefault();

		[CompetitionBenchmark(1.24, 1.46)]
		[GcAllocations(32, BinarySizeUnit.Byte)]
		public int MinOrDefaultStruct() => MinOrDefaultStruct(_data);

		[CompetitionBenchmark(1.02, 1.23)]
		[GcAllocations(32, BinarySizeUnit.Byte)]
		public int MinByOrDefault() => _data.MinByOrDefault(i => i);
	}
}