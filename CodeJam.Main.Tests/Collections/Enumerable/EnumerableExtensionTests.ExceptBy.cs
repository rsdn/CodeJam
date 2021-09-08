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
		public void ExceptByValueType()
		{
			var arr = new[] { 1, 2, 3, 1, 2, 3 };
			CollectionAssert.AreEquivalent(new[] { 3 }, arr.ExceptBy(new[] { 1, 2 }, e => e));
		}

		[Test]
		public void ExceptByNullableValueType()
		{
			var arr = new int?[] { 1, 2, 3, 1, 2, 3, null, null };
			CollectionAssert.AreEquivalent(new int?[] { 3, null }, arr.ExceptBy(new int?[] { 1, 2 }, e => e ?? 1));
		}

		[Test]
		public void ExceptByReferenceType()
		{
			var arr = new[] { "1", "2", "3", "1", "2", "3" };
			CollectionAssert.AreEquivalent(new[] { "3" }, arr.ExceptBy(new[] { "1", "2" }, e => e));
		}

		[Test]
		public void ExceptByNullableReferenceType()
		{
			var arr = new[] { "1", "2", "3", "1", "2", "3", null, null };
			CollectionAssert.AreEquivalent(new[] { "3", null }, arr.ExceptBy(new string?[] { "1", "2" }, e => e ?? ""));
		}

		private class ExceptByRecord
		{
			public int A { get; set; }
		}

		[Test]
		public void ExceptByNullableRecord()
		{
			var arr = new[] { new ExceptByRecord { A = 1 }, new ExceptByRecord { A = 2 }, new ExceptByRecord { A = 3 } };
			CollectionAssert.AreEquivalent(
				new[] { 3 },
				arr
					.ExceptBy(new[] { new ExceptByRecord { A = 1 }, new ExceptByRecord { A = 2 } }, e => e.A)
					.Select(e => e.A));
		}
	}
}