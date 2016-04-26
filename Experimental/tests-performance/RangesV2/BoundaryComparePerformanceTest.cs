using System;
using System.Collections.Generic;

using BenchmarkDotNet.NUnit;

using CodeJam.Arithmetic;

using JetBrains.Annotations;

using NUnit.Framework;

using static CodeJam.AssemblyWideConfig;

namespace CodeJam.RangesV2
{
	/// <summary>
	/// Checks:
	/// 1. Proofs that there's no way to make RangeBoundary (of T) faster.
	/// </summary>
	[TestFixture(Category = BenchmarkConstants.BenchmarkCategory + ": Boundary")]
	[PublicAPI]
	public class BoundaryComparePerformanceTest
	{
		[Test]
		[Explicit(BenchmarkConstants.ExplicitExcludeReason)]
		public void BenchmarkBoundaryComparisonInt() =>
			CompetitionBenchmarkRunner.Run<IntCase>(RunConfig);

		[PublicAPI]
		public class IntCase : IntBoundaryBenchmark
		{
			[CompetitionBaseline]
			public int Test00DirectCompare()
			{
				var result = 0;
				for (var i = 0; i < ValuesA.Length; i++)
					result = ValuesA[i].CompareTo(ValuesB[i]);
				return result;
			}

			[CompetitionBenchmark(4.42, 4.70)]
			public int Test01Operators()
			{
				var result = 0;
				for (var i = 0; i < ValuesA.Length; i++)
					result = Operators<int>.Compare(ValuesA[i], ValuesB[i]);
				return result;
			}

			[CompetitionBenchmark(4.75, 5.20)]
			public int Test02BoundaryValuesOperators()
			{
				var result = 0;
				for (var i = 0; i < ValuesA.Length; i++)
					result = Operators<int>.Compare(BoundariesA[i].GetValueOrDefault(), BoundariesB[i].GetValueOrDefault());
				return result;
			}

			[CompetitionBenchmark(12.67, 13.66)]
			public int Test03BoundariesCompare()
			{
				var result = 0;
				for (var i = 0; i < ValuesA.Length; i++)
					result = BoundariesA[i].CompareTo(BoundariesB[i]);
				return result;
			}

			[CompetitionBenchmark(9.24, 9.87)]
			public int Test04BoundaryToValueCompare()
			{
				var result = 0;
				for (var i = 0; i < ValuesA.Length; i++)
					result = BoundariesA[i].CompareTo(ValuesB[i]);
				return result;
			}
		}

		[Test]
		[Explicit(BenchmarkConstants.ExplicitExcludeReason)]
		public void BenchmarkBoundaryComparisonNullableInt() =>
			CompetitionBenchmarkRunner.Run<NullableIntCase>(RunConfig);

		[PublicAPI]
		public class NullableIntCase : NullableIntBoundaryBenchmark
		{
			[CompetitionBaseline]
			public int Test00DirectCompare()
			{
				var comparer = Comparer<int?>.Default;
				var result = 0;
				for (var i = 0; i < ValuesA.Length; i++)
					result = comparer.Compare(ValuesA[i], ValuesB[i]);
				return result;
			}

			[CompetitionBenchmark(1.88, 2.03)]
			public int Test01Operators()
			{
				var result = 0;
				for (var i = 0; i < ValuesA.Length; i++)
					result = Operators<int?>.Compare(ValuesA[i], ValuesB[i]);
				return result;
			}

			[CompetitionBenchmark(2.71, 2.90)]
			public int Test02BoundaryValuesOperators()
			{
				var result = 0;
				for (var i = 0; i < ValuesA.Length; i++)
					result = Operators<int?>.Compare(BoundariesA[i].GetValueOrDefault(), BoundariesB[i].GetValueOrDefault());
				return result;
			}

			[CompetitionBenchmark(5.34, 5.71)]
			public int Test03BoundariesCompare()
			{
				var result = 0;
				for (var i = 0; i < ValuesA.Length; i++)
					result = BoundariesA[i].CompareTo(BoundariesB[i]);
				return result;
			}

			[CompetitionBenchmark(28.42, 30.44)]
			public int Test04BoundaryToValueCompare()
			{
				var result = 0;
				for (var i = 0; i < ValuesA.Length; i++)
					result = BoundariesA[i].CompareTo(ValuesB[i]);
				return result;
			}
		}
	}
}