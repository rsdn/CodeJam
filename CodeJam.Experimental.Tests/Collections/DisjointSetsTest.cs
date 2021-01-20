using System;
using System.Collections.Generic;
using System.Linq;

using NUnit.Framework;

namespace CodeJam.Collections
{
	[TestFixture]
	public class DisjointSetsTest
	{
		private readonly Random _random = new();
		private const int _elementsNumber = 10000;
		private readonly List<int> _seq = Enumerable.Range(0, _elementsNumber).ToList();

		[Test]
		public void Test01NonGeneric()
		{
			for (var i = 1; i <= _elementsNumber; i += 1 + i / (10 + _random.Next(0, 10)))
			{
				//Console.WriteLine($"i = {i}");
				var djs = new DisjointSets(_elementsNumber);
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
			for (var i = 1; i <= _elementsNumber; i += 1 + i / (10 + _random.Next(0, 10)))
			{
				//Console.WriteLine($"i = {i}");
				var rs = RandomShuffle(_seq).ToList();
				var djs = new DisjointSets<int>(rs);
				foreach (var el in rs)
				{
					djs.Union(el, el % i);
				}
				VerifySets(djs, i);
				for (var j = 0; j < _elementsNumber; ++j)
				{
					Assert.That(djs[j], Is.EqualTo(rs[j]));
				}
			}
		}

		private static void VerifySets<T>(DisjointSetsBase<T> djs, int mod) where T : BasicNode
		{
			Assert.That(djs.Count, Is.EqualTo(_elementsNumber));
			Assert.That(djs.SetsCount, Is.EqualTo(mod));
			for (var i = 0; i < _elementsNumber; ++i)
			{
				Assert.That(djs.IndexToSetId(i), Is.EqualTo(djs.IndexToSetId(i % mod)), $"i = {i}, mod = {mod}");
			}
		}

		private IEnumerable<T> RandomShuffle<T>(IEnumerable<T> en) => en.OrderBy(x => _random.Next());
	}
}