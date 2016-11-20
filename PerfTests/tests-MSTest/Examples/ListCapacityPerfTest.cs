using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

using CodeJam.PerfTests;

using Microsoft.VisualStudio.TestTools.UnitTesting;

// ReSharper disable once CheckNamespace

namespace CodeJam.Examples
{
	[TestClass]
	[SuppressMessage("ReSharper", "SuggestVarOrType_BuiltInTypes")]
	public class ListCapacityPerfTest
	{
		private const int Count = 10;

		[TestMethod]
		[TestCategory("PerfTests: MSTest examples")]
		public void RunListCapacityPerfTest() => Competition.Run(this);

		[CompetitionBaseline]
		public int ListWithoutCapacity()
		{
			var data = new List<int>();
			for (int i = 0; i < Count; i++)
				data.Add(i);
			return data.Count;
		}

		[CompetitionBenchmark(0.20, 0.45)]
		public int ListWithCapacity()
		{
			var data = new List<int>(Count);
			for (int i = 0; i < Count; i++)
				data.Add(i);
			return data.Count;
		}
	}
}