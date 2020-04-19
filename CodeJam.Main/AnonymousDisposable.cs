using System;
using System.Threading;

using JetBrains.Annotations;

namespace CodeJam
{
	/// <summary>
	/// The <see cref="IDisposable"/> implementation that calls supplied action on <see cref="Dispose"/>.
	/// </summary>
	/// DONTTOUCH: DO NOT make it a struct, passing the structure by value will result in multiple Dispose() calls.
	/// SEEALSO: https://blogs.msdn.microsoft.com/ericlippert/2011/03/14/to-box-or-not-to-box-that-is-the-question/
	public class AnonymousDisposable : IDisposable
	{
		/// <summary>
		/// If <c>0</c> - instance not disposed.
		/// </summary>
		/// <remarks>
		/// Use int instead of bool, because <see cref="Interlocked.Exchange(ref double,double)"/> support it.
		/// </remarks>
		protected int Disposed;

		[CanBeNull]
		private readonly Action _disposeAction;

		/// <summary>Initialize instance.</summary>
		/// <param name="disposeAction">The dispose action.</param>
		public AnonymousDisposable([CanBeNull] Action disposeAction) => _disposeAction = disposeAction;

		/// <inheritdoc />
		public void Dispose()
		{
			if (_disposeAction == null)
				return;
			var disposed = Interlocked.Exchange(ref Disposed, 1);
			if (disposed == 0)
			{
				try
				{
					_disposeAction();
				}
				catch when (OnException(disposed))
				{
				}
			}
		}

		/// <summary>
		/// Restore <see cref="Disposed"/> value and returns <c>false</c>.
		/// </summary>
		protected bool OnException(int disposed)
		{
			Interlocked.Exchange(ref Disposed, disposed);
			return false;
		}
	}
}