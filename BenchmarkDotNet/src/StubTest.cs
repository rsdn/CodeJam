using System;

using NUnit.Framework;

// ReSharper disable All

namespace BenchmarkDotNet
{
	public static class StubTest
	{
		[Test]
		[Explicit("Too fool Appveyor")]
		public static void TestTooFoolAppveyor()
		{
			Assert.That(1, Is.EqualTo(1));
		}
	}
}