using System;
using System.Collections.Generic;
using System.Linq;

using NUnit.Framework;

namespace CodeJam.Collections
{
	[TestFixture]
	public class DisjointSetsTest
	{
		private readonly Random _random = new Random();
		private const int ElementsNumber = 10000;
		private readonly List<int> _seq = Enumerable.Range(0, ElementsNumber).ToList();

		[Test]
		public void Test01NonGeneric()
		{
			for (var i = 1; i <= ElementsNumber; i += 1 + i / (10 + _random.Next(0, 10)))
			{
				Console.WriteLine($"i = {i}");
				var djs = new DisjointSets(ElementsNumber);
				foreach (var el in RandomShuffle(_seq))
				{
					djs.Union(el, el % i);
				}
				VerifySets(djs, i);
			}
		}

		[Test]
		public void Test02Generic()
		{
			for (var i = 1; i <= ElementsNumber; i += 1 + i / (10 + _random.Next(0, 10)))
			{
				Console.WriteLine($"i = {i}");
				var rs = RandomShuffle(_seq).ToList();
				var djs = new DisjointSets<int>(rs);
				foreach (var el in rs)
				{
					djs.Union(el, el % i);
				}
				VerifySets(djs, i);
				for (var j = 0; j < ElementsNumber; ++j)
				{
					Assert.That(djs[j], Is.EqualTo(rs[j]));
				}
			}
		}

		private static void VerifySets<T>(DisjointSetsBase<T> djs, int mod) where T : BasicNode
		{
			Assert.That(djs.Count, Is.EqualTo(ElementsNumber));
			Assert.That(djs.SetsCount, Is.EqualTo(mod));
			for (var i = 0; i < ElementsNumber; ++i)
			{
				Assert.That(djs.IndexToSetId(i), Is.EqualTo(djs.IndexToSetId(i % mod)), $"i = {i}, mod = {mod}");
			}
		}

		private IEnumerable<T> RandomShuffle<T>(IEnumerable<T> en) => en.OrderBy(x => _random.Next());
	}
}