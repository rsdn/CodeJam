using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Reflection;

using CodeJam.Targeting;

using NUnit.Framework;

namespace CodeJam.Reflection
{
	[TestFixture]
	[SuppressMessage("ReSharper", "ReturnValueOfPureMethodIsNotUsed")]
	public class AssemblyExtensionsTest
	{
		[Test]
		public void TestGetAssemblyPath() =>
			Assert.True(
				string.Equals(
					"CodeJam.Tests.dll",
					Path.GetFileName(GetType().GetAssembly().GetAssemblyPath()),
					StringComparison.OrdinalIgnoreCase));

#if LESSTHAN_NETCOREAPP20
		// Assembly.Load(asmBytes) is not supported when targeting to these frameworks
#else
		[Test]
		public void TestGetAssemblyPathLoadedFromByteArray()
		{
			var asmBytes = File.ReadAllBytes(typeof(Code).GetAssembly().GetAssemblyPath());
			var loadedAssembly = Assembly.Load(asmBytes);

			Assert.AreEqual(loadedAssembly.Location, "");
			Assert.Throws<ArgumentException>(() => loadedAssembly.GetAssemblyPath());
		}
#endif
	}
}
