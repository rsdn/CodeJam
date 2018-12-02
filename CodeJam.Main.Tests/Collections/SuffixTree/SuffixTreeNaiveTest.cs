using System;
using NUnit.Framework;

namespace CodeJam.Collections
{
	[TestFixture]
	public class SuffixTreeNaiveTest
	{
		[Test]
		public void Test01() => Check("[{A}[{},{BRA}[{},{BRA}[{},{BRA}[{},{BRA}]]]],{BRA}[{},{BRA}[{},{BRA}[{},{BRA}]]],{RA}[{},{BRA}[{},{BRA}[{},{BRA}]]]]"
			, "ABRABRABRABRA");

		[Test]
		public void Test02() => Check("[{I}[{},{PPI},{SSI}[{PPI},{SSIPPI}]],{MISSISSIPPI},{P}[{I},{PI}],{S}[{I}[{PPI},{SSIPPI}],{SI}[{PPI},{SSIPPI}]]]"
			, "MISSISSIPPI");

		[Test]
		public void Test03() => Check("[{A}[{},{A}[{},{A}[{},{A}[{},{A}[{},{BAAAAA}],{BAAAAA}],{BAAAAA}],{BAAAAA}],{BAAAAA}],{BAAAAA}]"
			, "AAAAABAAAAA");

		[Test]
		public void Test04() => Check("[{A}[{B}[{ABY},{Y}],{LAVISTABABY},{STALAVISTABABY},{VISTABABY}],{B}[{ABY},{Y}],{ISTABABY},{LAVISTABABY},{STA}[{BABY},{LAVISTABABY}],{TA}[{BABY},{LAVISTABABY}],{VISTABABY},{Y}]"
			, "ASTALAVISTABABY");

		[Test]
		public void Test05() => Check("[{A}[{},{BRA}[{},{CADABRA}],{CADABRA},{DABRA}],{BRA}[{},{CADABRA}],{CADABRA},{DABRA},{RA}[{},{CADABRA}]]"
			, "ABRACADABRA");

		[Test]
		public void Test06() => Check("[{AB}[{CBDEABDF},{DF}],{B}[{CBDEABDF},{D}[{EABDF},{F}]],{CBDEABDF},{D}[{EABDF},{F}],{EABDF},{F}]"
			, "ABCBDEABDF");

		[Test]
		public void Test07() => Check("[{a}[{abbabb},{b}[{abbabbaabbabb},{b}[{},{a}[{abbabb},{bb}[{},{aabbabb}]]]]],{b}[{},{a}[{abbabb},{bb}[{},{a}[{abbabb},{bbaabbabb}]]],{b}[{},{a}[{abbabb},{bb}[{},{aabbabb}]]]]]"
			, "ababbabbaabbabb");

		[Test]
		public void Test08() => Check("[{ab}[{c}[{abxabcd},{d}],{xabcd}],{b}[{c}[{abxabcd},{d}],{xabcd}],{c}[{abxabcd},{d}],{d},{xabcd}]"
			, "abcabxabcd");

		[Test]
		public void Test09() => Check("[{c}[{},{d}[{c},{ddcdc}]],{d}[{c}[{},{dc}],{d}[{cdc},{dcdc}]]]"
			, "cdddcdc");

		[Test]
		public void Test10TwoStrings1() => Check("[{A}[{},{},{BRA}[{},{}],{DABRA}],{BRA}[{},{}],{CADABRA},{DABRA},{RA}[{},{}]]"
			, "ABRA", "CADABRA");

		[Test]
		public void Test11TwoStrings2() => Check("[{A}[{},{},{A}[{},{},{A}[{},{},{A}[{},{},{A}[{},{}]]]]],{BAAAAA}]"
			, "AAAAA", "BAAAAA");

		[Test]
		public void Test12ThreeStrings() => Check("[{A}[{LL},{TS}],{FOLKS},{HATS},{KS},{L}[{},{KS},{L}],{OLKS},{S}[{},{}],{T}[{HATS},{S}]]"
			, "THATS", "ALL", "FOLKS");

		[Test]
		public void Test13FourStrings() => Check("[{A}[{},{},{},{},{BRA}],{BRA}[{},{},{},{}],{RA}[{},{},{},{}]]"
			, "ABRA", "BRA", "BRA", "BRA");

		protected virtual void Check(string expected, params string[] data)
		{
			var st = CreateSt();
			foreach (var s in data)
			{
				st.Add(s);
			}
			Assert.That(SuffixTreeEncoder.Encode(st), Is.EqualTo(expected));
		}

		protected virtual SuffixTreeBase CreateSt() => new SuffixTreeNaive();
	}
}
