using System;
using System.Collections.Generic;

using CodeJam.PerfTests;

using NUnit.Framework;

// ReSharper disable once CheckNamespace

namespace CodeJam.Examples
{
	[Category("PerfTests: NUnit examples")]
	public class ListCapacityPerfTest
	{
		private const int Count = 10;

		[Test]
		public void RunListCapacityPerfTest() => Competition.Run(this);

		[CompetitionBaseline]
		public int ListWithoutCapacity()
		{
			var data = new List<int>();
			for (int i = 0; i < Count; i++)
				data.Add(i);
			return data.Count;
		}

		[CompetitionBenchmark(0.20, 0.50)]
		public int ListWithCapacity()
		{
			var data = new List<int>(Count);
			for (int i = 0; i < Count; i++)
				data.Add(i);
			return data.Count;
		}
	}
}