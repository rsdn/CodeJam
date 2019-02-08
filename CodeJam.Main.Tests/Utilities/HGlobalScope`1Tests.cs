using System;
using System.Runtime.InteropServices;

using NUnit.Framework;

namespace CodeJam.Utilities
{
	[TestFixture]
	public class HGlobalScope1Tests
	{
		[Test]
		public void DefaultConstructor()
		{
			using (var h = HGlobal.Create<int>())
			{
				Assert.AreEqual(sizeof(int), h.Length);
				Assert.NotNull(h.Data);

				Marshal.WriteInt32(h.Data, 100);
				Assert.AreEqual(100, h.Value);
			}
		}

		[Test]
		public void IntConstructor([Values(sizeof(int), sizeof(int) + 1)] int size)
		{
			using (var h = HGlobal.Create<int>(size))
			{
				Assert.AreEqual(size, h.Length);
				Assert.NotNull(h.Data);

				Marshal.WriteInt32(h.Data, 100);
				Assert.AreEqual(100, h.Value);
			}
		}

		[Test]
		public void IntConstructorSmallerSize()
		{
			Assert.Throws<ArgumentException>(() => HGlobal.Create<int>(sizeof(int) - 1));
		}
	}
}