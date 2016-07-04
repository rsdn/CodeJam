using System;

using CodeJam.PerfTests;

using JetBrains.Annotations;

using NUnit.Framework;

using static CodeJam.AssemblyWideConfig;
using static CodeJam.PerfTests.CompetitionHelpers;

namespace CodeJam.Ranges
{
	/// <summary>
	/// 1. Proofs that arg validation skipped when possible.
	/// </summary>
	[TestFixture(Category = PerfTestCategory + ": Ranges")]
	[PublicAPI]
	public class RangeBoundaryFactoryPerfTests
	{
		private const int Count = DefaultCount;

		[Test]
		public void RunRangeBoundaryFactoryPerfTests() => Competition.Run(this, RunConfig);

		[CompetitionBaseline]
		public RangeBoundaryFrom<int> Test00Validated()
		{
			var result = RangeBoundaryFrom<int>.Empty;
			for (var i = 0; i < Count; i++)
				result = new RangeBoundaryFrom<int>(i, RangeBoundaryFromKind.Inclusive);
			return result;
		}

		[CompetitionBenchmark(0.88, 0.94)]
		public RangeBoundaryFrom<int> Test01NoValidation()
		{
			var result = RangeBoundaryFrom<int>.Empty;
			for (var i = 0; i < Count; i++)
				result = Range.BoundaryFrom(i);
			return result;
		}
	}
}