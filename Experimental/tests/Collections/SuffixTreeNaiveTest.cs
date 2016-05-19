using System;
using NUnit.Framework;

namespace CodeJam.Collections
{
	[TestFixture]
	public class SuffixTreeNaiveTest
	{

		[Test]
		public void Test01()
		{
			// see http://felix-halim.net/pg/suffix-tree/ for sample builder
			var st = SuffixTreeNaive.Create("ABRABRABRABRA");
			Console.Write(st.Print());
			var expected =
				"[{A}[{},{BRA}[{},{BRA}[{},{BRA}[{},{BRA}]]]],{BRA}[{},{BRA}[{},{BRA}[{},{BRA}]]],{RA}[{},{BRA}[{},{BRA}[{},{BRA}]]]]";
			Assert.That(SuffixTreeEncoder.Encode(st), Is.EqualTo(expected));
		}

		[Test]
		public void Test02()
		{
			var st = SuffixTreeNaive.Create("MISSISSIPPI");
			Console.Write(st.Print());
			var expected = "[{I}[{},{PPI},{SSI}[{PPI},{SSIPPI}]],{MISSISSIPPI},{P}[{I},{PI}],{S}[{I}[{PPI},{SSIPPI}],{SI}[{PPI},{SSIPPI}]]]";
			Assert.That(SuffixTreeEncoder.Encode(st), Is.EqualTo(expected));
		}

		[Test]
		public void Test03()
		{
			var st = SuffixTreeNaive.Create("AAAAABAAAAA");
			Console.Write(st.Print());
			var expected = "[{A}[{},{A}[{},{A}[{},{A}[{},{A}[{},{BAAAAA}],{BAAAAA}],{BAAAAA}],{BAAAAA}],{BAAAAA}],{BAAAAA}]";
			Assert.That(SuffixTreeEncoder.Encode(st), Is.EqualTo(expected));
		}

		[Test]
		public void Test04()
		{
			var st = SuffixTreeNaive.Create("ASTALAVISTABABY");
			Console.Write(st.Print());
			var expected = "[{A}[{B}[{ABY},{Y}],{LAVISTABABY},{STALAVISTABABY},{VISTABABY}],{B}[{ABY},{Y}],{ISTABABY},{LAVISTABABY},{STA}[{BABY},{LAVISTABABY}],{TA}[{BABY},{LAVISTABABY}],{VISTABABY},{Y}]";
			Assert.That(SuffixTreeEncoder.Encode(st), Is.EqualTo(expected));
		}

		[Test]
		public void Test05()
		{
			var st = SuffixTreeNaive.Create("ABRACADABRA");
			Console.Write(st.Print());
			var expected = "[{A}[{},{BRA}[{},{CADABRA}],{CADABRA},{DABRA}],{BRA}[{},{CADABRA}],{CADABRA},{DABRA},{RA}[{},{CADABRA}]]";
			Assert.That(SuffixTreeEncoder.Encode(st), Is.EqualTo(expected));
		}
	}
}
