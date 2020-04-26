using System;
using System.Collections.Generic;
using System.Threading;

using CodeJam.Internal;

using JetBrains.Annotations;

namespace CodeJam
{
	/// <summary>Helper methods for <see cref="IDisposable"/></summary>
	[PublicAPI]
	public static class Disposable
	{
		#region Nested types
		/// <summary>
		/// Empty <see cref="IDisposable"/> implementation.
		/// </summary>
		public sealed class EmptyDisposable : IDisposable
		{
			/// <summary>
			/// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
			/// </summary>
			public void Dispose() { }
		}

		/// DONTTOUCH: DO NOT make it a struct, passing the structure by value will result in multiple Dispose() calls.
		/// SEEALSO: https://blogs.msdn.microsoft.com/ericlippert/2011/03/14/to-box-or-not-to-box-that-is-the-question/
		private sealed class AnonymousDisposable : IDisposable
		{
			private Action _disposeAction;

			public AnonymousDisposable(Action disposeAction) => _disposeAction = disposeAction;

			public void Dispose()
			{
				// ReSharper disable once AssignNullToNotNullAttribute
				var disposeAction = Interlocked.Exchange(ref _disposeAction, null);
				// ReSharper disable once ConditionIsAlwaysTrueOrFalse
				if (disposeAction != null)
				{
					try
					{
						disposeAction.Invoke();
					}
					catch when (OnException(disposeAction))
					{
					}
				}
			}

			private bool OnException(Action disposeAction)
			{
				Interlocked.Exchange(ref _disposeAction, disposeAction);
				return false;
			}
		}

		/// DONTTOUCH: DO NOT make it a struct, passing the structure by value will result in multiple Dispose() calls.
		/// SEEALSO: https://blogs.msdn.microsoft.com/ericlippert/2011/03/14/to-box-or-not-to-box-that-is-the-question/
		private sealed class AnonymousDisposable<T> : IDisposable
		{
			private Action<T> _disposeAction;
			private T _state;

			public AnonymousDisposable(Action<T> disposeAction, T state)
			{
				_disposeAction = disposeAction;
				_state = state;
			}

			public void Dispose()
			{
				// ReSharper disable once AssignNullToNotNullAttribute
				var disposeAction = Interlocked.Exchange(ref _disposeAction, null);
				// ReSharper disable once ConditionIsAlwaysTrueOrFalse
				if (disposeAction != null)
				{
					try
					{
						disposeAction.Invoke(_state);
						_state = default;
					}
					catch when (OnException(disposeAction))
					{
					}
				}
			}

			private bool OnException(Action<T> disposeAction)
			{
				Interlocked.Exchange(ref _disposeAction, disposeAction);
				return false;
			}
		}
		#endregion

		/// <summary>Empty <see cref="IDisposable"/> implementation.</summary>
		public static readonly EmptyDisposable Empty = new EmptyDisposable();

		/// <summary>
		/// Creates <see cref="IDisposable"/> instance that calls <paramref name="disposeAction"/> on disposing.
		/// </summary>
		/// <param name="disposeAction">The dispose action.</param>
		/// <returns>
		/// Instance of <see cref="IDisposable"/> that calls <paramref name="disposeAction"/> on disposing.
		/// </returns>
		[NotNull, Pure]
		public static IDisposable Create([NotNull] Action disposeAction) => new AnonymousDisposable(disposeAction);

		/// <summary>
		/// Creates <see cref="IDisposable"/> instance that calls <paramref name="disposeAction"/> on disposing.
		/// </summary>
		/// <typeparam name="T">Disposable state type.</typeparam>
		/// <param name="disposeAction">The dispose action.</param>
		/// <param name="state">A value that contains data for the disposal action.</param>
		/// <returns>
		/// Instance of <see cref="IDisposable"/> that calls <paramref name="disposeAction"/> on disposing.
		/// </returns>
		[NotNull, Pure]
		public static IDisposable Create<T>([NotNull] Action<T> disposeAction, [CanBeNull] T state) => new AnonymousDisposable<T>(disposeAction, state);

		/// <summary>Combine multiple <see cref="IDisposable"/> instances into single one.</summary>
		/// <param name="disposables">The disposables.</param>
		/// <returns>Instance of <see cref="IDisposable"/> that will dispose the specified disposables.</returns>
		[NotNull, Pure]
		public static IDisposable Merge([NotNull, ItemNotNull] params IDisposable[] disposables) => Merge((IEnumerable<IDisposable>)disposables);

		/// <summary>Combine multiple <see cref="IDisposable"/> instances into single one.</summary>
		/// <param name="disposables">The disposables.</param>
		/// <returns>Instance of <see cref="IDisposable"/> that will dispose the specified disposables.</returns>
		[NotNull, Pure]
		public static IDisposable Merge([NotNull] this IEnumerable<IDisposable> disposables) =>
			Create(disposables.DisposeAll);

		/// <summary>Invokes the dispose for each item in the <paramref name="disposables"/>.</summary>
		/// <param name="disposables">The multiple <see cref="IDisposable"/> instances.</param>
		/// <exception cref="AggregateException"></exception>
		public static void DisposeAll([NotNull, ItemNotNull, InstantHandle] this IEnumerable<IDisposable> disposables)
		{
			List<Exception> exceptions = null;

			foreach (var item in disposables)
			{
				try
				{
					item.Dispose();
				}
				catch (Exception ex)
				{
					exceptions ??= new List<Exception>();
					exceptions.Add(ex);
				}
			}

			if (exceptions != null)
				throw new AggregateException(exceptions);
		}

		/// <summary>Invokes the dispose for each item in the <paramref name="disposables"/>.</summary>
		/// <param name="disposables">The multiple <see cref="IDisposable"/> instances.</param>
		/// <param name="exceptionHandler">The exception handler.</param>
		public static void DisposeAll(
			[NotNull, ItemNotNull, InstantHandle] this IEnumerable<IDisposable> disposables,
			[NotNull, InstantHandle] Func<Exception, bool> exceptionHandler)
		{
			List<Exception> exceptions = null;
			foreach (var item in disposables)
			{
				try
				{
					item.Dispose();
				}
				catch (Exception ex) when (exceptionHandler(ex))
				{
					ex.LogToCodeTraceSourceOnCatch(true);
				}
				catch (Exception ex)
				{
					exceptions ??= new List<Exception>();
					exceptions.Add(ex);
				}
			}

			if (exceptions != null)
				throw new AggregateException(exceptions);
		}
	}
}