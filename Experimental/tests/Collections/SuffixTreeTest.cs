using System;
using System.Collections.Generic;
using System.Linq;

using NUnit.Framework;

namespace CodeJam.Collections
{
	[TestFixture]
	public class SuffixTreeTest : SuffixTreeNaiveTest
	{
		private static readonly char[] _alphabet = Enumerable.Range('A', 26).Select(_ => (char)_).ToArray();
		private static readonly Random _rnd = new Random();

		private static string MakeRandomString(int length)
			=> new string(Enumerable.Range(0, length).Select(_ => _alphabet[_rnd.Next(0, _alphabet.Length)]).ToArray());

		[Test]
		public void Test14RandomSingle()
		{
			const int length = 500;
			for (var i = 0; i < 1000; ++i)
			{
				var nst = new SuffixTreeNaive();
				var s = MakeRandomString(length);
				nst.Add(s);
				var expected = SuffixTreeEncoder.Encode(nst);
				var st = new SuffixTree();
				st.Add(s);
				var result = SuffixTreeEncoder.Encode(st);
				Assert.That(result, Is.EqualTo(expected), "String={0}\r\nExpected={1}\r\nReal={2}", s, expected, result);
			}
		}

		[Test]
		public void Test15RandomMultiple()
		{
			const int length = 100;
			for (var i = 0; i < 1000; ++i)
			{
				var strings = Enumerable.Range(0, _rnd.Next(2, 10))
					.Select(_ => MakeRandomString(length)).ToArray();
				var nst = new SuffixTreeNaive();
				var st = new SuffixTree();
				var stCompact = new SuffixTree();
				foreach (var s in strings)
				{
					nst.Add(s);
					st.Add(s);
					stCompact.Add(s);
					stCompact.Compact();
				}
				var expected = SuffixTreeEncoder.Encode(nst);
				var result = SuffixTreeEncoder.Encode(st);
				Assert.That(result, Is.EqualTo(expected));
				result = SuffixTreeEncoder.Encode(stCompact);
				Assert.That(result, Is.EqualTo(expected));
			}
		}

		[Test]
		public void Test16AllSuffixes()
		{
			const int length = 50;
			// pure random
			for (var numberOfString = 1; numberOfString < 6; ++numberOfString)
			{
				var strings = Enumerable.Range(0, numberOfString)
					.Select(_ => MakeRandomString(length)).ToArray();
				TestAllSuffixes(strings);
			}
			// with guaranteed duplicates
			var s = MakeRandomString(length);
			for (var numberOfString = 2; numberOfString < 6; ++numberOfString)
			{
				var strings = Enumerable.Range(0, numberOfString - 2)
					.Select(_ => MakeRandomString(length)).Union(s, s).ToArray();
				TestAllSuffixes(strings);
			}
		}

		[Test]
		public void Test17Contains()
		{
			const int length = 50;
			for (var numberOfString = 1; numberOfString < 6; ++numberOfString)
			{
				var strings = Enumerable.Range(0, numberOfString)
					.Select(_ => MakeRandomString(length)).ToArray();
				var st = new SuffixTree();
				var suffixes = new HashSet<string>();
				var properSubstrings = new HashSet<string>();
				foreach (var s in strings)
				{
					st.Add(s);
					for (var i = 0; i < s.Length; ++i)
					{
						var suffix = s.Substring(i);
						suffixes.Add(suffix);
						if (suffix.Length != 1)
						{
							properSubstrings.Add(suffix.Substring(0, suffix.Length - 1));
						}
					}
				}
				properSubstrings.ExceptWith(suffixes);
				st.Compact();

				const string notPresent = "@";
				Assert.That(st.Contains(string.Empty));
				Assert.That(st.ContainsSuffix(string.Empty));
				Assert.That(!st.Contains(notPresent));
				Assert.That(!st.ContainsSuffix(notPresent));
				foreach (var suffix in suffixes)
				{
					Assert.That(st.Contains(suffix));
					Assert.That(st.ContainsSuffix(suffix));
				}
				foreach (var properSubstring in properSubstrings)
				{
					Assert.That(st.Contains(properSubstring));
					Assert.That(!st.ContainsSuffix(properSubstring));
					for (var i = 0; i <= properSubstring.Length; ++i)
					{
						var notSubstring = properSubstring.Insert(i, notPresent);
						Assert.That(!st.Contains(notSubstring));
						Assert.That(!st.ContainsSuffix(notSubstring));
					}
				}
			}
		}

		[Test]
		public void Test18StartingWith()
		{
			const int length = 50;
			for (var numberOfString = 1; numberOfString < 6; ++numberOfString)
			{
				var strings = Enumerable.Range(0, numberOfString)
					.Select(_ => MakeRandomString(length)).ToArray();
				TestStartingWith(strings);
			}
		}

		private static void TestStartingWith(string[] strings)
		{
			var prefixes = new HashSet<string>();
			var st = new SuffixTree();
			foreach (var s in strings)
			{
				st.Add(s);
				for (var i = 0; i < s.Length; ++i)
				{
					for (var j = i + 1; j <= s.Length; ++j)
					{
						prefixes.Add(s.Substring(i, j - i));
					}
				}
			}
			st.Compact();
			foreach (var prefix in prefixes)
			{
				var expectedSuffixes = new List<string>();
				var expectedSources = new LazyDictionary<string, List<int>>(_ => new List<int>());
				for (var i = 0; i < strings.Length; ++i)
				{
					var s = strings[i];
					var pos = 0;
					for (;;)
					{
						pos = s.IndexOf(prefix, pos);
						if (pos == -1)
						{
							break;
						}
						var suffix = s.Substring(pos);
						expectedSuffixes.Add(suffix);
						expectedSources[suffix].Add(i);
						++pos;
					}
				}
				expectedSuffixes.Sort();
				var suffixes = st.StartingWith(prefix).ToList();
				Assert.That(suffixes.Select(_ => _.Value).ToList(), Is.EqualTo(expectedSuffixes));
				var grouped = suffixes.Select(_ => new { value = _.Value, source = _.SourceIndex })
					.GroupBy(_ => _.value).ToDictionary(_ => _.Key, _ => _.Select(v => v.source).OrderBy(v => v).ToList());
				foreach (var v in grouped)
				{
					Assert.That(v.Value, Is.EqualTo(expectedSources[v.Key]));
				}
			}
			Assert.That(st.StartingWith("@").Count(), Is.EqualTo(0));
			VerifyAllSuffixes(strings, st.StartingWith(string.Empty));
		}

		private static void TestAllSuffixes(string[] strings)
		{
			var st = new SuffixTree();
			foreach (var s in strings)
			{
				st.Add(s);
			}
			st.Compact();
			VerifyAllSuffixes(strings, st.All());
		}

		private static void VerifyAllSuffixes(string[] strings, IEnumerable<Suffix> result)
		{
			var expectedSuffixes = new List<string>();
			var expectedSources = new LazyDictionary<string, List<int>>(_ => new List<int>());
			for (var i = 0; i < strings.Length; ++i)
			{
				var s = strings[i];
				for (var j = 0; j < s.Length; ++j)
				{
					var suffix = s.Substring(j);
					expectedSuffixes.Add(suffix);
					expectedSources[suffix].Add(i);
				}
			}
			expectedSuffixes.Sort();
			var suffixes = result.ToList();
			Assert.That(suffixes.Select(_ => _.Value).ToList(), Is.EqualTo(expectedSuffixes));
			var grouped = suffixes.Select(_ => new { value = _.Value, source = _.SourceIndex })
				.GroupBy(_ => _.value).ToDictionary(_ => _.Key, _ => _.Select(v => v.source).OrderBy(v => v).ToList());
			foreach (var v in grouped)
			{
				Assert.That(v.Value, Is.EqualTo(expectedSources[v.Key]));
			}
		}

		protected override void Check(string expected, params string[] data)
		{
			base.Check(expected, data);

			if (data.Length < 2)
			{
				return;
			}
			// test that Compact() between Adds does not destroy processing
			var st = new SuffixTree();
			foreach (var s in data)
			{
				st.Add(s);
				st.Compact();
			}
			Assert.That(SuffixTreeEncoder.Encode(st), Is.EqualTo(expected));
		}

		protected override SuffixTreeBase CreateSt() => new SuffixTree();
	}
}
