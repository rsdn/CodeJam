using System;
using System.Collections.Generic;

using CodeJam.Arithmetic;
using CodeJam.PerfTests;

using JetBrains.Annotations;

using NUnit.Framework;

using static CodeJam.PerfTests.CompetitionHelpers;

namespace CodeJam.Ranges
{
	/// <summary>
	/// Checks:
	/// 1. Proofs that there's no way to make RangeBoundary (of T) faster.
	/// </summary>
	[TestFixture(Category = PerfTestCategory + ": Ranges")]
	[CompetitionBurstMode]
	public class RangeBoundaryCompareToPerfTests
	{
		[Test]
		public void RunRangeBoundaryCompareToIntCase() =>
			Competition.Run<RangeBoundaryCompareToIntCase>();

		[PublicAPI]
		public class RangeBoundaryCompareToIntCase : IntRangeBoundaryBaseCase
		{
			[CompetitionBaseline]
			[GcAllocations(0)]
			public int Test00DirectCompare()
			{
				var result = 0;
				for (var i = 0; i < ValuesA.Length; i++)
					result = ValuesA[i].CompareTo(ValuesB[i]);
				return result;
			}

			[CompetitionBenchmark(2.91, 6.75)]
			[GcAllocations(0)]
			public int Test01Operators()
			{
				var result = 0;
				for (var i = 0; i < ValuesA.Length; i++)
					result = Operators<int>.Compare(ValuesA[i], ValuesB[i]);
				return result;
			}

			[CompetitionBenchmark(2.78, 8.27)]
			[GcAllocations(0)]
			public int Test02BoundaryValuesOperators()
			{
				var result = 0;
				for (var i = 0; i < ValuesA.Length; i++)
					result = Operators<int>.Compare(BoundariesFromA[i].GetValueOrDefault(), BoundariesFromB[i].GetValueOrDefault());
				return result;
			}

			[CompetitionBenchmark(1.66, 4.06)]
			[GcAllocations(0)]
			public int Test03BoundariesCompareFrom()
			{
				var result = 0;
				for (var i = 0; i < ValuesA.Length; i++)
					result = BoundariesFromA[i].CompareTo(BoundariesFromB[i]);
				return result;
			}

			[CompetitionBenchmark(1.54, 4.33)]
			[GcAllocations(0)]
			public int Test03BoundariesCompareFromTo()
			{
				var result = 0;
				for (var i = 0; i < ValuesA.Length; i++)
					result = BoundariesFromA[i].CompareTo(BoundariesToB[i]);
				return result;
			}

			[CompetitionBenchmark(5.62, 13.80)]
			[GcAllocations(0)]
			public int Test04BoundaryToValueCompare()
			{
				var result = 0;
				for (var i = 0; i < ValuesA.Length; i++)
					result = BoundariesFromA[i].CompareTo(ValuesB[i]);
				return result;
			}
		}

		[Test]
		public void RunRangeBoundaryCompareToNullableIntCase() =>
			Competition.Run<RangeBoundaryCompareToNullableIntCase>();

		[PublicAPI]
		public class RangeBoundaryCompareToNullableIntCase : NullableIntRangeBoundaryBaseCase
		{
			[CompetitionBaseline]
			[GcAllocations(0)]
			public int Test00DirectCompare()
			{
				var comparer = Comparer<int?>.Default;
				var result = 0;
				for (var i = 0; i < ValuesA.Length; i++)
					result = comparer.Compare(ValuesA[i], ValuesB[i]);
				return result;
			}

			[CompetitionBenchmark(1.35, 3.71)]
			[GcAllocations(0)]
			public int Test01Operators()
			{
				var result = 0;
				for (var i = 0; i < ValuesA.Length; i++)
					result = Operators<int?>.Compare(ValuesA[i], ValuesB[i]);
				return result;
			}

			[CompetitionBenchmark(2.26, 4.11)]
			[GcAllocations(0)]
			public int Test02BoundaryValuesOperators()
			{
				var result = 0;
				for (var i = 0; i < ValuesA.Length; i++)
					result = Operators<int?>.Compare(BoundariesFromA[i].GetValueOrDefault(), BoundariesFromB[i].GetValueOrDefault());
				return result;
			}

			[CompetitionBenchmark(1.20, 2.95)]
			[GcAllocations(0)]
			public int Test03BoundariesCompareFrom()
			{
				var result = 0;
				for (var i = 0; i < ValuesA.Length; i++)
					result = BoundariesFromA[i].CompareTo(BoundariesFromB[i]);
				return result;
			}

			[CompetitionBenchmark(1.27, 2.60)]
			[GcAllocations(0)]
			public int Test03BoundariesCompareFromTo()
			{
				var result = 0;
				for (var i = 0; i < ValuesA.Length; i++)
					result = BoundariesFromA[i].CompareTo(BoundariesToB[i]);
				return result;
			}

			[CompetitionBenchmark(3.44, 6.64)]
			[GcAllocations(0)]
			public int Test04BoundaryToValueCompare()
			{
				var result = 0;
				for (var i = 0; i < ValuesA.Length; i++)
					result = BoundariesFromA[i].CompareTo(ValuesB[i]);
				return result;
			}
		}
	}
}