using System;
using System.Runtime.ConstrainedExecution;
using System.Runtime.InteropServices;
using System.Security;

using JetBrains.Annotations;

namespace CodeJam
{
	/// <summary>
	/// Wraps Mashal.AllocHGlobal and Marshal.FreeHGlobal using generic.
	/// </summary>
	[PublicAPI]
	[SecurityCritical]
	public class HGlobalScope<T> : HGlobalScope where T : struct
	{
		/// <summary>
		/// Default constructor, allocates memory with the size of <typeparam name="T"/>
		/// </summary>
		[ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
		internal HGlobalScope() : base(Size) { }

		/// <summary>
		/// Allocates memory from the unmanaged memory of the process by using the specified number of bytes.
		/// </summary>
		/// <param name="cb">The required number of bytes in memory.</param>
		[ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
		internal HGlobalScope(int cb) : base(validateArguments(cb))
		{
		}

		private static int validateArguments(int cb)
		{
			if (cb < Size)
				throw new ArgumentException($"size is less than {Size}");

			return cb;
		}
		
		/// <summary>
		/// Value
		/// </summary>
		public T Value => (T)Marshal.PtrToStructure(Data, typeof(T));
		
		/// <summary>
		/// Size of the of the generic parameter <typeparam name="T"/>.
		/// </summary>
		private static readonly int Size = Marshal.SizeOf(typeof(T));
	}
}