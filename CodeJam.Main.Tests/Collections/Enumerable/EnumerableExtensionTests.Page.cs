using System;
using System.Linq;

using CodeJam.Strings;

using NUnit.Framework;

namespace CodeJam.Collections
{
	partial class EnumerableExtensionTests
	{
		[TestCase(0, ExpectedResult = "")]
		[TestCase(1, ExpectedResult = "")]
		[TestCase(2, ExpectedResult = "")]
		[TestCase(3, ExpectedResult = "2")]
		[TestCase(4, ExpectedResult = "2, 3")]
		[TestCase(5, ExpectedResult = "2, 3")]
		public string Page1Test(int n)
		{
			var source = n > 0
				? Enumerable.Range(0, n)
				: Enumerable.Empty<int>();
			return source.Page(2, 2).Join(", ");
		}

		[TestCase(0, ExpectedResult = "")]
		[TestCase(1, ExpectedResult = "0")]
		[TestCase(2, ExpectedResult = "0, 1")]
		[TestCase(3, ExpectedResult = "0, 1")]
		[TestCase(4, ExpectedResult = "0, 1")]
		[TestCase(5, ExpectedResult = "0, 1")]
		public string Page2Test(int n)
		{
			var source = n > 0
				? Enumerable.Range(0, n)
				: Enumerable.Empty<int>();
			return source.Page(1, 2).Join(", ");
		}

		[Test]
		public void Page3Test()
		{
			for (var n = 1; n < 100; n++)
			{
				var actual = Enumerable.Range(0, n).Page(1, n);
				var expected = Enumerable.Range(0, n);
				Assert.That(actual, Is.EquivalentTo(expected));
			}
		}

		[TestCase(new[] {3, 1, 8, 0, 6}, ExpectedResult = true)]
		[TestCase(new[] {3, 1, 8},       ExpectedResult = true)]
		[TestCase(new[] {3, 1},          ExpectedResult = true)]
		[TestCase(new[] {1},             ExpectedResult = true)]
		public bool PageIdentityTest(int[] source) =>
			// Page from 0 to sequence.Count returns the same sequence
			ReferenceEquals(source.Page(1, source.Length), source);
	}
}
