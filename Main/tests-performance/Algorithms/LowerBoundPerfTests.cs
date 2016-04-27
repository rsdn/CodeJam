using System;
using System.Collections.Generic;

using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.NUnit;

using CodeJam.Collections;

using JetBrains.Annotations;

using NUnit.Framework;

namespace CodeJam.Algorithms
{
	[TestFixture(Category = PerfTestsConstants.PerfTestCategory)]
	[Explicit(PerfTestsConstants.ExplicitExcludeReason)]
	[PublicAPI]
	public class LowerBoundPerfTests
	{
		private const int Steps = 10;

		private double[] _testData;
		private int _increment;

		[Params(1000, 10 * 1000, 100 * 1000, 1000 * 1000)]
		public int Count { get; set; }

		[Setup]
		public void SetupData()
		{
			var rnd = new Random();
			_testData = new double[Count];
			for (var i = 0; i < _testData.Length; ++i)
			{
				_testData[i] = rnd.Next(int.MaxValue) * 0.001;
			}
			_testData.Sort();
			_increment = _testData.Length / Steps;
		}

		[Test]
		public void RunLowerBoundPerfTests() => CompetitionBenchmarkRunner.Run(this, AssemblyWideConfig.RunConfig);

		[CompetitionBaseline]
		public void Test00IComparable()
		{
			for (var i = 0; i < Steps; ++i)
			{
				var target = _testData[_increment * i];
				LowerBoundIComparable(_testData, target, 0, _testData.Length);
			}
		}

		[CompetitionBenchmark(1.20, 1.54)]
		public void Test01Comparer()
		{
			for (var i = 0; i < Steps; ++i)
			{
				var target = _testData[_increment * i];
				// ReSharper disable once ReturnValueOfPureMethodIsNotUsed
				_testData.LowerBound(target, 0, _testData.Length, Comparer<double>.Default.Compare);
			}
		}

		[CompetitionBenchmark(0.55, 0.77)]
		public void Test02DirectTypeCompare()
		{
			for (var i = 0; i < Steps; ++i)
			{
				var target = _testData[_increment * i];
				// ReSharper disable once ReturnValueOfPureMethodIsNotUsed
				_testData.LowerBound(target, 0, _testData.Length);
			}
		}

		private static int LowerBoundIComparable<T>(IList<T> list, T value, int from, int to) where T : IComparable<T>
		{
			ValidateIndicesRange(from, to, list.Count);
			while (from < to)
			{
				var median = from + (to - from) / 2;
				var compareResult = list[median].CompareTo(value);
				if (compareResult >= 0)
				{
					to = median;
				}
				else
				{
					from = median + 1;
				}
			}
			return from;
		}

		internal static void ValidateIndicesRange(int from, int to, int count)
		{
			if (from < 0)
			{
				throw new ArgumentOutOfRangeException(nameof(from), $"The {nameof(from)} index should be non-negative");
			}

			if (to < 0)
			{
				throw new ArgumentOutOfRangeException(nameof(to), $"The {nameof(to)} index should be non-negative");
			}
			if (to > count)
			{
				throw new ArgumentOutOfRangeException(nameof(to), $"The {nameof(to)} index should not exceed the {nameof(count)}");
			}
			if (to < from)
			{
				throw new ArgumentException(nameof(to), $"The {nameof(to)} index should be not less than the {nameof(from)} index");
			}
		}
	}
}