using System;
using System.Runtime.CompilerServices;
using System.Runtime.ConstrainedExecution;

namespace CodeJam
{
	/// <summary>
	/// The utility class for working with arrays of primitive types.
	/// </summary>
	public static unsafe class Memory
	{
		/// <summary>
		/// Determines whether the first count of bytes of the <paramref name="p1"/> is equal to the <paramref name="p2"/>.
		/// </summary>
		/// <param name="p1">The first buffer to compare.</param>
		/// <param name="p2">The second buffer to compare.</param>
		/// <param name="count">The number of bytes to compare.</param>
		/// <returns>
		/// true if all count bytes of the <paramref name="p1"/> and <paramref name="p2"/> are equal; otherwise, false.
		/// </returns>
		[ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
		public static bool Compare(byte* p1, byte* p2, int count) =>
			CompareInline(p1, p2, count);

		/// <summary>
		/// Determines whether the first count of bytes of the <paramref name="p1"/> is equal to the <paramref name="p2"/>.
		/// </summary>
		/// <remarks>
		/// This is a forced inline version, use with care.
		/// </remarks>
		/// <param name="p1">The first buffer to compare.</param>
		/// <param name="p2">The second buffer to compare.</param>
		/// <param name="count">The number of bytes to compare.</param>
		/// <returns>
		/// true if all count bytes of the <paramref name="p1"/> and <paramref name="p2"/> are equal; otherwise, false.
		/// </returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		[ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
		public static bool CompareInline(byte* p1, byte* p2, int count)
		{
			var bp1 = p1;
			var bp2 = p2;
			var len = count;

			if (count >= 32)
			{
				do
				{
					if (*(long*)bp1 != *(long*)bp2
						|| *(long*)(bp1 + 1*8) != *(long*)(bp2 + 1*8)
						|| *(long*)(bp1 + 2*8) != *(long*)(bp2 + 2*8)
						|| *(long*)(bp1 + 3*8) != *(long*)(bp2 + 3*8))
						return false;

					bp1 += 32;
					bp2 += 32;
					len -= 32;
				}
				while (len >= 32);
			}

			if ((len & 16) != 0)
			{
				if (*(long*)bp1 != *(long*)bp2
					|| *(long*)(bp1 + 8) != *(long*)(bp2 + 8))
					return false;

				bp1 += 16;
				bp2 += 16;
				len -= 16;
			}

			if ((len & 8) != 0)
			{
				if (*(long*)bp1 != *(long*)bp2)
					return false;

				bp1 += 8;
				bp2 += 8;
				len -= 8;
			}

			if ((len & 4) != 0)
			{
				if (*(int*)bp1 != *(int*)bp2)
					return false;

				bp1 += 4;
				bp2 += 4;
				len -= 4;
			}

			if ((len & 2) != 0)
			{
				if (*(short*)bp1 != *(short*)bp2)
					return false;

				bp1 += 2;
				bp2 += 2;
				len -= 2;
			}

			return len == 0 || *bp1 == *bp2;
		}
	}
}