using System;
using System.Runtime.ConstrainedExecution;
using System.Runtime.InteropServices;
using System.Security;

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
		[ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
		public HGlobalScope() : this(_size) { }

		/// <summary>
		/// Allocates memory from the unmanaged memory of the process by using the specified number of bytes.
		/// </summary>
		/// <param name="cb">The required number of bytes in memory.</param>
		[ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
		public HGlobalScope(int cb) : base(CheckSize(cb))
		{
		}

#pragma warning disable 618 // 'Marshal.SizeOf(Type)', 'Marshal.PtrToStructure(IntPtr, Type)' are obsolete

		/// <summary>
		/// Value
		/// </summary>
		public T Value => (T)Marshal.PtrToStructure(Data, typeof(T))!;

		/// <summary>
		/// Size of the of the generic parameter <typeparamref name="T"/>.
		/// </summary>
		private static readonly int _size = Marshal.SizeOf(typeof(T));

#pragma warning restore 618

		/// <summary>
		/// Validate <paramref name="cb" /> is at least as the size of <typeparamref name="T"/>.
		/// </summary>
		/// <param name="cb">The required number of bytes in memory.</param>
		/// <returns><paramref name="cb" /></returns>
		private static int CheckSize(int cb)
		{
			if (cb < _size)
				throw new ArgumentException($"size is less than {_size}");

			return cb;
		}
	}
}