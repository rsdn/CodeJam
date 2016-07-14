using System;

using NUnit.Framework;

// ReSharper disable All

namespace CodeJam.PerfTests
{
	internal static class StubNUnitTest
	{
		// WAITINGFOR: https://github.com/nunit/nunit/issues/668
		[Test]
		[Explicit("Workaroiund for NUnit console test runner failing on no tests assemblies.")]
		public static void StubTest()
		{
			Assert.True(true);
		}
	}
}