using System.Linq;

using CodeJam.Strings;

using NUnit.Framework;

namespace CodeJam.Collections
{
	partial class EnumerableExtensionTests
	{
		[TestCase(new[] {3, 1, 8, 0, 6}, ExpectedResult = "0, 6")]
		[TestCase(new[] {1},             ExpectedResult = "1")]
		[TestCase(new int[0],            ExpectedResult = "")]
		public string TakeLastTest(int[] source) => source.TakeLast(2).Join(", ");

		[TestCase(new[] {3, 1, 8, 0, 6}, ExpectedResult = "0, 6")]
		[TestCase(new[] {1},             ExpectedResult = "1")]
		[TestCase(new int[0],            ExpectedResult = "")]
		public string TakeLastEnumerableTest(int[] source) => source.Select(i => i).TakeLast(2).Join(", ");

		[TestCase(new[] {3, 1, 8, 0, 6}, ExpectedResult = true)]
		[TestCase(new[] {1},             ExpectedResult = true)]
		public bool TakeLastIdentityTest(int[] source) => ReferenceEquals(source.TakeLast(source.Length), source);
	}
}
