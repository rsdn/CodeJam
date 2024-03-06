using JetBrains.Annotations;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

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
			private Action? _disposeAction;

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
					catch when (OnException(disposeAction)) { }
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
			private Action<T?>? _disposeAction;

			private T? _state;

			/// <summary>Initialize instance.</summary>
			/// <param name="disposeAction">The dispose action.</param>
			/// <param name="state">A value that contains data for the disposal action.</param>
			public AnonymousDisposable(Action<T?> disposeAction, T? state)
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
					catch when (OnException(disposeAction)) { }
				}
			}

			private bool OnException(Action<T?> disposeAction)
			{
				Interlocked.Exchange(ref _disposeAction, disposeAction);
				return false;
			}
		}

		/// <summary>
		/// Disposable wrapper for single object.
		/// </summary>
		/// <typeparam name="T">Type of wrapped object that needed to be deinitialized.</typeparam>
		public class CustomDisposable<T> : IDisposable
		{
			private readonly Action<T> _destroyer;

			private bool _wasDisposed = false;

			private readonly T _entity;

			public T Entity => (_wasDisposed ? throw new ObjectDisposedException(nameof(_entity)) : _entity);

			public CustomDisposable(T entity, Action<T> destroyer)
			{
				_entity = entity;
				_destroyer = destroyer;
			}

			public void Dispose()
			{
				if (_wasDisposed) return; else _wasDisposed = true;
				_destroyer(_entity);
			}
		}

		/// <summary>
		/// Disposable wrapper for multiple objects.
		/// </summary>
		/// <typeparam name="TE">Type of wrapped objects collection, in which each element must be deinitialized.</typeparam>
		/// <typeparam name="T">Type of element of wrapped objects collection.</typeparam>
		public class CustomDisposable<TE, T> : IDisposable
			where TE : IEnumerable<T>
		{

			private readonly Action<T> _destroyer;

			private bool _wasDisposed = false;

			private readonly TE _entities;

			public TE Entities => (_wasDisposed ? throw new ObjectDisposedException(nameof(_entities)) : _entities);

			public CustomDisposable(TE entities, Action<T> destroyer)
			{
				_entities = entities;
				_destroyer = destroyer;
			}

			public void Dispose()
			{
				if (_wasDisposed) return; else _wasDisposed = true;
				_entities.Select(e => (IDisposable)new CustomDisposable<T>(e, _destroyer)).DisposeAll();
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
		[Pure, System.Diagnostics.Contracts.Pure]
		public static IDisposable Create(Action disposeAction) => new AnonymousDisposable(disposeAction);

		/// <summary>
		/// Creates <see cref="IDisposable"/> instance that calls <paramref name="disposeAction"/> on disposing.
		/// </summary>
		/// <typeparam name="T">Disposable state type.</typeparam>
		/// <param name="disposeAction">The dispose action.</param>
		/// <param name="state">A value that contains data for the disposal action.</param>
		/// <returns>
		/// Instance of <see cref="IDisposable"/> that calls <paramref name="disposeAction"/> on disposing.
		/// </returns>
		[Pure, System.Diagnostics.Contracts.Pure]
		public static IDisposable Create<T>(Action<T?> disposeAction, T? state) =>
			new AnonymousDisposable<T>(disposeAction, state);

		/// <summary>
		/// Creates disposable wrapper for single object.
		/// </summary>
		/// <typeparam name="T">Type of wrapped object that needed to be deinitialized.</typeparam>
		/// <param name="creator">Used at place immediately to initialize internal entity inside returning disposable wrapper.</param>
		/// <param name="destroyer">Used during internal entity deinitialization as an action of the dispose method.</param>
		/// <returns>Created disposable wrapper of single object.</returns>
		public static CustomDisposable<T> Create<T>(Func<T> creator, Action<T> destroyer) =>
			new(creator(), destroyer);

		/// <summary>
		/// Creates disposable wrapper for multiple objects.
		/// </summary>
		/// <typeparam name="TE">Type of wrapped objects collection, in which each element must be deinitialized.</typeparam>
		/// <typeparam name="T">Type of element of wrapped objects collection.</typeparam>
		/// <param name="creator">Used at place immediately to initialize internal entities inside returning disposable wrapper.</param>
		/// <param name="destroyer">Used during each internal entity deinitialization as an action of the dispose method.</param>
		/// <returns>Created disposable wrapper of multiple objects.</returns>
		public static CustomDisposable<TE, T> Create<TE, T>(Func<TE> creator, Action<T> destroyer) where TE : IEnumerable<T> =>
			new(creator(), destroyer);

		/// <summary>
		/// Creates disposable wrapper for multiple objects.
		/// </summary>
		/// <typeparam name="TTe">Type of wrapped objects collection, in which each element must be deinitialized.</typeparam>
		/// <typeparam name="T">Type of element of wrapped objects collection.</typeparam>
		/// <param name="counter">Used at place immediately to get source collection for internal entities initialize enumeration.</param>
		/// <param name="creator">Used at place immediately to initialize each internal entity inside returning disposable wrapper.</param>
		/// <param name="destroyer">Used during each internal entity deinitialization as an action of the dispose method.</param>
		/// <returns>Created disposable wrapper of multiple objects.</returns>
		public static CustomDisposable<IEnumerable<T>, T> Create<TTe, T>(Func<IEnumerable<TTe>> counter, Func<TTe, T> creator, Action<T> destroyer) =>
			new(counter().Select(creator).ToArray(), destroyer);

		/// <summary>Combine multiple <see cref="IDisposable"/> instances into single one.</summary>
		/// <param name="disposables">The disposables.</param>
		/// <returns>Instance of <see cref="IDisposable"/> that will dispose the specified disposables.</returns>
		[Pure, System.Diagnostics.Contracts.Pure]
		public static IDisposable Merge(params IDisposable[] disposables)
			=> Merge((IEnumerable<IDisposable>)disposables);

		/// <summary>Combine multiple <see cref="IDisposable"/> instances into single one.</summary>
		/// <param name="disposables">The disposables.</param>
		/// <returns>Instance of <see cref="IDisposable"/> that will dispose the specified disposables.</returns>
		[Pure, System.Diagnostics.Contracts.Pure]
		public static IDisposable Merge(this IEnumerable<IDisposable> disposables) =>
			Create(disposables.DisposeAll);
	}
}