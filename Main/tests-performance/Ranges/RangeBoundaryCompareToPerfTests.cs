using System;
using System.Collections.Generic;

using CodeJam.Arithmetic;
using CodeJam.PerfTests;

using JetBrains.Annotations;

using NUnit.Framework;

using static CodeJam.AssemblyWideConfig;
using static CodeJam.PerfTests.CompetitionHelpers;

namespace CodeJam.Ranges
{
	/// <summary>
	/// Checks:
	/// 1. Proofs that there's no way to make RangeBoundary (of T) faster.
	/// </summary>
	[TestFixture(Category = PerfTestCategory + ": Ranges")]
	[PublicAPI]
	public class RangeBoundaryCompareToPerfTests
	{
		[Test]
		public void RunRangeBoundaryCompareToIntCase() =>
			Competition.Run<RangeBoundaryCompareToIntCase>(RunConfig);

		[PublicAPI]
		public class RangeBoundaryCompareToIntCase : IntRangeBoundaryBaseCase
		{
			[CompetitionBaseline]
			public int Test00DirectCompare()
			{
				var result = 0;
				for (var i = 0; i < ValuesA.Length; i++)
					result = ValuesA[i].CompareTo(ValuesB[i]);
				return result;
			}

			[CompetitionBenchmark(3.80, 5.15)]
			public int Test01Operators()
			{
				var result = 0;
				for (var i = 0; i < ValuesA.Length; i++)
					result = Operators<int>.Compare(ValuesA[i], ValuesB[i]);
				return result;
			}

			[CompetitionBenchmark(3.97, 5.48)]
			public int Test02BoundaryValuesOperators()
			{
				var result = 0;
				for (var i = 0; i < ValuesA.Length; i++)
					result = Operators<int>.Compare(BoundariesFromA[i].GetValueOrDefault(), BoundariesFromB[i].GetValueOrDefault());
				return result;
			}

			[CompetitionBenchmark(2.45, 3.19)]
			public int Test03BoundariesCompareFrom()
			{
				var result = 0;
				for (var i = 0; i < ValuesA.Length; i++)
					result = BoundariesFromA[i].CompareTo(BoundariesFromB[i]);
				return result;
			}

			[CompetitionBenchmark(2.39, 3.36)]
			public int Test03BoundariesCompareFromTo()
			{
				var result = 0;
				for (var i = 0; i < ValuesA.Length; i++)
					result = BoundariesFromA[i].CompareTo(BoundariesToB[i]);
				return result;
			}

			[CompetitionBenchmark(8.92, 12.38)]
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
			Competition.Run<RangeBoundaryCompareToNullableIntCase>(RunConfig);

		[PublicAPI]
		public class RangeBoundaryCompareToNullableIntCase : NullableIntRangeBoundaryBaseCase
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

			[CompetitionBenchmark(2.23, 2.55)]
			public int Test01Operators()
			{
				var result = 0;
				for (var i = 0; i < ValuesA.Length; i++)
					result = Operators<int?>.Compare(ValuesA[i], ValuesB[i]);
				return result;
			}

			[CompetitionBenchmark(3.51, 4.00)]
			public int Test02BoundaryValuesOperators()
			{
				var result = 0;
				for (var i = 0; i < ValuesA.Length; i++)
					result = Operators<int?>.Compare(BoundariesFromA[i].GetValueOrDefault(), BoundariesFromB[i].GetValueOrDefault());
				return result;
			}

			[CompetitionBenchmark(1.97, 2.48)]
			public int Test03BoundariesCompareFrom()
			{
				var result = 0;
				for (var i = 0; i < ValuesA.Length; i++)
					result = BoundariesFromA[i].CompareTo(BoundariesFromB[i]);
				return result;
			}

			[CompetitionBenchmark(1.89, 2.33)]
			public int Test03BoundariesCompareFromTo()
			{
				var result = 0;
				for (var i = 0; i < ValuesA.Length; i++)
					result = BoundariesFromA[i].CompareTo(BoundariesToB[i]);
				return result;
			}

			[CompetitionBenchmark(5.53, 6.64)]
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