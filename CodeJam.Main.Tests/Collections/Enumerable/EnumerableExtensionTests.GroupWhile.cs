using System;
using System.Globalization;
using System.Linq;

using CodeJam.Strings;

using NUnit.Framework;

namespace CodeJam.Collections
{
	[TestFixture]
	public partial class EnumerableExtensionTests
	{
		[TestCase("", "")]
		[TestCase("1", "1")]
		[TestCase("1,2", "1,2")]
		[TestCase("1,2,3,4,5,100,110,112,100,1,5,6", "1,2,3,4,5|100|110,112|100|1,5,6")]
		[TestCase("1,2,1,1,1,10,20,11,15,20,1,1,3", "1,2,1,1,1|10|20|11,15|20|1,1,3")]
		public void GroupWhile(string input, string expected)
		{
			var data = input.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(int.Parse);
			var grouping = data.GroupWhileEquals(i => i / 10);
			var result = grouping.Select(g => g.Join(",")).Join("|");
			Assert.AreEqual(result, expected);
		}

		[TestCase("", "")]
		[TestCase("1", "1")]
		[TestCase("1,2", "1,2")]
		[TestCase("1,2,3,4,5,100,110,112,100,1,5,6", "1,2,3,4,5|100|110,112|100|1,5,6")]
		[TestCase("1,2,1,1,1,10,20,11,15,20,1,1,3", "1,2,1,1,1|10|20|11,15|20|1,1,3")]
		public void GroupWhileToString(string input, string expected)
		{
			var data = input.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(int.Parse);
			var grouping = data.GroupWhileEquals(i => i / 10, i => i.ToString(CultureInfo.InvariantCulture));
			var result = grouping.Select(g => g.Join(",")).Join("|");
			Assert.AreEqual(result, expected);
		}

		[TestCase("1", 2, "1")]
		[TestCase("1,2", 2, "1,2")]
		[TestCase("1,2,3,5,8,9,10,123,1,2,3,4,8,12", 2, "1,2,3,5|8,9,10|123|1,2,3,4|8|12")]
		public void GroupWhileDelta(string input, int delta, string expected)
		{
			var data = input.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(int.Parse);
			var grouping = data.GroupWhile((a, b) => Math.Abs(a - b) <= delta);
			var result = grouping.Select(g => g.Join(",")).Join("|");
			Assert.AreEqual(result, expected);
		}
	}
}
