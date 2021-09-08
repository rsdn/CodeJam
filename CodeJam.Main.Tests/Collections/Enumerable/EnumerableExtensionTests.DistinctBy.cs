using System;
using System.Linq;

using NUnit.Framework;

// ReSharper disable PropertyCanBeMadeInitOnly.Local

namespace CodeJam.Collections
{
	[TestFixture]
	public partial class EnumerableExtensionTests
	{
		[Test]
		public void DistinctByValueType()
		{
			var arr = new[] { 1, 2, 3, 1, 2, 3 };
			CollectionAssert.AreEquivalent(new[] { 1, 2, 3 }, arr.DistinctBy(e => e));
		}

		[Test]
		public void DistinctByNullableValueType()
		{
			var arr = new int?[] { 1, 2, 3, 1, 2, 3, null, null };
			CollectionAssert.AreEquivalent(new int?[] { 1, 2, 3, null }, arr.DistinctBy(e => e ?? 1));
		}

		[Test]
		public void DistinctByReferenceType()
		{
			var arr = new[] { "1", "2", "3", "1", "2", "3" };
			CollectionAssert.AreEquivalent(new[] {"1","2","3" }, arr.DistinctBy(e => e));
		}

		[Test]
		public void DistinctByNullableReferenceType()
		{
			var arr = new[] { "1", "2", "3", "1", "2", "3", null, null };
			CollectionAssert.AreEquivalent(new[] { "1", "2", "3", null }, arr.DistinctBy(e => e ?? ""));
		}

		private class DistinctByRecord
		{
			public int A { get; set; }
		}

		[Test]
		public void DistinctByNullableRecord()
		{
			var arr = new[] { new DistinctByRecord { A = 1 }, new DistinctByRecord { A = 2 }, new DistinctByRecord { A = 3 } };
			CollectionAssert.AreEquivalent(new[] { 1, 2, 3 }, arr.DistinctBy(e => e.A).Select(e => e.A));
		}
	}
}