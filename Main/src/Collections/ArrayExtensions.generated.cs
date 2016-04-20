using System;

using CodeJam.Utilities;

using JetBrains.Annotations;

namespace CodeJam.Collections
{
	partial class ArrayExtensions
	{
		#region string
		/// <summary>
		/// Returns true, if length and content of <paramref name="a"/> equals <paramref name="b"/>.
		/// </summary>
		/// <param name="a">The first array to compare.</param>
		/// <param name="b">The second array to compare.</param>
		[Pure]
		public static bool EqualsTo([CanBeNull] this string[] a, [CanBeNull] string[] b)
		{
			if (a == b)
				return true;

			if (a == null || b == null)
				return false;

			if (a.Length != b.Length)
				return false;

			for (var i = 0; i < a.Length; i++)
				if (a[i] != b[i])
					return false;

			return true;
		}
		#endregion

		#region byte
		/// <summary>
		/// Returns true, if length and content of <paramref name="a"/> equals <paramref name="b"/>.
		/// </summary>
		/// <param name="a">The first array to compare.</param>
		/// <param name="b">The second array to compare.</param>
		[Pure]
		public static unsafe bool EqualsTo([CanBeNull] this byte[] a, [CanBeNull] byte[] b)
		{
			if (a == b)
				return true;

			if (a == null || b == null)
				return false;

			if (a.Length != b.Length)
				return false;

			if (a.Length < 5)
			{
				for (var i = 0; i < a.Length; i++)
					if (a[i] != b[i])
						return false;

				return true;
			}

			fixed (void* pa = a, pb = b)
				return Memory.Compare((byte*)pa, (byte*)pb, a.Length * sizeof(byte));
		}

		/// <summary>
		/// Returns true, if length and content of <paramref name="a"/> equals <paramref name="b"/>.
		/// </summary>
		/// <param name="a">The first array to compare.</param>
		/// <param name="b">The second array to compare.</param>
		[Pure]
		public static bool EqualsTo([CanBeNull] this byte?[] a, [CanBeNull] byte?[] b)
		{
			if (a == b)
				return true;

			if (a == null || b == null)
				return false;

			if (a.Length != b.Length)
				return false;

			for (var i = 0; i < a.Length; i++)
			{
				var lhs = a[i];
				var rhs = b[i];
				if (lhs.GetValueOrDefault() != rhs.GetValueOrDefault() || lhs.HasValue != rhs.HasValue)
					return false;
			}

			return true;
		}
		#endregion

		#region sbyte
		/// <summary>
		/// Returns true, if length and content of <paramref name="a"/> equals <paramref name="b"/>.
		/// </summary>
		/// <param name="a">The first array to compare.</param>
		/// <param name="b">The second array to compare.</param>
		[Pure]
		public static unsafe bool EqualsTo([CanBeNull] this sbyte[] a, [CanBeNull] sbyte[] b)
		{
			if (a == b)
				return true;

			if (a == null || b == null)
				return false;

			if (a.Length != b.Length)
				return false;

			if (a.Length < 5)
			{
				for (var i = 0; i < a.Length; i++)
					if (a[i] != b[i])
						return false;

				return true;
			}

			fixed (void* pa = a, pb = b)
				return Memory.Compare((byte*)pa, (byte*)pb, a.Length * sizeof(sbyte));
		}

		/// <summary>
		/// Returns true, if length and content of <paramref name="a"/> equals <paramref name="b"/>.
		/// </summary>
		/// <param name="a">The first array to compare.</param>
		/// <param name="b">The second array to compare.</param>
		[Pure]
		public static bool EqualsTo([CanBeNull] this sbyte?[] a, [CanBeNull] sbyte?[] b)
		{
			if (a == b)
				return true;

			if (a == null || b == null)
				return false;

			if (a.Length != b.Length)
				return false;

			for (var i = 0; i < a.Length; i++)
			{
				var lhs = a[i];
				var rhs = b[i];
				if (lhs.GetValueOrDefault() != rhs.GetValueOrDefault() || lhs.HasValue != rhs.HasValue)
					return false;
			}

			return true;
		}
		#endregion

		#region short
		/// <summary>
		/// Returns true, if length and content of <paramref name="a"/> equals <paramref name="b"/>.
		/// </summary>
		/// <param name="a">The first array to compare.</param>
		/// <param name="b">The second array to compare.</param>
		[Pure]
		public static unsafe bool EqualsTo([CanBeNull] this short[] a, [CanBeNull] short[] b)
		{
			if (a == b)
				return true;

			if (a == null || b == null)
				return false;

			if (a.Length != b.Length)
				return false;

			if (a.Length < 5)
			{
				for (var i = 0; i < a.Length; i++)
					if (a[i] != b[i])
						return false;

				return true;
			}

			fixed (void* pa = a, pb = b)
				return Memory.Compare((byte*)pa, (byte*)pb, a.Length * sizeof(short));
		}

		/// <summary>
		/// Returns true, if length and content of <paramref name="a"/> equals <paramref name="b"/>.
		/// </summary>
		/// <param name="a">The first array to compare.</param>
		/// <param name="b">The second array to compare.</param>
		[Pure]
		public static bool EqualsTo([CanBeNull] this short?[] a, [CanBeNull] short?[] b)
		{
			if (a == b)
				return true;

			if (a == null || b == null)
				return false;

			if (a.Length != b.Length)
				return false;

			for (var i = 0; i < a.Length; i++)
			{
				var lhs = a[i];
				var rhs = b[i];
				if (lhs.GetValueOrDefault() != rhs.GetValueOrDefault() || lhs.HasValue != rhs.HasValue)
					return false;
			}

			return true;
		}
		#endregion

		#region ushort
		/// <summary>
		/// Returns true, if length and content of <paramref name="a"/> equals <paramref name="b"/>.
		/// </summary>
		/// <param name="a">The first array to compare.</param>
		/// <param name="b">The second array to compare.</param>
		[Pure]
		public static unsafe bool EqualsTo([CanBeNull] this ushort[] a, [CanBeNull] ushort[] b)
		{
			if (a == b)
				return true;

			if (a == null || b == null)
				return false;

			if (a.Length != b.Length)
				return false;

			if (a.Length < 5)
			{
				for (var i = 0; i < a.Length; i++)
					if (a[i] != b[i])
						return false;

				return true;
			}

			fixed (void* pa = a, pb = b)
				return Memory.Compare((byte*)pa, (byte*)pb, a.Length * sizeof(ushort));
		}

		/// <summary>
		/// Returns true, if length and content of <paramref name="a"/> equals <paramref name="b"/>.
		/// </summary>
		/// <param name="a">The first array to compare.</param>
		/// <param name="b">The second array to compare.</param>
		[Pure]
		public static bool EqualsTo([CanBeNull] this ushort?[] a, [CanBeNull] ushort?[] b)
		{
			if (a == b)
				return true;

			if (a == null || b == null)
				return false;

			if (a.Length != b.Length)
				return false;

			for (var i = 0; i < a.Length; i++)
			{
				var lhs = a[i];
				var rhs = b[i];
				if (lhs.GetValueOrDefault() != rhs.GetValueOrDefault() || lhs.HasValue != rhs.HasValue)
					return false;
			}

			return true;
		}
		#endregion

		#region int
		/// <summary>
		/// Returns true, if length and content of <paramref name="a"/> equals <paramref name="b"/>.
		/// </summary>
		/// <param name="a">The first array to compare.</param>
		/// <param name="b">The second array to compare.</param>
		[Pure]
		public static unsafe bool EqualsTo([CanBeNull] this int[] a, [CanBeNull] int[] b)
		{
			if (a == b)
				return true;

			if (a == null || b == null)
				return false;

			if (a.Length != b.Length)
				return false;

			if (a.Length < 5)
			{
				for (var i = 0; i < a.Length; i++)
					if (a[i] != b[i])
						return false;

				return true;
			}

			fixed (void* pa = a, pb = b)
				return Memory.Compare((byte*)pa, (byte*)pb, a.Length * sizeof(int));
		}

		/// <summary>
		/// Returns true, if length and content of <paramref name="a"/> equals <paramref name="b"/>.
		/// </summary>
		/// <param name="a">The first array to compare.</param>
		/// <param name="b">The second array to compare.</param>
		[Pure]
		public static bool EqualsTo([CanBeNull] this int?[] a, [CanBeNull] int?[] b)
		{
			if (a == b)
				return true;

			if (a == null || b == null)
				return false;

			if (a.Length != b.Length)
				return false;

			for (var i = 0; i < a.Length; i++)
			{
				var lhs = a[i];
				var rhs = b[i];
				if (lhs.GetValueOrDefault() != rhs.GetValueOrDefault() || lhs.HasValue != rhs.HasValue)
					return false;
			}

			return true;
		}
		#endregion

		#region uint
		/// <summary>
		/// Returns true, if length and content of <paramref name="a"/> equals <paramref name="b"/>.
		/// </summary>
		/// <param name="a">The first array to compare.</param>
		/// <param name="b">The second array to compare.</param>
		[Pure]
		public static unsafe bool EqualsTo([CanBeNull] this uint[] a, [CanBeNull] uint[] b)
		{
			if (a == b)
				return true;

			if (a == null || b == null)
				return false;

			if (a.Length != b.Length)
				return false;

			if (a.Length < 5)
			{
				for (var i = 0; i < a.Length; i++)
					if (a[i] != b[i])
						return false;

				return true;
			}

			fixed (void* pa = a, pb = b)
				return Memory.Compare((byte*)pa, (byte*)pb, a.Length * sizeof(uint));
		}

		/// <summary>
		/// Returns true, if length and content of <paramref name="a"/> equals <paramref name="b"/>.
		/// </summary>
		/// <param name="a">The first array to compare.</param>
		/// <param name="b">The second array to compare.</param>
		[Pure]
		public static bool EqualsTo([CanBeNull] this uint?[] a, [CanBeNull] uint?[] b)
		{
			if (a == b)
				return true;

			if (a == null || b == null)
				return false;

			if (a.Length != b.Length)
				return false;

			for (var i = 0; i < a.Length; i++)
			{
				var lhs = a[i];
				var rhs = b[i];
				if (lhs.GetValueOrDefault() != rhs.GetValueOrDefault() || lhs.HasValue != rhs.HasValue)
					return false;
			}

			return true;
		}
		#endregion

		#region long
		/// <summary>
		/// Returns true, if length and content of <paramref name="a"/> equals <paramref name="b"/>.
		/// </summary>
		/// <param name="a">The first array to compare.</param>
		/// <param name="b">The second array to compare.</param>
		[Pure]
		public static unsafe bool EqualsTo([CanBeNull] this long[] a, [CanBeNull] long[] b)
		{
			if (a == b)
				return true;

			if (a == null || b == null)
				return false;

			if (a.Length != b.Length)
				return false;

			if (a.Length < 5)
			{
				for (var i = 0; i < a.Length; i++)
					if (a[i] != b[i])
						return false;

				return true;
			}

			fixed (void* pa = a, pb = b)
				return Memory.Compare((byte*)pa, (byte*)pb, a.Length * sizeof(long));
		}

		/// <summary>
		/// Returns true, if length and content of <paramref name="a"/> equals <paramref name="b"/>.
		/// </summary>
		/// <param name="a">The first array to compare.</param>
		/// <param name="b">The second array to compare.</param>
		[Pure]
		public static bool EqualsTo([CanBeNull] this long?[] a, [CanBeNull] long?[] b)
		{
			if (a == b)
				return true;

			if (a == null || b == null)
				return false;

			if (a.Length != b.Length)
				return false;

			for (var i = 0; i < a.Length; i++)
			{
				var lhs = a[i];
				var rhs = b[i];
				if (lhs.GetValueOrDefault() != rhs.GetValueOrDefault() || lhs.HasValue != rhs.HasValue)
					return false;
			}

			return true;
		}
		#endregion

		#region ulong
		/// <summary>
		/// Returns true, if length and content of <paramref name="a"/> equals <paramref name="b"/>.
		/// </summary>
		/// <param name="a">The first array to compare.</param>
		/// <param name="b">The second array to compare.</param>
		[Pure]
		public static unsafe bool EqualsTo([CanBeNull] this ulong[] a, [CanBeNull] ulong[] b)
		{
			if (a == b)
				return true;

			if (a == null || b == null)
				return false;

			if (a.Length != b.Length)
				return false;

			if (a.Length < 5)
			{
				for (var i = 0; i < a.Length; i++)
					if (a[i] != b[i])
						return false;

				return true;
			}

			fixed (void* pa = a, pb = b)
				return Memory.Compare((byte*)pa, (byte*)pb, a.Length * sizeof(ulong));
		}

		/// <summary>
		/// Returns true, if length and content of <paramref name="a"/> equals <paramref name="b"/>.
		/// </summary>
		/// <param name="a">The first array to compare.</param>
		/// <param name="b">The second array to compare.</param>
		[Pure]
		public static bool EqualsTo([CanBeNull] this ulong?[] a, [CanBeNull] ulong?[] b)
		{
			if (a == b)
				return true;

			if (a == null || b == null)
				return false;

			if (a.Length != b.Length)
				return false;

			for (var i = 0; i < a.Length; i++)
			{
				var lhs = a[i];
				var rhs = b[i];
				if (lhs.GetValueOrDefault() != rhs.GetValueOrDefault() || lhs.HasValue != rhs.HasValue)
					return false;
			}

			return true;
		}
		#endregion

		#region float
		/// <summary>
		/// Returns true, if length and content of <paramref name="a"/> equals <paramref name="b"/>.
		/// </summary>
		/// <param name="a">The first array to compare.</param>
		/// <param name="b">The second array to compare.</param>
		[Pure]
		public static bool EqualsTo([CanBeNull] this float[] a, [CanBeNull] float[] b)
		{
			if (a == b)
				return true;

			if (a == null || b == null)
				return false;

			if (a.Length != b.Length)
				return false;

			for (var i = 0; i < a.Length; i++)
				if (a[i] != b[i])
					return false;

			return true;
		}

		/// <summary>
		/// Returns true, if length and content of <paramref name="a"/> equals <paramref name="b"/>.
		/// </summary>
		/// <param name="a">The first array to compare.</param>
		/// <param name="b">The second array to compare.</param>
		[Pure]
		public static bool EqualsTo([CanBeNull] this float?[] a, [CanBeNull] float?[] b)
		{
			if (a == b)
				return true;

			if (a == null || b == null)
				return false;

			if (a.Length != b.Length)
				return false;

			for (var i = 0; i < a.Length; i++)
			{
				var lhs = a[i];
				var rhs = b[i];
				if (lhs.GetValueOrDefault() != rhs.GetValueOrDefault() || lhs.HasValue != rhs.HasValue)
					return false;
			}

			return true;
		}
		#endregion

		#region double
		/// <summary>
		/// Returns true, if length and content of <paramref name="a"/> equals <paramref name="b"/>.
		/// </summary>
		/// <param name="a">The first array to compare.</param>
		/// <param name="b">The second array to compare.</param>
		[Pure]
		public static bool EqualsTo([CanBeNull] this double[] a, [CanBeNull] double[] b)
		{
			if (a == b)
				return true;

			if (a == null || b == null)
				return false;

			if (a.Length != b.Length)
				return false;

			for (var i = 0; i < a.Length; i++)
				if (a[i] != b[i])
					return false;

			return true;
		}

		/// <summary>
		/// Returns true, if length and content of <paramref name="a"/> equals <paramref name="b"/>.
		/// </summary>
		/// <param name="a">The first array to compare.</param>
		/// <param name="b">The second array to compare.</param>
		[Pure]
		public static bool EqualsTo([CanBeNull] this double?[] a, [CanBeNull] double?[] b)
		{
			if (a == b)
				return true;

			if (a == null || b == null)
				return false;

			if (a.Length != b.Length)
				return false;

			for (var i = 0; i < a.Length; i++)
			{
				var lhs = a[i];
				var rhs = b[i];
				if (lhs.GetValueOrDefault() != rhs.GetValueOrDefault() || lhs.HasValue != rhs.HasValue)
					return false;
			}

			return true;
		}
		#endregion

		#region decimal
		/// <summary>
		/// Returns true, if length and content of <paramref name="a"/> equals <paramref name="b"/>.
		/// </summary>
		/// <param name="a">The first array to compare.</param>
		/// <param name="b">The second array to compare.</param>
		[Pure]
		public static bool EqualsTo([CanBeNull] this decimal[] a, [CanBeNull] decimal[] b)
		{
			if (a == b)
				return true;

			if (a == null || b == null)
				return false;

			if (a.Length != b.Length)
				return false;

			for (var i = 0; i < a.Length; i++)
				if (a[i] != b[i])
					return false;

			return true;
		}

		/// <summary>
		/// Returns true, if length and content of <paramref name="a"/> equals <paramref name="b"/>.
		/// </summary>
		/// <param name="a">The first array to compare.</param>
		/// <param name="b">The second array to compare.</param>
		[Pure]
		public static bool EqualsTo([CanBeNull] this decimal?[] a, [CanBeNull] decimal?[] b)
		{
			if (a == b)
				return true;

			if (a == null || b == null)
				return false;

			if (a.Length != b.Length)
				return false;

			for (var i = 0; i < a.Length; i++)
			{
				var lhs = a[i];
				var rhs = b[i];
				if (lhs.GetValueOrDefault() != rhs.GetValueOrDefault() || lhs.HasValue != rhs.HasValue)
					return false;
			}

			return true;
		}
		#endregion

		#region TimeSpan
		/// <summary>
		/// Returns true, if length and content of <paramref name="a"/> equals <paramref name="b"/>.
		/// </summary>
		/// <param name="a">The first array to compare.</param>
		/// <param name="b">The second array to compare.</param>
		[Pure]
		public static bool EqualsTo([CanBeNull] this TimeSpan[] a, [CanBeNull] TimeSpan[] b)
		{
			if (a == b)
				return true;

			if (a == null || b == null)
				return false;

			if (a.Length != b.Length)
				return false;

			for (var i = 0; i < a.Length; i++)
				if (a[i] != b[i])
					return false;

			return true;
		}

		/// <summary>
		/// Returns true, if length and content of <paramref name="a"/> equals <paramref name="b"/>.
		/// </summary>
		/// <param name="a">The first array to compare.</param>
		/// <param name="b">The second array to compare.</param>
		[Pure]
		public static bool EqualsTo([CanBeNull] this TimeSpan?[] a, [CanBeNull] TimeSpan?[] b)
		{
			if (a == b)
				return true;

			if (a == null || b == null)
				return false;

			if (a.Length != b.Length)
				return false;

			for (var i = 0; i < a.Length; i++)
			{
				var lhs = a[i];
				var rhs = b[i];
				if (lhs.GetValueOrDefault() != rhs.GetValueOrDefault() || lhs.HasValue != rhs.HasValue)
					return false;
			}

			return true;
		}
		#endregion

		#region DateTime
		/// <summary>
		/// Returns true, if length and content of <paramref name="a"/> equals <paramref name="b"/>.
		/// </summary>
		/// <param name="a">The first array to compare.</param>
		/// <param name="b">The second array to compare.</param>
		[Pure]
		public static bool EqualsTo([CanBeNull] this DateTime[] a, [CanBeNull] DateTime[] b)
		{
			if (a == b)
				return true;

			if (a == null || b == null)
				return false;

			if (a.Length != b.Length)
				return false;

			for (var i = 0; i < a.Length; i++)
				if (a[i] != b[i])
					return false;

			return true;
		}

		/// <summary>
		/// Returns true, if length and content of <paramref name="a"/> equals <paramref name="b"/>.
		/// </summary>
		/// <param name="a">The first array to compare.</param>
		/// <param name="b">The second array to compare.</param>
		[Pure]
		public static bool EqualsTo([CanBeNull] this DateTime?[] a, [CanBeNull] DateTime?[] b)
		{
			if (a == b)
				return true;

			if (a == null || b == null)
				return false;

			if (a.Length != b.Length)
				return false;

			for (var i = 0; i < a.Length; i++)
			{
				var lhs = a[i];
				var rhs = b[i];
				if (lhs.GetValueOrDefault() != rhs.GetValueOrDefault() || lhs.HasValue != rhs.HasValue)
					return false;
			}

			return true;
		}
		#endregion

		#region DateTimeOffset
		/// <summary>
		/// Returns true, if length and content of <paramref name="a"/> equals <paramref name="b"/>.
		/// </summary>
		/// <param name="a">The first array to compare.</param>
		/// <param name="b">The second array to compare.</param>
		[Pure]
		public static bool EqualsTo([CanBeNull] this DateTimeOffset[] a, [CanBeNull] DateTimeOffset[] b)
		{
			if (a == b)
				return true;

			if (a == null || b == null)
				return false;

			if (a.Length != b.Length)
				return false;

			for (var i = 0; i < a.Length; i++)
				if (a[i] != b[i])
					return false;

			return true;
		}

		/// <summary>
		/// Returns true, if length and content of <paramref name="a"/> equals <paramref name="b"/>.
		/// </summary>
		/// <param name="a">The first array to compare.</param>
		/// <param name="b">The second array to compare.</param>
		[Pure]
		public static bool EqualsTo([CanBeNull] this DateTimeOffset?[] a, [CanBeNull] DateTimeOffset?[] b)
		{
			if (a == b)
				return true;

			if (a == null || b == null)
				return false;

			if (a.Length != b.Length)
				return false;

			for (var i = 0; i < a.Length; i++)
			{
				var lhs = a[i];
				var rhs = b[i];
				if (lhs.GetValueOrDefault() != rhs.GetValueOrDefault() || lhs.HasValue != rhs.HasValue)
					return false;
			}

			return true;
		}
		#endregion
	}
}
