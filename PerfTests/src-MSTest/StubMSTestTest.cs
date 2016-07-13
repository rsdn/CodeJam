using System;

using Microsoft.VisualStudio.TestTools.UnitTesting;

// ReSharper disable All

namespace CodeJam.PerfTests
{
	[TestClass]
	internal static class StubMSTestTest
	{
		[TestMethod]
		public static void MSTestTestToFoolAppveyor()
		{
			Assert.IsTrue(true);
		}
	}
}