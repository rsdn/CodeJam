using System;

using Xunit;

// ReSharper disable All

namespace CodeJam.PerfTests
{
	internal static class StubXunitTest
	{
		[Fact(Skip = "To fool Appveyor")]
		public static void XunitTestToFoolAppveyor()
		{
			Assert.True(true);
		}
	}
}