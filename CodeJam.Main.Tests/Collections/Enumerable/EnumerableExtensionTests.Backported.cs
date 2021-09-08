using System;
// ReSharper disable once RedundantUsingDirective
using System.Collections.Generic;
using System.Linq;

using NUnit.Framework;

namespace CodeJam.Collections
{
	[TestFixture]
	public partial class EnumerableExtensionTests
	{
		[Test]
		public void ToHashSet()
		{
			var lst = new[] { 1, 2, 3 };
			var hashSet = lst.ToHashSet();
			CollectionAssert.AreEquivalent(hashSet, lst);
		}
	}
}

namespace External.CodeJam.Collections
{
	[TestFixture]
	public class EnumerableExtensionTests
	{
		[Test]
		public void ToHashSet()
		{
			var lst = new[] { 1, 2, 3 };
			var hashSet = lst.ToHashSet();
			CollectionAssert.AreEquivalent(hashSet, lst);
		}
	}
}
