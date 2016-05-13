using System.Collections.Generic;
using System.Linq;

using CodeJam.Strings;

using NUnit.Framework;

namespace CodeJam.Collections
{
	[TestFixture]
	public class GroupTopoSortTest
	{
		[TestCase(arg: new[] { "a:b", "b:c", "c" }, ExpectedResult = "c, b, a")]
		[TestCase(arg: new[] { "a:c", "b:c", "c" }, ExpectedResult = "c, a, b")]
		[TestCase(arg: new[] { "a", "b", "c: a, b" }, ExpectedResult = "a, b, c")]
		[TestCase(arg: new[] { "a:c", "b:c", "c", "d:a, b" }, TestName = "Diamond", ExpectedResult = "c, a, b, d")]
		[TestCase(arg: new[] { "a", "b:a", "c" }, ExpectedResult = "a, c, b")]
		// TODO: add more cases
		public string GroupTopoSort(string[] source)
		{
			// Prepare dependency structure
			Dictionary<string, string[]> deps;
			var items = GetDepStructure(source, out deps);

			// Perform sort
			return items.GroupTopoSort(i => deps[i]).Select(l => l.Join(", ")).Join(" : ");
		}

		private static IEnumerable<string> GetDepStructure(IEnumerable<string> source, out Dictionary<string, string[]> deps)
		{
			var items = new HashSet<string>();
			deps = new Dictionary<string, string[]>();
			foreach (var itemStr in source)
			{
				var itemParts = itemStr.Split(':');
				var item = itemParts[0].Trim();
				items.Add(item);
				deps.Add(
					item,
					itemParts.Length > 1
						? itemParts[1].Split(',').Select(s => s.Trim()).ToArray()
						: Array<string>.Empty);
			}
			return items;
		}

		private static IEnumerable<Holder> GetDepStructure(IEnumerable<string> source, out Dictionary<Holder, Holder[]> deps)
		{
			Dictionary<string, string[]> innerDeps;
			var items = GetDepStructure(source, out innerDeps);
			deps = innerDeps.ToDictionary(
				kv => new Holder(kv.Key),
				kv => kv.Value.Select(v => new Holder(v)).ToArray(),
				new KeyEqualityComparer<Holder, string>(v => v.Value));

			return items.Select(v => new Holder(v));
		}

		private class Holder
		{
			public string Value { get; }

			public Holder(string value)
			{
				Value = value;

			}
			public override string ToString() => Value;
		}
	}
}