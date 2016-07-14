using System;

using NUnit.Framework;

namespace CodeJam.PerfTests.Running.Core.NUnit
{
	internal static class StubNUnitTest
	{
		// WAITINGFOR: https://github.com/nunit/nunit/issues/668
		[Test]
		[Explicit("Workaround for NUnit console test runner failing on no tests assemblies.")]
		public static void StubTest() => Assert.True(true);
	}
}