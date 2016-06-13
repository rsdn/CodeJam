using System;
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
