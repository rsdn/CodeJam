
using NUnit.Framework;

// ReSharper disable once CheckNamespace

namespace CodeJam
{
	[TestFixture]
	public class FnTests
	{
		[Test]
		public void TestFn()
		{
			Assert.IsFalse(Fn.False());
			Assert.IsTrue(Fn.True());
		}

		[Test]
		public void TestAction()
		{
			var i = 0;
			var a = Fn.Action(() => i++);

			Assert.AreEqual(i, 0);
			a();
			Assert.AreEqual(i, 1);
			a();
			Assert.AreEqual(i, 2);
		}

		[Test]
		public void TestFunc()
		{
			var i = 0;
			var f = Fn.Func(() => i++);

			Assert.AreEqual(i, 0);
			f();
			Assert.AreEqual(i, 1);
			f();
			Assert.AreEqual(i, 2);
		}
	}
}