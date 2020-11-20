using System;
using System.Collections.Generic;
using System.Threading;

using JetBrains.Annotations;

namespace CodeJam
{
	/// <summary>Helper methods for <see cref="IDisposable"/></summary>
	[PublicAPI]
	public static class Disposable
	{
		#region Nested types
		/// <summary>
		/// The <see cref="IDisposable"/> implementation with no action on <see cref="Dispose"/>
		/// </summary>
		public sealed class EmptyDisposable : IDisposable
		{
			/// <summary>
			/// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
			/// </summary>
			public void Dispose() { }
		}

		/// <summary>
		/// The <see cref="IDisposable"/> implementation that calls supplied action on <see cref="Dispose"/>.
		/// </summary>
		/// DONTTOUCH: DO NOT make it a struct, passing the structure by value will result in multiple Dispose() calls.
		/// SEEALSO: https://blogs.msdn.microsoft.com/ericlippert/2011/03/14/to-box-or-not-to-box-that-is-the-question/
		private sealed class AnonymousDisposable : IDisposable
		{
			private Action _disposeAction;

			/// <summary>Initialize instance.</summary>
			/// <param name="disposeAction">The dispose action.</param>
			public AnonymousDisposable(Action disposeAction) => _disposeAction = disposeAction;

			/// <summary>
			/// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
			/// </summary>
			public void Dispose()
			{
				var disposeAction = Interlocked.Exchange(ref _disposeAction, null);
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

		/// <summary>
		/// The <see cref="IDisposable"/> implementation that calls supplied action on <see cref="Dispose"/>.
		/// </summary>
		/// <typeparam name="T">Disposable state type.</typeparam>
		/// DONTTOUCH: DO NOT make it a struct, passing the structure by value will result in multiple Dispose() calls.
		/// SEEALSO: https://blogs.msdn.microsoft.com/ericlippert/2011/03/14/to-box-or-not-to-box-that-is-the-question/
		private sealed class AnonymousDisposable<T> : IDisposable
		{
			private Action<T> _disposeAction;
			private T _state;

			/// <summary>Initialize instance.</summary>
			/// <param name="disposeAction">The dispose action.</param>
			/// <param name="state">A value that contains data for the disposal action.</param>
			public AnonymousDisposable(Action<T> disposeAction, T state)
			{
				_disposeAction = disposeAction;
				_state = state;
			}

			/// <summary>
			/// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
			/// </summary>
			public void Dispose()
			{
				var disposeAction = Interlocked.Exchange(ref _disposeAction, null);
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

		/// <summary><see cref="IDisposable"/> instance without any code in <see cref="IDisposable.Dispose"/>.</summary>
		public static readonly EmptyDisposable Empty = new();

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
	}
}