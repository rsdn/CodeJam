using System;
using NUnit.Framework;

namespace CodeJam.Collections
{
	[TestFixture]
	public class SuffixTreeNaiveTest
	{
		[Test]
		public void Test01() => Check("ABRABRABRABRA"
			, "[{A}[{},{BRA}[{},{BRA}[{},{BRA}[{},{BRA}]]]],{BRA}[{},{BRA}[{},{BRA}[{},{BRA}]]],{RA}[{},{BRA}[{},{BRA}[{},{BRA}]]]]");

		[Test]
		public void Test02() => Check("MISSISSIPPI"
			, "[{I}[{},{PPI},{SSI}[{PPI},{SSIPPI}]],{MISSISSIPPI},{P}[{I},{PI}],{S}[{I}[{PPI},{SSIPPI}],{SI}[{PPI},{SSIPPI}]]]");

		[Test]
		public void Test03() => Check("AAAAABAAAAA"
			, "[{A}[{},{A}[{},{A}[{},{A}[{},{A}[{},{BAAAAA}],{BAAAAA}],{BAAAAA}],{BAAAAA}],{BAAAAA}],{BAAAAA}]");

		[Test]
		public void Test04() => Check("ASTALAVISTABABY"
			, "[{A}[{B}[{ABY},{Y}],{LAVISTABABY},{STALAVISTABABY},{VISTABABY}],{B}[{ABY},{Y}],{ISTABABY},{LAVISTABABY},{STA}[{BABY},{LAVISTABABY}],{TA}[{BABY},{LAVISTABABY}],{VISTABABY},{Y}]");

		[Test]
		public void Test05() => Check("ABRACADABRA"
			,"[{A}[{},{BRA}[{},{CADABRA}],{CADABRA},{DABRA}],{BRA}[{},{CADABRA}],{CADABRA},{DABRA},{RA}[{},{CADABRA}]]");

		private static void Check(string data, string expected)
		{
			var st = SuffixTreeNaive.Build(data);
			Console.Write(st.Print());
			Assert.That(SuffixTreeEncoder.Encode(st), Is.EqualTo(expected));

			var builder = SuffixTreeNaive.CreateBuilder();
			builder.Add(data);
			st = builder.Complete();
			Assert.That(SuffixTreeEncoder.Encode(st), Is.EqualTo(expected));
		}

		[Test]
		public void Test06Builder()
		{
			//TODO: test multiple strings
		}
	}
}
