using System;
using System.Linq;

using CodeJam.Collections;

using NUnit.Framework;

// ReSharper disable once CheckNamespace
namespace CodeJam
{
	[TestFixture(Category = "Compatibility")]
	public static class OverloadResolutionsTests
	{
		[Test]
		public static void ArrayResolutionTests()
		{
			var a = new[] { 1, 2, 3 };
			a.Reverse();
			Assert.AreEqual(a, new[] { 3, 2, 1 });
		}
	}
}