using System;

using CodeJam.Strings;

using JetBrains.Annotations;

using NUnit.Framework;

namespace CodeJam.Collections
{
	partial class EnumerableExtensionTests
	{
		[TestCase(new[] {3, 1, 8, 0, 6}, ExpectedResult = "1, 8, 0")]
		[TestCase(new[] {3, 1, 8},       ExpectedResult = "1, 8")]
		[TestCase(new[] {3, 1},          ExpectedResult = "1")]
		[TestCase(new[] {1},             ExpectedResult = "")]
		[TestCase(new int[0],            ExpectedResult = "")]
		public string Slice1Test(int[] source) => source.Slice(1, 3).Join(", ");

		[TestCase(new[] {3, 1, 8, 0, 6}, ExpectedResult = "3, 1, 8")]
		[TestCase(new[] {3, 1, 8},       ExpectedResult = "3, 1, 8")]
		[TestCase(new[] {3, 1},          ExpectedResult = "3, 1")]
		[TestCase(new[] {1},             ExpectedResult = "1")]
		[TestCase(new int[0],            ExpectedResult = "")]
		public string Slice2Test(int[] source) => source.Slice(0, 3).Join(", ");

		[TestCase(new[] {3, 1, 8, 0, 6}, ExpectedResult = "")]
		[TestCase(new[] {3, 1, 8},       ExpectedResult = "")]
		[TestCase(new[] {3, 1},          ExpectedResult = "")]
		[TestCase(new[] {1},             ExpectedResult = "")]
		[TestCase(new int[0],            ExpectedResult = "")]
		public string Slice3Test(int[] source) => source.Slice(1, 0).Join(", ");

		[TestCase(new[] {3, 1, 8, 0, 6}, ExpectedResult = true)]
		[TestCase(new[] {3, 1, 8},       ExpectedResult = true)]
		[TestCase(new[] {3, 1},          ExpectedResult = true)]
		[TestCase(new[] {1},             ExpectedResult = true)]
		public bool SliceIdentityTest(int[] source) =>
			// Slice from 0 to sequence.Count returns the same sequence
			ReferenceEquals(source.Slice(0, source.Length), source);
	}
}
