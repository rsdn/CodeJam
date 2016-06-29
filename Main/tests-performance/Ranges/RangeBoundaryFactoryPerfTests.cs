using System;

using CodeJam.PerfTests;

using JetBrains.Annotations;

using NUnit.Framework;

using static CodeJam.AssemblyWideConfig;

namespace CodeJam.Ranges
{
	/// <summary>
	/// 1. Proofs that arg validation skipped when possible.
	/// </summary>
	[TestFixture(Category = PerfTestsConstants.PerfTestCategory + ": Ranges")]
	[Explicit(PerfTestsConstants.ExplicitExcludeReason)]
	[PublicAPI]
	public class RangeBoundaryFactoryPerfTests
	{
		private const int Count = 1000 * 1000;

		[Test]
		public void RunRangeBoundaryFactoryPerfTests() =>
			CompetitionBenchmarkRunner.Run(this, RunConfig);

		[CompetitionBaseline]
		public RangeBoundaryFrom<int> Test00Validated()
		{
			var result = RangeBoundaryFrom<int>.Empty;
			for (var i = 0; i < Count; i++)
				result = new RangeBoundaryFrom<int>(i, RangeBoundaryFromKind.Inclusive);
			return result;
		}

		[CompetitionBenchmark(0.83, 0.90)]
		public RangeBoundaryFrom<int> Test01NoValidation()
		{
			var result = RangeBoundaryFrom<int>.Empty;
			for (var i = 0; i < Count; i++)
				result = Range.BoundaryFrom(i);
			return result;
		}
	}
}