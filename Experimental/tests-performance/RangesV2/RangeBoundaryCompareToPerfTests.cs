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
	[PublicAPI]
	public class RangeBoundaryCompareToPerfTests
	{
		[Test]
		[Explicit(PerfTestsConstants.ExplicitExcludeReason)]
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

			[CompetitionBenchmark(4.45, 4.79)]
			public int Test01Operators()
			{
				var result = 0;
				for (var i = 0; i < ValuesA.Length; i++)
					result = Operators<int>.Compare(ValuesA[i], ValuesB[i]);
				return result;
			}

			[CompetitionBenchmark(4.89, 5.23)]
			public int Test02BoundaryValuesOperators()
			{
				var result = 0;
				for (var i = 0; i < ValuesA.Length; i++)
					result = Operators<int>.Compare(BoundariesA[i].GetValueOrDefault(), BoundariesB[i].GetValueOrDefault());
				return result;
			}

			[CompetitionBenchmark(4.38, 4.74)]
			public int Test03BoundariesCompare()
			{
				var result = 0;
				for (var i = 0; i < ValuesA.Length; i++)
					result = BoundariesA[i].CompareTo(BoundariesB[i]);
				return result;
			}

			[CompetitionBenchmark(5.69, 6.06)]
			public int Test04BoundaryToValueCompare()
			{
				var result = 0;
				for (var i = 0; i < ValuesA.Length; i++)
					result = BoundariesA[i].CompareTo(ValuesB[i]);
				return result;
			}
		}

		[Test]
		[Explicit(PerfTestsConstants.ExplicitExcludeReason)]
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

			[CompetitionBenchmark(1.85, 1.99)]
			public int Test01Operators()
			{
				var result = 0;
				for (var i = 0; i < ValuesA.Length; i++)
					result = Operators<int?>.Compare(ValuesA[i], ValuesB[i]);
				return result;
			}

			[CompetitionBenchmark(2.56, 2.74)]
			public int Test02BoundaryValuesOperators()
			{
				var result = 0;
				for (var i = 0; i < ValuesA.Length; i++)
					result = Operators<int?>.Compare(BoundariesA[i].GetValueOrDefault(), BoundariesB[i].GetValueOrDefault());
				return result;
			}

			[CompetitionBenchmark(2.31, 2.47)]
			public int Test03BoundariesCompare()
			{
				var result = 0;
				for (var i = 0; i < ValuesA.Length; i++)
					result = BoundariesA[i].CompareTo(BoundariesB[i]);
				return result;
			}

			[CompetitionBenchmark(25.49, 27.22)]
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