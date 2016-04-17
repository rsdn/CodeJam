using System;
using System.Collections.Generic;

using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.NUnit;

using CodeJam.Collections;

using JetBrains.Annotations;

using NUnit.Framework;

namespace CodeJam.Algorithms
{
	[TestFixture(Category = BenchmarkConstants.BenchmarkCategory)]
	[Explicit(BenchmarkConstants.ExplicitExcludeReason)]
	[PublicAPI]
	public class LowerBoundBenchmark
	{
		private static double[] testData_;
		private const int Steps = 10;
		private static int increment_;

		[Test]
		[Explicit(BenchmarkConstants.ExplicitExcludeReason)]
		public void BenchmarkSensitivity() => CompetitionBenchmarkRunner.Run(this, AssemblyWideConfig.RunConfig);

		[Params(1000, 10 * 1000, 100 * 1000, 1000 * 1000)]
		public int Count { get; set; }

		[Setup]
		public void SetupData()
		{
			var rnd = new Random();
			testData_ = new double[Count];
			for (var i = 0; i < testData_.Length; ++i)
			{
				testData_[i] = rnd.Next(int.MaxValue) * 0.001;
			}
			testData_.Sort();
			increment_ = testData_.Length / Steps;
		}

		[Test]
		public void TestLowerBound() => CompetitionBenchmarkRunner.Run(this, AssemblyWideConfig.RunConfig);

		[CompetitionBenchmark(1.25, 1.5)]
		public void Test00Comparer()
		{
			for (var i = 0; i < Steps; ++i)
			{
				var target = testData_[increment_ * i];
				testData_.LowerBound(target, 0, testData_.Length, Comparer<double>.Default.Compare);
			}
		}

		[CompetitionBaseline]
		public void Test01IComparable()
		{
			for (var i = 0; i < Steps; ++i)
			{
				var target = testData_[increment_ * i];
				LowerBoundIComparable(testData_, target, 0, testData_.Length);
			}
		}

		[CompetitionBenchmark(0.65, 0.75)]
		public void Test02DirectTypeCompare()
		{
			for (var i = 0; i < Steps; ++i)
			{
				var target = testData_[increment_ * i];
				testData_.LowerBound(target, 0, testData_.Length);
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