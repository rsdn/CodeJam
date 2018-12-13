using System;
using System.Linq;

using CodeJam.Strings;

using JetBrains.Annotations;

using NUnit.Framework;

namespace CodeJam.Collections
{
	partial class EnumerableExtensionTests
	{
		[TestCase(new[] {3, 1, 8, 0, 6}, ExpectedResult = "3, 1, 8", TestName = nameof(SkipLastTest) + "1")]
		[TestCase(new[] {1},             ExpectedResult = "",        TestName = nameof(SkipLastTest) + "2")]
		[TestCase(new int[0],            ExpectedResult = "",        TestName = nameof(SkipLastTest) + "3")]
		public string SkipLastTest([NotNull] int[] source) => source.SkipLast(2).Join(", ");

		[TestCase(new[] {3, 1, 8, 0, 6}, ExpectedResult = "3, 1, 8", TestName = nameof(SkipLastEnumerableTest) + "1")]
		[TestCase(new[] {1},             ExpectedResult = "",        TestName = nameof(SkipLastEnumerableTest) + "2")]
		[TestCase(new int[0],            ExpectedResult = "",        TestName = nameof(SkipLastEnumerableTest) + "3")]
		public string SkipLastEnumerableTest([NotNull] int[] source) => source.Select(i => i).SkipLast(2).Join(", ");
	}
}
