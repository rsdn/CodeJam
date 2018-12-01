using System;
using System.Threading;

using CodeJam.PerfTests;
using CodeJam.PerfTests.Configs;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using static CodeJam.PerfTests.CompetitionRunHelpers;
// ReSharper disable once CheckNamespace

namespace CodeJam.Examples.PerfTests
{
	// A perf test class.
	[TestClass]
	[CompetitionMaximumReruns(10)]
	[CompetitionNoRelativeTime]
	public class SimpleAllocPerfTest
	{
		private static readonly int _count = SmallLoopCount;

		[TestMethod]
		[TestCategory("PerfTests: MSTest examples")]
		public void RunSimpleAllocPerfTest() => Competition.Run(this);

		[CompetitionBaseline, GcAllocations(0)]
		public void Baseline() => Thread.SpinWait(_count);

		[CompetitionBenchmark, GcAllocations(256, BinarySizeUnit.Byte)]
		public void Allocates256BytesArray()
		{
			var array = new byte[256 - SizeOfSzArrayHeader];
			Thread.SpinWait(_count);
			GC.KeepAlive(array);
		}

		[CompetitionBenchmark, GcAllocations(40, BinarySizeUnit.Byte)]
		public void Allocates40BytesObjectArray()
		{
			var array = new object[(40 - SizeOfReferenceTypeSzArrayHeader) / IntPtr.Size];
			Thread.SpinWait(_count);
			GC.KeepAlive(array);
		}

		[CompetitionBenchmark, GcAllocations(384, BinarySizeUnit.Byte)]
		public void Allocates384BytesEmptyObjects()
		{
			Thread.SpinWait(_count);

			var allocCount = 384 / SizeOfEmptyObject;
			for (int i = 0; i < allocCount; i++)
			{
				GC.KeepAlive(new object());
			}
		}

		[CompetitionBenchmark, GcAllocations(512, BinarySizeUnit.Byte)]
		public void Allocates512BytesString()
		{
			var text = new string('a', (512 - SizeOfStringHeader) / sizeof(char));
			Thread.SpinWait(_count);
			GC.KeepAlive(text);
		}
	}
}