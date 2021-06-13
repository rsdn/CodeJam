using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using System.Security;
#if LESSTHAN_NET50
using System.Runtime.ConstrainedExecution;
#endif
using JetBrains.Annotations;

namespace CodeJam
{
	/// <summary>
	/// Wraps Marshal.AllocHGlobal and Marshal.FreeHGlobal using generic.
	/// </summary>
	/// <typeparam name="T">Type of the wrapped value behind.</typeparam>
	[PublicAPI]
	[SecurityCritical]
	public class HGlobalScope<T> : HGlobalScope where T : struct
	{
		/// <summary>
		/// Default constructor, allocates memory with the size of <typeparamref name="T"/>
		/// </summary>
#if LESSTHAN_NET50
			[ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
#endif
		public HGlobalScope() : this(_size) { }

		/// <summary>
		/// Allocates memory from the unmanaged memory of the process by using the specified number of bytes.
		/// </summary>
		/// <param name="cb">The required number of bytes in memory.</param>
#if LESSTHAN_NET50
			[ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
#endif
		public HGlobalScope([NonNegativeValue] int cb) : base(CheckSize(cb)) { }

		/// <summary>
		/// Value
		/// </summary>
		[SuppressMessage("ReSharper", "RedundantSuppressNullableWarningExpression")]
		public T Value => (T)Marshal.PtrToStructure(Data, typeof(T))!;

		/// <summary>
		/// Size of the of the generic parameter <typeparamref name="T"/>.
		/// </summary>
		private static readonly int _size = Marshal.SizeOf(typeof(T));

		/// <summary>
		/// Validate <paramref name="cb" /> is at least as the size of <typeparamref name="T"/>.
		/// </summary>
		/// <param name="cb">The required number of bytes in memory.</param>
		/// <returns><paramref name="cb" /></returns>
		private static int CheckSize([NonNegativeValue] int cb)
		{
			if (cb < _size)
				throw new ArgumentException($"size is less than {_size}");

			return cb;
		}
	}
}