using System;
using System.Linq;

using CodeJam.Strings;

using NUnit.Framework;

namespace CodeJam.Collections
{
	[TestFixture]
	public partial class EnumerableExtensionTests
	{
		[TestCase(new[] {"1", "2"}, "3", TestName = "Concat1 1", ExpectedResult = "1, 2, 3")]
		[TestCase(new string[0],    "3", TestName = "Concat1 2", ExpectedResult = "3")]
		public string Concat1(string[] input, string concat)
			=> input.Concat(concat).Join(", ");

		[TestCase(new[] {"1", "2"}, new string[0],      TestName = "Concat2 1", ExpectedResult = "1, 2")]
		[TestCase(new string[0],    new[] { "3", "5" }, TestName = "Concat2 2", ExpectedResult = "3, 5")]
		[TestCase(new[] {"1", "2"}, new[] { "3", "0" }, TestName = "Concat2 3", ExpectedResult = "1, 2, 3, 0")]
		public string Concat2(string[] input, string[] concats)
			=> input.Concat(concats).Join(", ");

		[TestCase(new[] {"1", "2"}, "0", TestName = "Prepend1 1", ExpectedResult = "0, 1, 2")]
		[TestCase(new string[0],    "0", TestName = "Prepend1 2", ExpectedResult = "0")]
		public string Prepend1(string[] input, string prepend)
			=> input.Prepend(prepend).Join(", ");

		[TestCase(new[] {"1", "2"}, new string[0],     TestName = "Prepend2 1", ExpectedResult = "1, 2")]
		[TestCase(new[] {"1", "2"}, new[] {"-1", "0"}, TestName = "Prepend2 2", ExpectedResult = "-1, 0, 1, 2")]
		public string Prepend(string[] input, string[] prepend)
			=> input.Prepend(prepend).Join(", ");

		[Test]
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
			// ReSharper disable PossibleMultipleEnumeration
			Assert.IsTrue(enSrc.IsFirst("a"), "#A06");
			Assert.IsFalse(enSrc.IsFirst("b"), "#A07");

			Assert.IsTrue(enSrc.IsFirst("a", null), "#A08");
			Assert.IsFalse(enSrc.IsFirst("A", null), "#A09");
			Assert.IsTrue(enSrc.IsFirst("A", StringComparer.OrdinalIgnoreCase), "#A10");
			// ReSharper restore PossibleMultipleEnumeration
		}

		[Test]
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
			// ReSharper disable PossibleMultipleEnumeration
			Assert.IsTrue(enSrc.IsLast("c"), "#A06");
			Assert.IsFalse(enSrc.IsLast("b"), "#A07");

			Assert.IsTrue(enSrc.IsLast("c", null), "#A08");
			Assert.IsFalse(enSrc.IsLast("C", null), "#A09");
			Assert.IsTrue(enSrc.IsLast("C", StringComparer.OrdinalIgnoreCase), "#A10");
			// ReSharper restore PossibleMultipleEnumeration
		}
	}
}
