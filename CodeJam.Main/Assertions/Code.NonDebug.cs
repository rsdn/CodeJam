using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;

using JetBrains.Annotations;

using static CodeJam.Targeting.MethodImplOptionsEx;

namespace CodeJam
{
	/// <summary>Assertions class.</summary>
	// Part that excluded from debug assertions generation.
	partial class Code
	{
		#region Argument validation (DO NOT copy into DebugCode)
		// NB: Conditional methods cannot have a return type.

		/// <summary>
		/// Creates <see cref="ArgumentAssertion{T}"/> for fluent assertions.
		/// </summary>
		/// <typeparam name="T">Type of argument</typeparam>
		/// <param name="arg">Argument value.</param>
		/// <param name="argName">Argument name.</param>
		/// <returns><see cref="ArgumentAssertion{T}"/> instance.</returns>
		[DebuggerHidden, MethodImpl(AggressiveInlining)]
		[MustUseReturnValue]
		[AssertionMethod]
		public static ArgumentAssertion<T> Arg<T>(T arg, [InvokerParameterName] string argName) => new(arg, argName);
		#endregion

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
			TDisposable thisReference)
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
			TDisposable thisReference,
			string message)
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
			TDisposable thisReference,
			string messageFormat,
			params object[]? args)
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
		[ContractAnnotation("resource: null => halt")]
		public static void DisposedIfNull<TResource, TDisposable>(
			[System.Diagnostics.CodeAnalysis.NotNull, NoEnumeration] TResource? resource,
			TDisposable thisReference)
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
		[ContractAnnotation("resource: null => halt")]
		public static void DisposedIfNull<TResource, TDisposable>(
			[NoEnumeration] TResource? resource,
			TDisposable thisReference,
			string message)
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
		[ContractAnnotation("resource: null => halt")]
		public static void DisposedIfNull<TResource, TDisposable>(
			[NoEnumeration] TResource? resource,
			TDisposable thisReference,
			string messageFormat,
			params object[]? args)
			where TResource : class
			where TDisposable : IDisposable
		{
			if (resource == null)
				throw CodeExceptions.ObjectDisposed(thisReference.GetType(), messageFormat, args);
		}
		#endregion
	}
}