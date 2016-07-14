using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

using CodeJam.PerfTests;

using Xunit;

// ReSharper disable once CheckNamespace

namespace CodeJam.Examples
{
	[Trait("Category", "PerfTests: xUnit examples")]
	[SuppressMessage("ReSharper", "SuggestVarOrType_BuiltInTypes")]
	public class ListCapacityPerfTest
	{
		private const int Count = 10000;

		[CompetitionFact]
		public void RunListCapacityPerfTest() => Competition.Run(this, CompetitionHelpers.DefaultConfigAnnotate);

		[CompetitionBaseline]
		public int ListWithoutCapacity()
		{
			var data = new List<int>();
			for (int i = 0; i < Count; i++)
			{
				data.Add(i);
			}
			return data.Count;
		}

		[CompetitionBenchmark(0.66, 0.79)]
		public int ListWithCapacity()
		{
			var data = new List<int>(Count);
			for (int i = 0; i < Count; i++)
			{
				data.Add(i);
			}
			return data.Count;
		}
	}
}