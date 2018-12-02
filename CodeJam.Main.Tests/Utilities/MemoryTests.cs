using System;
using System.Linq;

using JetBrains.Annotations;

using NUnit.Framework;

namespace CodeJam.Utilities
{
	[TestFixture]
	public class MemoryTests
	{
		[Test]
		public unsafe void CompareIdenticalArraysTest()
		{
			for (var i = 0; i < 1024; i++)
			{
				var a = CreateByteArray(i);
				var b = CreateByteArray(i);

				fixed (byte* pa = a, pb = b)
					Assert.IsTrue(Memory.Compare(pa, pb, a.Length), "Length=" + a.Length);
			}
		}

		[Test]
		public unsafe void CompareNonIdenticalArrays1Test()
		{
			for (var i = 1; i < 1024; i++)
			{
				var a = CreateByteArray(i);
				var b = CreateByteArray(i);

				a[i - 1] = 0;
				b[i - 1] = 1;

				fixed (byte* pa = a, pb = b)
					Assert.IsFalse(Memory.Compare(pa, pb, a.Length), "Length=" + a.Length);
			}
		}

		[Test]
		public unsafe void CompareNonIdenticalArrays2Test()
		{
			for (var i = 1; i < 1024; i++)
			{
				var a = CreateByteArray(i);
				var b = CreateByteArray(i);

				a[i / 2] = 0;
				b[i / 2] = 1;

				fixed (byte* pa = a, pb = b)
					Assert.IsFalse(Memory.Compare(pa, pb, a.Length), "Length=" + a.Length);
			}
		}

		[NotNull]
		private static byte[] CreateByteArray(int length) => Enumerable.Range(0, length).Select(n => unchecked ((byte)n)).ToArray();
	}
}
