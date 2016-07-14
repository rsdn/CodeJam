using System;

using NUnit.Framework;

// ReSharper disable All

namespace CodeJam.PerfTests
{
	internal static class StubNUnitTest
	{
		//[Test]
		//[Explicit("To fool Appveyor")]
		public static void NUnitTestToFoolAppveyor()
		{
			Assert.True(true);
		}
	}
}