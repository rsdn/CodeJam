using NUnit.Framework;
using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Reflection;

namespace CodeJam.Reflection
{
	[TestFixture]
	[SuppressMessage("ReSharper", "ReturnValueOfPureMethodIsNotUsed")]
	public class AssemblyExtensionsTest
	{
		[Test]
		public void TestGetAssemblyPath() =>
			Assert.True(

#if !LESSTHAN_NETSTANDARD20 && !LESSTHAN_NETCOREAPP20
				string.Equals(
					"CodeJam.Tests.dll",
					Path.GetFileName(GetType().GetAssembly().GetAssemblyPath()),
					StringComparison.InvariantCultureIgnoreCase)
#else
				string.Equals(
					"CodeJam.Tests.dll".ToUpperInvariant(),
					Path.GetFileName(GetType().GetAssembly().GetAssemblyPath().ToUpperInvariant()))
#endif
				);

#if !LESSTHAN_NETSTANDARD20 && !LESSTHAN_NETCOREAPP20
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
