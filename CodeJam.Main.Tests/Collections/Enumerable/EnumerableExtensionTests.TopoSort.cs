using System;
using System.Collections.Generic;
using System.Linq;

using CodeJam.Strings;

using NUnit.Framework;

namespace CodeJam.Collections
{
	public partial class EnumerableExtensionTests
	{
		[TestCase(arg: new[] { "a:b", "b:c", "c" }, ExpectedResult = "c, b, a")]
		[TestCase(arg: new[] { "a:c", "b:c", "c" }, ExpectedResult = "c, a, b")]
		[TestCase(arg: new[] { "a", "b", "c: a, b" }, ExpectedResult = "a, b, c")]
		[TestCase(arg: new[] { "a:c", "b:c", "c", "d:a, b" }, TestName = "Diamond", ExpectedResult = "c, a, b, d")]
		[TestCase(arg: new[] { "a", "b:a", "c" }, ExpectedResult = "a, c, b")]
		public string TopoSort(string[] source)
		{
			// Prepare dependency structure
			var items = GetDepStructure(source, out Dictionary<string, string[]> deps);

			// Perform sort
			return items.TopoSort(i => deps[i]).Join(", ");
		}

		[TestCase(arg: new[] { "a:b", "b:c", "c" }, ExpectedResult = "c, b, a")]
		[TestCase(arg: new[] { "a:c", "b:c", "c" }, ExpectedResult = "c, a, b")]
		[TestCase(arg: new[] { "a", "b", "c: a, b" }, ExpectedResult = "a, b, c")]
		[TestCase(arg: new[] { "a:c", "b:c", "c", "d:a, b" }, TestName = "Diamond", ExpectedResult = "c, a, b, d")]
		// TODO: add more cases
		public string TopoSortByKey(string[] source)
		{
			// Prepare dependency structure
			var items = GetDepStructure(source, out Dictionary<Holder, Holder[]> deps);

			// Perform sort
			return items.TopoSort(i => deps[i], v => v!.Value).Join(", ");
		}

		[TestCase(arg: new[] { "a:a" })]
		[TestCase(arg: new[] { "a:b", "b:a" })]
		[TestCase(arg: new[] { "a:c", "b:a", "c:b" })]
		public void TopoSortCycle(string[] source)
		{
			// Prepare dependency structure
			var items = GetDepStructure(source, out Dictionary<string, string[]> deps);

			// Perform sort
			// ReSharper disable once ReturnValueOfPureMethodIsNotUsed
			Assert.Throws<ArgumentException>(() => items.TopoSort(i => deps[i]).Join(", "));
		}

		[TestCase(arg: new string[0], ExpectedResult = "")]
		[TestCase(arg: new[] { "a" }, ExpectedResult = "a")]
		[TestCase(arg: new[] { "a", "b" }, ExpectedResult = "a, b")]
		[TestCase(arg: new[] { "a:b", "b:c", "c" }, TestName = "3 in line", ExpectedResult = "c : b : a")]
		[TestCase(arg: new[] { "a:c", "b:c", "c" }, ExpectedResult = "c : a, b")]
		[TestCase(arg: new[] { "a", "b", "c: a, b" }, ExpectedResult = "a, b : c")]
		[TestCase(arg: new[] { "a:c", "b:c", "c", "d:a, b" }, TestName = "Diamond", ExpectedResult = "c : a, b : d")]
		[TestCase(arg: new[] { "a", "b:a", "c" }, ExpectedResult = "a, c : b")]
		[TestCase(arg: new[] { "a", "b:a", "c", "d:c" }, ExpectedResult = "a, c : b, d")]
		[TestCase(arg: new[] { "a", "b:a", "c:b", "d:e", "e" }, ExpectedResult = "a, e : b, d : c")]
		public string GroupTopoSort(string[] source)
		{
			// Prepare dependency structure
			var items = GetDepStructure(source, out Dictionary<string, string[]> deps);

			// Perform sort
			var collSort =
				items
					.GroupTopoSort(i => deps[i])
					.Select(l => l.Join(", "))
					.Join(" : ");
			var enSort =
				items
					.AsEnumerable()
					.GroupTopoSort(i => deps[i])
					.Select(l => l.Join(", "))
					.Join(" : ");
			Assert.AreEqual(collSort, enSort);
			return collSort;
		}

		[TestCase(arg: new[] { "a:a" })]
		[TestCase(arg: new[] { "a:b", "b:a" })]
		[TestCase(arg: new[] { "a:c", "b:a", "c:b" })]
		public void GroupTopoSortCycle(string[] source)
		{
			// Prepare dependency structure
			var items = GetDepStructure(source, out Dictionary<string, string[]> deps);

			// Perform sort
			// ReSharper disable once ReturnValueOfPureMethodIsNotUsed
			Assert.Throws<ArgumentException>(
				() =>
					items
						.GroupTopoSort(i => deps[i])
						.ToArray());
		}

		private static ICollection<string> GetDepStructure(
			IEnumerable<string> source, out Dictionary<string, string[]> deps)
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
			var items = GetDepStructure(source, out Dictionary<string, string[]> innerDeps);
			deps = innerDeps.ToDictionary(
				kv => new Holder(kv.Key),
				kv => kv.Value.Select(v => new Holder(v)).ToArray(),
				new KeyEqualityComparer<Holder, string>(v => v!.Value));

			return items.Select(v => new Holder(v));
		}

		#region Inner type
		private class Holder
		{
			public string Value { get; }

			public Holder(string value)
			{
				Value = value;

			}
			public override string ToString() => Value;
		}
		#endregion
	}
}