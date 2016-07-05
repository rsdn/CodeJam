using System;

using Xunit;

// ReSharper disable All

namespace CodeJam.PerfTests
{
	// TODO: refactor into assembly with NUnit runner
	public static class StubXunitTest
	{
		[Fact]
		public static void XunitTestToFoolAppveyor()
		{
			Assert.True(true);
		}
	}
}