using System;
using System.ComponentModel;
#if !LESSTHAN_NET35
using System.Threading.Tasks;
#endif

using NUnit.Framework;

// ReSharper disable once CheckNamespace

namespace CodeJam
{
	[TestFixture]
	public class TargetingTests
	{
		public const string ExpectedTarget =
#if NET20
			".NETFramework,Version=v2.0";
#elif NET35
			".NETFramework,Version=v3.5";
#elif NET40
			".NETFramework,Version=v4.0";
#elif NET45
			".NETFramework,Version=v4.5";
#elif NET461
			".NETFramework,Version=v4.6.1";
#elif NET472
			".NETFramework,Version=v4.7.2";
#elif NETCOREAPP2_0
			".NETCoreApp,Version=v2.0";
#else
			"UNKNOWN";
#endif

		[Test]
		public void TestTargeting()
		{
			TestTools.PrintQuircks();
			Assert.AreEqual(PlatformDependent.TargetPlatform, ExpectedTarget);
		}

#if !LESSTHAN_NET35
		[Test]
		public void TestTasks()
		{
			var t = Task.Factory.StartNew(() => 42);
			var r = t.Result;
			Assert.AreEqual(r, 42);
		}
#endif

		[Test]
		public void TestTuple()
		{
			var a = (a: 1, b: 2, c: 3);
			Assert.AreEqual(a.b, 2);
		}
	}
}