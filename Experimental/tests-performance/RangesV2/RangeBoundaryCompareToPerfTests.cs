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
	[TestFixture(Category = PerfTestsConstants.PerfTestCategory + ": Boundary")]
	[Explicit(PerfTestsConstants.ExplicitExcludeReason)]
	[PublicAPI]
	public class RangeBoundaryCompareToPerfTests
	{
		[Test]
		public void RunIntRangeBoundaryCompareToCase() =>
			CompetitionBenchmarkRunner.Run<IntRangeBoundaryCompareToCase>(RunConfig);

		[PublicAPI]
		public class IntRangeBoundaryCompareToCase : IntRangeBoundaryBaseCase
		{
			[CompetitionBaseline]
			public int Test00DirectCompare()
			{
				var result = 0;
				for (var i = 0; i < ValuesA.Length; i++)
					result = ValuesA[i].CompareTo(ValuesB[i]);
				return result;
			}

			[CompetitionBenchmark(3.89, 4.26)]
			public int Test01Operators()
			{
				var result = 0;
				for (var i = 0; i < ValuesA.Length; i++)
					result = Operators<int>.Compare(ValuesA[i], ValuesB[i]);
				return result;
			}

			[CompetitionBenchmark(4.35, 5.20)]
			public int Test02BoundaryValuesOperators()
			{
				var result = 0;
				for (var i = 0; i < ValuesA.Length; i++)
					result = Operators<int>.Compare(BoundariesA[i].GetValueOrDefault(), BoundariesB[i].GetValueOrDefault());
				return result;
			}

			[CompetitionBenchmark(5.85, 6.30)]
			public int Test03BoundariesCompare()
			{
				var result = 0;
				for (var i = 0; i < ValuesA.Length; i++)
					result = BoundariesA[i].CompareTo(BoundariesB[i]);
				return result;
			}

			[CompetitionBenchmark(5.28, 6.01)]
			public int Test04BoundaryToValueCompare()
			{
				var result = 0;
				for (var i = 0; i < ValuesA.Length; i++)
					result = BoundariesA[i].CompareTo(ValuesB[i]);
				return result;
			}
		}

		[Test]
		public void RunNullableIntRangeBoundaryCompareToCase() =>
			CompetitionBenchmarkRunner.Run<NullableIntRangeBoundaryCompareToCase>(RunConfig);

		[PublicAPI]
		public class NullableIntRangeBoundaryCompareToCase : NullableIntRangeBoundaryBaseCase
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

			[CompetitionBenchmark(2.13, 2.49)]
			public int Test01Operators()
			{
				var result = 0;
				for (var i = 0; i < ValuesA.Length; i++)
					result = Operators<int?>.Compare(ValuesA[i], ValuesB[i]);
				return result;
			}

			[CompetitionBenchmark(3.03, 3.34)]
			public int Test02BoundaryValuesOperators()
			{
				var result = 0;
				for (var i = 0; i < ValuesA.Length; i++)
					result = Operators<int?>.Compare(BoundariesA[i].GetValueOrDefault(), BoundariesB[i].GetValueOrDefault());
				return result;
			}

			[CompetitionBenchmark(2.26, 2.68)]
			public int Test03BoundariesCompare()
			{
				var result = 0;
				for (var i = 0; i < ValuesA.Length; i++)
					result = BoundariesA[i].CompareTo(BoundariesB[i]);
				return result;
			}

			[CompetitionBenchmark(24.76, 29.16)]
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