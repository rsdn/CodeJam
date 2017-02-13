using System;
using System.Collections.Generic;

using CodeJam.PerfTests;

using Microsoft.VisualStudio.TestTools.UnitTesting;

// ReSharper disable once CheckNamespace

namespace CodeJam.Examples.PerfTests
{
	[TestClass]
	public class ListCapacityPerfTest
	{
		private const int Count = 10;

		[TestMethod]
		[TestCategory("PerfTests: MSTest examples")]
		public void RunListCapacityPerfTest() => Competition.Run(this);

		[CompetitionBaseline]
		[GcAllocations(172, BinarySizeUnit.Byte)]
		public int ListWithoutCapacity()
		{
			var data = new List<int>();
			for (var i = 0; i < Count; i++)
				data.Add(i);
			return data.Count;
		}

		[CompetitionBenchmark(0.20, 0.50)]
		[GcAllocations(76, BinarySizeUnit.Byte)]
		public int ListWithCapacity()
		{
			var data = new List<int>(Count);
			for (var i = 0; i < Count; i++)
				data.Add(i);
			return data.Count;
		}
	}
}