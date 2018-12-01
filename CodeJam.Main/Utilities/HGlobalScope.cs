using System;
using System.Runtime.ConstrainedExecution;
using System.Runtime.InteropServices;
using System.Security;

using JetBrains.Annotations;

namespace CodeJam
{
	/// <summary>
	/// Wraps <see cref="Marshal.AllocHGlobal(System.IntPtr)"/> and <see cref="Marshal.FreeHGlobal"/>.
	/// </summary>
	[PublicAPI]
	[SecurityCritical]
	public class HGlobalScope : CriticalFinalizerObject, IDisposable
	{
		/// <summary>
		/// Internal pointer.
		/// </summary>
		private IntPtr _ptr;

		/// <summary>
		/// Allocates memory from the unmanaged memory of the process by using the specified number of bytes.
		/// </summary>
		/// <param name="cb">The required number of bytes in memory.</param>
		[ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
		internal HGlobalScope(int cb)
		{
			_ptr = Marshal.AllocHGlobal(cb);

			Length = cb;
		}

		/// <summary>
		/// Finalizer.
		/// </summary>
		~HGlobalScope()
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
		/// </summary>
		public int Length { get; }

		/// <summary>
		/// Pointer to data.
		/// </summary>
		public IntPtr Data => _ptr;

		/// <summary>
		/// Internal Dispose method.
		/// </summary>
		private void DisposeInternal()
		{
			if (_ptr != IntPtr.Zero)
			{
				Marshal.FreeHGlobal(_ptr);
				_ptr = IntPtr.Zero;
			}
		}
	}
}
