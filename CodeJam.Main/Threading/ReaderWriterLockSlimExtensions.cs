﻿using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading;

using JetBrains.Annotations;

namespace CodeJam.Threading
{
	/// <summary>
	/// Represents the extension methods for <see cref="ReaderWriterLockSlim"/>.
	/// </summary>
	[PublicAPI]
	public static class ReaderWriterLockSlimExtensions
	{
		/// <summary>
		/// Tries to enter the lock in read mode.
		/// </summary>
		/// <param name="readerWriterLock">The <see cref="ReaderWriterLockSlim"/> instance.</param>
		/// <returns>
		/// The <see cref="IDisposable"/> object that reduce the recursion count for read mode, and exits read mode if the
		/// resulting count is 0 (zero).
		/// </returns>
		[Pure, System.Diagnostics.Contracts.Pure]
		public static ReadLockScope GetReadLock(this ReaderWriterLockSlim readerWriterLock)
		{
			Code.NotNull(readerWriterLock, nameof(readerWriterLock));

			readerWriterLock.EnterReadLock();
			return new ReadLockScope(readerWriterLock);
		}

		/// <summary>
		/// Tries to enter the lock in write mode.
		/// </summary>
		/// <param name="readerWriterLock">The <see cref="ReaderWriterLockSlim"/> instance.</param>
		/// <returns>
		/// The <see cref="IDisposable"/> object that reduce the recursion count for write mode, and exits write mode if the
		/// resulting count is 0 (zero).
		/// </returns>
		[Pure, System.Diagnostics.Contracts.Pure]
		public static WriteLockScope GetWriteLock(this ReaderWriterLockSlim readerWriterLock)
		{
			Code.NotNull(readerWriterLock, nameof(readerWriterLock));

			readerWriterLock.EnterWriteLock();
			return new WriteLockScope(readerWriterLock);
		}

		/// <summary>
		/// Tries to enter the lock in upgradeable mode.
		/// </summary>
		/// <param name="readerWriterLock">The <see cref="ReaderWriterLockSlim"/> instance.</param>
		/// <returns>
		/// The <see cref="IDisposable"/> object that reduce the recursion count for upgradeable mode, and exits upgradeable
		/// mode if the resulting count is 0 (zero).
		/// </returns>
		[Pure, System.Diagnostics.Contracts.Pure]
		public static UpgradeableReadLockScope GetUpgradeableReadLock(this ReaderWriterLockSlim readerWriterLock)
		{
			Code.NotNull(readerWriterLock, nameof(readerWriterLock));

			readerWriterLock.EnterUpgradeableReadLock();
			return new UpgradeableReadLockScope(readerWriterLock);
		}

		#region Inner type: ReadLockScope
		/// <summary>
		/// The <see cref="ReaderWriterLockSlim"/> wrapper.
		/// </summary>
		[EditorBrowsable(EditorBrowsableState.Never)]
		public struct ReadLockScope : IDisposable
		{
			private ReaderWriterLockSlim? _readerWriterLock;

			/// <summary>
			/// Initializes a new instance of the <see cref="ReadLockScope"/> class.
			/// </summary>
			/// <param name="readerWriterLock">The <see cref="ReaderWriterLockSlim"/> instance.</param>
			[DebuggerStepThrough]
			internal ReadLockScope(ReaderWriterLockSlim readerWriterLock) =>
				_readerWriterLock = readerWriterLock;

			/// <summary>
			/// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
			/// </summary>
			[DebuggerStepThrough]
			public void Dispose()
			{
				_readerWriterLock?.ExitReadLock();
				_readerWriterLock = null;
			}
		}
		#endregion

		#region Inner type: WriteLockScope
		/// <summary>
		/// The <see cref="ReaderWriterLockSlim"/> wrapper.
		/// </summary>
		[EditorBrowsable(EditorBrowsableState.Never)]
		public struct WriteLockScope : IDisposable
		{
			private ReaderWriterLockSlim? _readerWriterLock;

			/// <summary>
			/// Initializes a new instance of the <see cref="WriteLockScope"/> class.
			/// </summary>
			/// <param name="readerWriterLock">The <see cref="ReaderWriterLockSlim"/> instance.</param>
			[DebuggerStepThrough]
			internal WriteLockScope(ReaderWriterLockSlim readerWriterLock) =>
				_readerWriterLock = readerWriterLock;

			/// <summary>
			/// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
			/// </summary>
			[DebuggerStepThrough]
			public void Dispose()
			{
				_readerWriterLock?.ExitWriteLock();
				_readerWriterLock = null;
			}
		}
		#endregion

		#region Inner type: UpgradeableReadLockScope
		/// <summary>
		/// The <see cref="ReaderWriterLockSlim"/> wrapper.
		/// </summary>
		[EditorBrowsable(EditorBrowsableState.Never)]
		public struct UpgradeableReadLockScope : IDisposable
		{
			private ReaderWriterLockSlim? _readerWriterLock;

			/// <summary>
			/// Initializes a new instance of the <see cref="UpgradeableReadLockScope"/> class.
			/// </summary>
			/// <param name="readerWriterLock">The <see cref="ReaderWriterLockSlim"/> instance.</param>
			[DebuggerStepThrough]
			public UpgradeableReadLockScope(ReaderWriterLockSlim readerWriterLock) =>
				_readerWriterLock = readerWriterLock;

			/// <summary>
			/// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
			/// </summary>
			[DebuggerStepThrough]
			public void Dispose()
			{
				_readerWriterLock?.ExitUpgradeableReadLock();
				_readerWriterLock = null;
			}
		}
		#endregion
	}
}