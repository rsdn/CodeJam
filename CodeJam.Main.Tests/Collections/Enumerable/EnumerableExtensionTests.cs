using System;
using System.Collections.Generic;
using System.Linq;

using CodeJam.Strings;

using NUnit.Framework;

using SuppressMessageAttribute = System.Diagnostics.CodeAnalysis.SuppressMessageAttribute;
// ReSharper disable UnusedVariable

namespace CodeJam.Collections
{
	[TestFixture]
	public partial class EnumerableExtensionTests
	{
		[TestCase(new[] { "1", "2" }, "3", TestName = "Concat1 1", ExpectedResult = "1, 2, 3")]
		[TestCase(new string[0],      "3", TestName = "Concat1 2", ExpectedResult = "3")]
		public string Concat1(string[] input, string concat) =>
			input.Concat(concat).Join(", ");

		[TestCase(new[] { "1", "2" }, new string[0],      TestName = "Concat2 1", ExpectedResult = "1, 2")]
		[TestCase(new string[0],      new[] { "3", "5" }, TestName = "Concat2 2", ExpectedResult = "3, 5")]
		[TestCase(new[] { "1", "2" }, new[] { "3", "0" }, TestName = "Concat2 3", ExpectedResult = "1, 2, 3, 0")]
		public string Concat2(string[] input, string[] concats) =>
			input.Concat(concats).Join(", ");

		[TestCase(new[] { "1", "2" }, "0", TestName = "Prepend1 1", ExpectedResult = "0, 1, 2")]
		[TestCase(new string[0],      "0", TestName = "Prepend1 2", ExpectedResult = "0")]
		public string Prepend1(string[] input, string prepend) =>
			input.Prepend(prepend).Join(", ");

		[TestCase(new[] { "1", "2" }, new string[0],       TestName = "Prepend2 1", ExpectedResult = "1, 2")]
		[TestCase(new[] { "1", "2" }, new[] { "-1", "0" }, TestName = "Prepend2 2", ExpectedResult = "-1, 0, 1, 2")]
		public string Prepend(string[] input, string[] prepend) =>
			input.Prepend(prepend).Join(", ");

		[Test]
		[SuppressMessage("ReSharper", "PossibleMultipleEnumeration")]
		public void IsFirst()
		{
			var src = new[] { "a", "b", "c" };

			// Fast path
			Assert.IsTrue(src.IsFirst("a"), "#A01");
			Assert.IsFalse(src.IsFirst("b"), "#A02");

			Assert.IsTrue(src.IsFirst("a", null), "#A03");
			Assert.IsFalse(src.IsFirst("A", null), "#A04");
			Assert.IsTrue(src.IsFirst("A", StringComparer.OrdinalIgnoreCase), "#A05");

			// Slow path
			var enSrc = src.Select(i => i);
			Assert.IsTrue(enSrc.IsFirst("a"), "#A06");
			Assert.IsFalse(enSrc.IsFirst("b"), "#A07");

			Assert.IsTrue(enSrc.IsFirst("a", null), "#A08");
			Assert.IsFalse(enSrc.IsFirst("A", null), "#A09");
			Assert.IsTrue(enSrc.IsFirst("A", StringComparer.OrdinalIgnoreCase), "#A10");
		}

		[Test]
		[SuppressMessage("ReSharper", "PossibleMultipleEnumeration")]
		public void IsFirstForEmptyCollections()
		{
			var src = new string[0];

			// Fast path
			Assert.IsFalse(src.IsFirst("a"), "#A01");
			Assert.IsFalse(src.IsFirst("b"), "#A02");
			Assert.IsFalse(src.IsFirst("A", null), "#A03");
			Assert.IsFalse(src.IsFirst("A", StringComparer.OrdinalIgnoreCase), "#A04");

			// Slow path
			var enSrc = src.Select(i => i);
			Assert.IsFalse(enSrc.IsFirst("a"), "#A05");
			Assert.IsFalse(enSrc.IsFirst("b"), "#A06");

			Assert.IsFalse(enSrc.IsFirst("a", null), "#A07");
			Assert.IsFalse(enSrc.IsFirst("A", StringComparer.OrdinalIgnoreCase), "#A08");
		}

		[Test]
		[SuppressMessage("ReSharper", "PossibleMultipleEnumeration")]
		public void IsLast()
		{
			var src = new[] { "a", "b", "c" };

			// Fast path
			Assert.IsTrue(src.IsLast("c"), "#A01");
			Assert.IsFalse(src.IsLast("b"), "#A02");

			Assert.IsTrue(src.IsLast("c", null), "#A03");
			Assert.IsFalse(src.IsLast("C", null), "#A04");
			Assert.IsTrue(src.IsLast("C", StringComparer.OrdinalIgnoreCase), "#A05");

			// Slow path
			var enSrc = src.Select(i => i);
			Assert.IsTrue(enSrc.IsLast("c"), "#A06");
			Assert.IsFalse(enSrc.IsLast("b"), "#A07");

			Assert.IsTrue(enSrc.IsLast("c", null), "#A08");
			Assert.IsFalse(enSrc.IsLast("C", null), "#A09");
			Assert.IsTrue(enSrc.IsLast("C", StringComparer.OrdinalIgnoreCase), "#A10");
		}

		[Test]
		[SuppressMessage("ReSharper", "PossibleMultipleEnumeration")]
		public void IsLastForEmptyCollections()
		{
			var src = new string[0];

			// Fast path
			Assert.IsFalse(src.IsLast("c"), "#A01");
			Assert.IsFalse(src.IsLast("b"), "#A02");
			Assert.IsFalse(src.IsLast("c", null), "#A03");
			Assert.IsFalse(src.IsLast("C", StringComparer.OrdinalIgnoreCase), "#A04");

			// Slow path
			var enSrc = src.Select(i => i);
			Assert.IsFalse(enSrc.IsLast("c"), "#A05");
			Assert.IsFalse(enSrc.IsLast("b"), "#A06");
			Assert.IsFalse(enSrc.IsLast("C", null), "#A07");
			Assert.IsFalse(enSrc.IsLast("C", StringComparer.OrdinalIgnoreCase), "#A08");
		}

		[Test]
		public void DeconstructArray()
		{
			var array = new[] { 1, 2, 3, 4, 5 };
			Assert.IsTrue(array is (1, _, _, _, _));
			Assert.IsFalse(array is (2, _, _, _, _));
			Assert.IsTrue(array is (_, _, 3, _, _));
			Assert.IsFalse(array is (_, _, 5, _, _));
			Assert.IsTrue(array is (1, _, 3, _, _));
			Assert.IsFalse(array is (1, _, 5, _, _));
			Assert.IsTrue(array is (1, 2, 3, 4, 5));
			Assert.IsFalse(array is (1, 2, 3, 6, 5));

			Assert.Throws<ArgumentException>(
				() =>
				{
					var b = array is (1, _, _, _, _, _);
				});
		}

		[Test]
		public void DeconstructList()
		{
			IList<int> array = new[] { 1, 2, 3, 4, 5 };
			Assert.IsTrue(array is (1, _, _, _, _));
			Assert.IsFalse(array is (2, _, _, _, _));
			Assert.IsTrue(array is (_, _, 3, _, _));
			Assert.IsFalse(array is (_, _, 5, _, _));
			Assert.IsTrue(array is (1, _, 3, _, _));
			Assert.IsFalse(array is (1, _, 5, _, _));
			Assert.IsTrue(array is (1, 2, 3, 4, 5));
			Assert.IsFalse(array is (1, 2, 3, 6, 5));

			Assert.Throws<ArgumentException>(
				() =>
				{
					var b = array is (1, _, _, _, _, _);
				});
		}

		[Test]
		public void DeconstructEnumerable()
		{
#pragma warning disable CA1508 // Avoid dead conditional code
			IEnumerable<int> array = new[] { 1, 2, 3, 4, 5 };
			Assert.IsTrue(array is (1, _, _, _, _));
			Assert.IsFalse(array is (2, _, _, _, _));
			Assert.IsTrue(array is (_, _, 3, _, _));
			Assert.IsFalse(array is (_, _, 5, _, _));
			Assert.IsTrue(array is (1, _, 3, _, _));
			Assert.IsFalse(array is (1, _, 5, _, _));
			Assert.IsTrue(array is (1, 2, 3, 4, 5));
			Assert.IsFalse(array is (1, 2, 3, 6, 5));

			Assert.Throws<ArgumentException>(
				() =>
				{
					var b = array is (1, _, _, _, _, _);
				});
#pragma warning restore CA1508 // Avoid dead conditional code
		}

		[Test]
		public void Flatten()
		{
			var tree = new[]
			{
				new FlattenNode(
					"root1",
					new FlattenNode(
						"root1/1",
						new FlattenNode("root1/1/1"),
						new FlattenNode("root1/1/2")),
					new FlattenNode("root1/2")),
				new FlattenNode(
					"root2",
					new FlattenNode(
						"root2/1",
						new FlattenNode("root2/1/1"),
						new FlattenNode("root2/1/2")),
					new FlattenNode("root2/2"))
			};

			var expected = new[]
			{
				"root1",
				"root1/1",
				"root1/1/1",
				"root1/1/2",
				"root1/2",
				"root2",
				"root2/1",
				"root2/1/1",
				"root2/1/2",
				"root2/2"
			};

			var nodeNames = tree
				.Flatten(i => i.ChildNodes)
				.Select(i => i.Name)
				.ToArray();

			Assert.AreEqual(nodeNames, expected);
		}

		private sealed class FlattenNode
		{
			public FlattenNode(string name, params FlattenNode[] childNodes)
			{
				Name = name;
				ChildNodes = childNodes;
			}

			public string Name { get; }
			public FlattenNode[] ChildNodes { get; }
		}
	}
}
