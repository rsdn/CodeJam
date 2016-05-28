using System;

using NUnit.Framework;

// ReSharper disable All

namespace CodeJam.PerfTests
{
	// TODO: refactor into assembly with NUnit runner
	internal static class StubTest
	{
		[Test]
		[Explicit("To fool Appveyor")]
		public static void TestTooFoolAppveyor()
		{
			Assert.That(1, Is.EqualTo(1));
		}
	}
}