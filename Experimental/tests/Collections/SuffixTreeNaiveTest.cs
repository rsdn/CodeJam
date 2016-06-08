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

		private static void Check(string expected, params string[] data)
		{
			var st = new SuffixTreeNaive();
			foreach (var s in data)
			{
				st.Add(s);
			}
			Console.Write(st.Print());
			Assert.That(SuffixTreeEncoder.Encode(st), Is.EqualTo(expected));
		}

		[Test]
		public void Test06TwoStrings1() => Check("[{A}[{},{},{BRA}[{},{}],{DABRA}],{BRA}[{},{}],{CADABRA},{DABRA},{RA}[{},{}]]"
			, "ABRA", "CADABRA");

		[Test]
		public void Test07TwoStrings2() => Check("[{A}[{},{},{A}[{},{},{A}[{},{},{A}[{},{},{A}[{},{}]]]]],{BAAAAA}]"
			, "AAAAA", "BAAAAA");

		[Test]
		public void Test08ThreeStrings() => Check("[{A}[{LL},{TS}],{FOLKS},{HATS},{KS},{L}[{},{KS},{L}],{OLKS},{S}[{},{}],{T}[{HATS},{S}]]"
			, "THATS", "ALL", "FOLKS");

		[Test]		
		public void Test09FourStrings() => Check("[{A}[{},{},{},{},{BRA}],{BRA}[{},{},{},{}],{RA}[{},{},{},{}]]"
			, "ABRA", "BRA", "BRA", "BRA");
	}
}
