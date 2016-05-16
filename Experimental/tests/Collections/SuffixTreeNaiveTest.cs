using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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
			var st = SuffixTreeNaiveProxy.Create("ABRABRABRABRA");
			Console.Write(st.Print());
			var expected =
				"[{},{A}[{},{BRA}[{},{BRA}[{},{BRA}[{},{BRA}]]]],{BRA}[{},{BRA}[{},{BRA}[{},{BRA}]]],{RA}[{},{BRA}[{},{BRA}[{},{BRA}]]]]";
			Assert.That(st.EncodeNodes(), Is.EqualTo(expected));
		}

		[Test]
		public void Test02()
		{
			var st = SuffixTreeNaiveProxy.Create("MISSISSIPPI");
			Console.Write(st.Print());
			var expected = "[{},{I}[{},{PPI},{SSI}[{PPI},{SSIPPI}]],{MISSISSIPPI},{P}[{I},{PI}],{S}[{I}[{PPI},{SSIPPI}],{SI}[{PPI},{SSIPPI}]]]";
			Assert.That(st.EncodeNodes(), Is.EqualTo(expected));
		}

		[Test]
		public void Test03()
		{
			var st = SuffixTreeNaiveProxy.Create("AAAAABAAAAA");
			Console.Write(st.Print());
			var expected = "[{},{A}[{},{A}[{},{A}[{},{A}[{},{A}[{},{BAAAAA}],{BAAAAA}],{BAAAAA}],{BAAAAA}],{BAAAAA}],{BAAAAA}]";
			Assert.That(st.EncodeNodes(), Is.EqualTo(expected));
		}

		[Test]
		public void Test04()
		{
			var st = SuffixTreeNaiveProxy.Create("ASTALAVISTABABY");
			Console.Write(st.Print());
			var expected = "[{A}[{B}[{ABY},{Y}],{LAVISTABABY},{STALAVISTABABY},{VISTABABY}],{B}[{ABY},{Y}],{ISTABABY},{LAVISTABABY},{STA}[{BABY},{LAVISTABABY}],{TA}[{BABY},{LAVISTABABY}],{VISTABABY},{Y}]";
			Assert.That(st.EncodeNodes(), Is.EqualTo(expected));
		}

		[Test]
		public void Test05()
		{
			var st = SuffixTreeNaiveProxy.Create("ABRACADABRA");
			Console.Write(st.Print());
			var expected = "[{},{A}[{},{BRA}[{},{CADABRA}],{CADABRA},{DABRA}],{BRA}[{},{CADABRA}],{CADABRA},{DABRA},{RA}[{},{CADABRA}]]";
			Assert.That(st.EncodeNodes(), Is.EqualTo(expected));
		}

		private class SuffixTreeNaiveProxy : SuffixTreeNaive
		{
			private SuffixTreeNaiveProxy(string data, char terminal) : base(data, terminal){}

			public static SuffixTreeNaiveProxy Create(string data)
			{
				var tree = new SuffixTreeNaiveProxy(data, Char.MaxValue);
				tree.Construct();
				return tree;
			}

			public string EncodeNodes()
			{
				var sb = new StringBuilder();
				sb.Append('[');
				var children = Root.Children;
				if (children != null)
				{
					AppendChildren(sb, children);
				}
				sb.Append(']');
				return sb.ToString();
			}

			private void AppendChildren(StringBuilder sb, IEnumerable<int> children)
			{
				var first = true;
				foreach (var v in children.Select(GetNode)
					.Select(_ => new { value = Data.Substring(_.Begin, _.End - _.Begin), children = _.Children })
					.OrderBy(_ => _.value))
				{
					if (!first)
					{
						sb.Append(',');
					}
					sb.Append("{" + v.value + "}");
					if (v.children != null)
					{
						sb.Append('[');
						AppendChildren(sb, v.children);
						sb.Append(']');
					}
					first = false;
				}
			}
		}
	}
}
