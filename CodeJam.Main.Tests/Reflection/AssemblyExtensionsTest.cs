using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Reflection;

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
					"CodeJam-Tests.dll",
					Path.GetFileName(GetType().Assembly.GetAssemblyPath()),
					StringComparison.InvariantCultureIgnoreCase));

		[Test]
		public void TestGetAssemblyPathLoadedFromByteArray()
		{
			var asmBytes = File.ReadAllBytes(typeof(Code).Assembly.GetAssemblyPath());
			var loadedAssembly = Assembly.Load(asmBytes);

			Assert.AreEqual(loadedAssembly.Location, "");
			Assert.Throws<ArgumentException>(() => loadedAssembly.GetAssemblyPath());
		}
	}
}
