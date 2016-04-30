using System;

using BenchmarkDotNet.NUnit;

using JetBrains.Annotations;

using NUnit.Framework;

using static CodeJam.AssemblyWideConfig;

namespace CodeJam.RangesV2
{
	/// <summary>
	/// 1. Proofs that arg validation skipped when possible.
	/// </summary>
	[TestFixture(Category = PerfTestsConstants.PerfTestCategory + ": Boundary")]
	[Explicit(PerfTestsConstants.ExplicitExcludeReason)]
	[PublicAPI]
	public class RangeBoundaryFactoryPerfTests
	{
		private const int Count = 1000 * 1000;

		[Test]
		public void RunRangeBoundaryFactoryPerfTests() =>
			CompetitionBenchmarkRunner.Run(this, RunConfig);

		[CompetitionBaseline]
		public RangeBoundary<int> Test00Validated()
		{
			var result = RangeBoundary<int>.Empty;
			for (var i = 0; i < Count; i++)
				result = new RangeBoundary<int>(i, RangeBoundaryKind.FromInclusive);
			return result;
		}

		[CompetitionBenchmark(0.43, 0.48)]
		public RangeBoundary<int> Test01NoValidation()
		{
			var result = RangeBoundary<int>.Empty;
			for (var i = 0; i < Count; i++)
				result = Range.BoundaryFrom(i);
			return result;
		}
	}
}