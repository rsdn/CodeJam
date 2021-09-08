using System.Linq;

using CodeJam.Strings;

using NUnit.Framework;

namespace CodeJam.Collections
{
	public partial class EnumerableExtensionTests
	{
		[TestCase(new[] {3, 1, 8, 0, 6}, ExpectedResult = "3, 1, 8", TestName = nameof(SkipLastTest) + "1")]
		[TestCase(new[] {1},             ExpectedResult = "",        TestName = nameof(SkipLastTest) + "2")]
		[TestCase(new int[0],            ExpectedResult = "",        TestName = nameof(SkipLastTest) + "3")]
		public string SkipLastTest(int[] source) => source.SkipLast(2).Join(", ");

		[TestCase(new[] {3, 1, 8, 0, 6}, ExpectedResult = "3, 1, 8", TestName = nameof(SkipLastEnumerableTest) + "1")]
		[TestCase(new[] {1},             ExpectedResult = "",        TestName = nameof(SkipLastEnumerableTest) + "2")]
		[TestCase(new int[0],            ExpectedResult = "",        TestName = nameof(SkipLastEnumerableTest) + "3")]
		public string SkipLastEnumerableTest(int[] source)
		{
			var enumerable = source.Select(i => i);
			return enumerable.SkipLast( 2).Join(", ");
		}
	}
}
