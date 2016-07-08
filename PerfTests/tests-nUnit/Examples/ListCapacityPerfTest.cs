using System;
using System.Collections.Generic;

using CodeJam.PerfTests;

using NUnit.Framework;

// ReSharper disable once CheckNamespace

namespace CodeJam.Examples
{
	[Category("PerfTests: examples")]
	public class ListCapacityPerfTest
	{
		private const int Count = 10000;

		[Test]
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

		[CompetitionBenchmark(0.71, 0.73)]
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