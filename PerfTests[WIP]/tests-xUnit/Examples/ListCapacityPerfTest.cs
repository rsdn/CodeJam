using System;
using System.Collections.Generic;

using CodeJam.PerfTests;
using CodeJam.PerfTests.Configs;

using Xunit;

// ReSharper disable once CheckNamespace

namespace CodeJam.Examples.PerfTests
{
	[Trait("Category", "PerfTests: xUnit examples")]
	[CompetitionIgnoreAllocations]
	public class ListCapacityPerfTest
	{
		private const int Count = 10;

		[CompetitionFact]
		public void RunListCapacityPerfTest() => Competition.Run(this);

		[CompetitionBaseline]
		public int ListWithoutCapacity()
		{
			var data = new List<int>();
			for (var i = 0; i < Count; i++)
				data.Add(i);
			return data.Count;
		}

		[CompetitionBenchmark(0.20, 0.50)]
		public int ListWithCapacity()
		{
			var data = new List<int>(Count);
			for (var i = 0; i < Count; i++)
				data.Add(i);
			return data.Count;
		}
	}
}