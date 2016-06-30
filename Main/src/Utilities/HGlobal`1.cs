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
	public class HGlobal<T> : CriticalFinalizerObject, IDisposable where T : struct
	{
		private IntPtr _buffer;

		/// <summary>
		/// Default constructor, allocates memory with the size of <typeparamref name="T"/>
		/// </summary>
		[ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
		public HGlobal() : this(_size) { }

		/// <summary>
		/// Allocates memory from the unmanaged memory of the process by using the specified number of bytes.
		/// </summary>
		/// <param name="cb">The required number of bytes in memory.</param>
		[ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
		public HGlobal(int cb)
		{
			if (cb < _size)
				throw new ArgumentException($"size is less than {_size}");

			_buffer = Marshal.AllocHGlobal(cb);
			Length = cb;
		}

		/// <summary>
		/// Finalizer.
		/// </summary>
		~HGlobal()
		{
			DisposeInternal();
		}

		/// <summary>
		/// Dispose method to free all resources.
		/// </summary>
		public void Dispose()
		{
			DisposeInternal();
			GC.SuppressFinalize(this);
		}

		/// <summary>
		/// Length
		///  </summary>
		public int Length { get; }

		/// <summary>
		/// Pointer to data.
		/// </summary>
		public IntPtr Data => _buffer;

		/// <summary>
		/// Value
		/// </summary>
		public T Value => (T)Marshal.PtrToStructure(_buffer, typeof(T));

		/// <summary>
		/// Internal Dispose method.
		/// </summary>
		private void DisposeInternal()
		{
			if (_buffer != IntPtr.Zero)
			{
				Marshal.FreeHGlobal(_buffer);
				_buffer = IntPtr.Zero;
			}
		}

		/// <summary>
		/// Size of the of the generic parameter <typeparamref name="T"/>.
		/// </summary>
		private static readonly int _size = Marshal.SizeOf(typeof(T));
	}
}