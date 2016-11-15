using System;
using System.Linq;

using CodeJam.Collections;
using CodeJam.PerfTests;

using JetBrains.Annotations;

using NUnit.Framework;

namespace CodeJam
{
	[TestFixture(Category = CompetitionHelpers.PerfTestCategory)]
	[Explicit(CompetitionHelpers.TemporarilyExcludedReason)]
	[PublicAPI]
	public class SequencePerfTests
	{
		public readonly int Count = 100 * 1000;

		[Test]
		public void RunSequencePerfTests() => Competition.Run(this);

		[CompetitionBenchmark(17.03, 23.40)]
		public long TestSequence()
		{
			long result = 0;
			foreach (var i in Sequence.Create(0, i => i <= Count, i => i + 1))
			{
				result += i;
			}

			return result;
		}

		[CompetitionBenchmark(11.15, 14.13)]
		public long TestRange()
		{
			long result = 0;
			foreach (var i in Enumerable.Range(0, Count))
			{
				result += i;
			}

			return result;
		}

		[CompetitionBaseline]
		public long TestRaw()
		{
			long result = 0;
			for (int i = 0; i < Count; i++)
			{
				result += i;
			}

			return result;
		}
	}
}