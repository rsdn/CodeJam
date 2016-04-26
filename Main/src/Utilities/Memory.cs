using System;
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
		public static bool Compare(byte* p1, byte* p2, int count)
		{
			var bp1 = p1;
			var bp2 = p2;
			var len = count;

			while (len >= 64)
			{
				if (((long*)bp1)[0] != ((long*)bp2)[0]
					|| ((long*)bp1)[1] != ((long*)bp2)[1]
					|| ((long*)bp1)[2] != ((long*)bp2)[2]
					|| ((long*)bp1)[3] != ((long*)bp2)[3]
					|| ((long*)bp1)[4] != ((long*)bp2)[4]
					|| ((long*)bp1)[5] != ((long*)bp2)[5]
					|| ((long*)bp1)[6] != ((long*)bp2)[6]
					|| ((long*)bp1)[7] != ((long*)bp2)[7])
					return false;

				bp1 += 64;
				bp2 += 64;
				len -= 64;
			}

			if (len >= 32)
			{
				if (((long*)bp1)[0] != ((long*)bp2)[0]
					|| ((long*)bp1)[1] != ((long*)bp2)[1]
					|| ((long*)bp1)[2] != ((long*)bp2)[2]
					|| ((long*)bp1)[3] != ((long*)bp2)[3])
					return false;

				bp1 += 32;
				bp2 += 32;
				len -= 32;
			}

			while (len >= 8)
			{
				if (*(long*)bp1 != *(long*)bp2)
					return false;

				bp1 += 8;
				bp2 += 8;
				len -= 8;
			}

			if (len >= 4)
			{
				if (*(int*)bp1 != *(int*)bp2)
					return false;

				bp1 += 4;
				bp2 += 4;
				len -= 4;
			}

			if (len >= 2)
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