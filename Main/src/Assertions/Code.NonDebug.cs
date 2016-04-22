using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;

using JetBrains.Annotations;

using static CodeJam.PlatformDependent;

namespace CodeJam
{
	/// <summary>Assertions class.</summary>
	// Part that excluded from debug assertions generation.
	static partial class Code
	{
		#region DisposedIf assertions (DO NOT copy into DebugCode)
		// NB: ObjectDisposedException should be thrown from all builds or not thrown at all.
		// There's no point in pairing these assertions with a debug-time-only ones

		/// <summary>Assertion for object disposal</summary>
		/// <typeparam name="TDisposable">The type of the disposable.</typeparam>
		/// <param name="disposed">Dispose condition.</param>
		/// <param name="thisReference">The this reference.</param>
		[DebuggerHidden, MethodImpl(AggressiveInlining)]
		[AssertionMethod]
		public static void DisposedIf<TDisposable>(
			bool disposed,
			[NotNull] TDisposable thisReference)
			where TDisposable : IDisposable
		{
			if (disposed)
				throw CodeExceptions.ObjectDisposed(thisReference.GetType());
		}

		/// <summary>Assertion for object disposal</summary>
		/// <typeparam name="TDisposable">The type of the disposable.</typeparam>
		/// <param name="disposed">Dispose condition.</param>
		/// <param name="thisReference">The this reference.</param>
		/// <param name="message">The message.</param>
		[DebuggerHidden, MethodImpl(AggressiveInlining)]
		[AssertionMethod]
		public static void DisposedIf<TDisposable>(
			bool disposed,
			[NotNull] TDisposable thisReference,
			[NotNull] string message)
			where TDisposable : IDisposable
		{
			if (disposed)
				throw CodeExceptions.ObjectDisposed(thisReference.GetType(), message);
		}

		/// <summary>Assertion for object disposal</summary>
		/// <typeparam name="TDisposable">The type of the disposable.</typeparam>
		/// <param name="disposed">Dispose condition.</param>
		/// <param name="thisReference">The this reference.</param>
		/// <param name="messageFormat">The message format.</param>
		/// <param name="args">The arguments.</param>
		[DebuggerHidden, MethodImpl(AggressiveInlining)]
		[AssertionMethod, StringFormatMethod("messageFormat")]
		public static void DisposedIf<TDisposable>(
			bool disposed,
			[NotNull] TDisposable thisReference,
			[NotNull] string messageFormat,
			[CanBeNull] params object[] args)
			where TDisposable : IDisposable
		{
			if (disposed)
				throw CodeExceptions.ObjectDisposed(thisReference.GetType(), messageFormat, args);
		}

		/// <summary>Assertion for object disposal</summary>
		/// <typeparam name="TResource">The type of the resource.</typeparam>
		/// <typeparam name="TDisposable">The type of the disposable.</typeparam>
		/// <param name="resource">The resource. Should be not null if the object is not disposed.</param>
		/// <param name="thisReference">The this reference.</param>
		[DebuggerHidden, MethodImpl(AggressiveInlining)]
		[AssertionMethod]
		public static void DisposedIfNull<TResource, TDisposable>(
			[CanBeNull] TResource resource,
			[NotNull] TDisposable thisReference)
			where TResource : class
			where TDisposable : IDisposable
		{
			if (resource == null)
				throw CodeExceptions.ObjectDisposed(thisReference.GetType());
		}

		/// <summary>Assertion for object disposal</summary>
		/// <typeparam name="TResource">The type of the resource.</typeparam>
		/// <typeparam name="TDisposable">The type of the disposable.</typeparam>
		/// <param name="resource">The resource. Should be not null if the object is not disposed.</param>
		/// <param name="thisReference">The this reference.</param>
		/// <param name="message">The message.</param>
		[DebuggerHidden, MethodImpl(AggressiveInlining)]
		[AssertionMethod]
		public static void DisposedIfNull<TResource, TDisposable>(
			[CanBeNull] TResource resource,
			[NotNull] TDisposable thisReference,
			[NotNull] string message)
			where TResource : class
			where TDisposable : IDisposable
		{
			if (resource == null)
				throw CodeExceptions.ObjectDisposed(thisReference.GetType(), message);
		}

		/// <summary>Assertion for object disposal</summary>
		/// <typeparam name="TResource">The type of the resource.</typeparam>
		/// <typeparam name="TDisposable">The type of the disposable.</typeparam>
		/// <param name="resource">The resource. Should be not null if the object is not disposed.</param>
		/// <param name="thisReference">The this reference.</param>
		/// <param name="messageFormat">The message format.</param>
		/// <param name="args">The arguments.</param>
		[DebuggerHidden, MethodImpl(AggressiveInlining)]
		[AssertionMethod, StringFormatMethod("messageFormat")]
		public static void DisposedIfNull<TResource, TDisposable>(
			[CanBeNull] TResource resource,
			[NotNull] TDisposable thisReference,
			[NotNull] string messageFormat,
			[CanBeNull] params object[] args)
			where TResource : class
			where TDisposable : IDisposable
		{
			if (resource == null)
				throw CodeExceptions.ObjectDisposed(thisReference.GetType(), messageFormat, args);
		}
		#endregion
	}
}