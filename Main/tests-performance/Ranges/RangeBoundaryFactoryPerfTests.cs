using System;

using CodeJam.PerfTests;

using JetBrains.Annotations;

using NUnit.Framework;

using static CodeJam.PerfTests.CompetitionHelpers;

namespace CodeJam.Ranges
{
	/// <summary>
	/// 1. Proofs that arg validation skipped when possible.
	/// </summary>
	[TestFixture(Category = PerfTestCategory + ": Ranges")]
	[PublicAPI]
	[CompetitionBurstMode]
	public class RangeBoundaryFactoryPerfTests
	{
		private static readonly int Count = CompetitionRunHelpers.SmallLoopCount;

		[Test]
		public void RunRangeBoundaryFactoryPerfTests() => Competition.Run(this);

		[CompetitionBaseline]
		[GcAllocations(0)]
		public RangeBoundaryFrom<int> Test00Validated()
		{
			var result = RangeBoundaryFrom<int>.Empty;
			for (var i = 0; i < Count; i++)
				result = new RangeBoundaryFrom<int>(i, RangeBoundaryFromKind.Inclusive);
			return result;
		}

		[CompetitionBenchmark(0.42, 1.04)]
		[GcAllocations(0)]
		public RangeBoundaryFrom<int> Test01NoValidation()
		{
			var result = RangeBoundaryFrom<int>.Empty;
			for (var i = 0; i < Count; i++)
				result = Range.BoundaryFrom(i);
			return result;
		}
	}
}