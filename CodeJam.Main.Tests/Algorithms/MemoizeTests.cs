using System;

using NUnit.Framework;

namespace CodeJam
{
	[TestFixture]
	public class MemoizeTests
	{
		[TestCase(true)]
		[TestCase(false)]
		public void Memoize(bool threadSafe)
		{
			var calls = 0;
			Func<string, int> func = str => { calls++; return str.Length; };
			var memFunc = func.Memoize(threadSafe);
			Assert.AreEqual(7, memFunc("O La La"), "#A01");
			Assert.AreEqual(1, calls, "#A02");
			Assert.AreEqual(7, memFunc("O La La"), "#A03");
			Assert.AreEqual(1, calls, "#A04");
			Assert.AreEqual(9, memFunc("O La La 2"), "#A05");
			Assert.AreEqual(2, calls, "#A06");
		}

		[TestCase(true)]
		[TestCase(false)]
		public void MemoizeTuple(bool threadSafe)
		{
			var calls = 0;
			Func<string, string, int> func = (s1, s2) => { calls++; return s1.Length + s2.Length; };
			var memFunc = func.Memoize(threadSafe);
			Assert.AreEqual(15, memFunc("O La La", "Ho Ho Ho"), "#A01");
			Assert.AreEqual(1, calls, "#A02");
			Assert.AreEqual(15, memFunc("O La La", "Ho Ho Ho"), "#A03");
			Assert.AreEqual(1, calls, "#A04");
			Assert.AreEqual(19, memFunc("O La La 2", "Ho Ho Ho 2"), "#A05");
			Assert.AreEqual(2, calls, "#A06");
		}
	}
}