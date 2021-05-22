using System;

using JetBrains.Annotations;

namespace CodeJam
{
	/// <summary>Helper methods for <see cref="IDisposable"/></summary>
	[PublicAPI]
	public static class InitDispose
	{
		/// <summary>
		/// Calls <paramref name="initAction"/> and
		/// creates <see cref="IDisposable"/> instance that calls <paramref name="disposeAction"/> on disposing.
		/// </summary>
		/// <param name="initAction">The init action.</param>
		/// <param name="disposeAction">The dispose action.</param>
		/// <returns>
		/// Instance of <see cref="IDisposable"/> that calls <paramref name="disposeAction"/> on disposing.
		/// </returns>
		[Pure, System.Diagnostics.Contracts.Pure]
		public static IDisposable Create(Action initAction, Action disposeAction)
		{
			Code.NotNull(initAction, nameof(initAction));
			Code.NotNull(disposeAction, nameof(disposeAction));

			initAction();
			return Disposable.Create(disposeAction);
		}

		/// <summary>
		/// Calls <paramref name="initAction"/> and
		/// creates <see cref="IDisposable"/> instance that calls <paramref name="disposeAction"/> on disposing.
		/// </summary>
		/// <typeparam name="T">Type of the initialization value.</typeparam>
		/// <param name="initAction">The init action.</param>
		/// <param name="disposeAction">The dispose action.</param>
		/// <returns>
		/// Instance of <see cref="IDisposable"/> that calls <paramref name="disposeAction"/> on disposing.
		/// </returns>
		[Pure, System.Diagnostics.Contracts.Pure]
		public static IDisposable Create<T>(Func<T> initAction, Action<T?> disposeAction)
		{
			Code.NotNull(initAction, nameof(initAction));
			Code.NotNull(disposeAction, nameof(disposeAction));

			var initState = initAction();
			return Disposable.Create(disposeAction, initState);
		}

		/// <summary>
		/// Calls <paramref name="initDisposeAction"/> and
		/// creates <see cref="IDisposable"/> instance that calls <paramref name="initDisposeAction"/> on disposing.
		/// </summary>
		/// <param name="initDisposeAction">The init and dispose action.</param>
		/// <returns>
		/// Instance of <see cref="IDisposable"/> that calls <paramref name="initDisposeAction"/> on disposing.
		/// </returns>
		[Pure, System.Diagnostics.Contracts.Pure]
		public static IDisposable Create(Action initDisposeAction)
		{
			Code.NotNull(initDisposeAction, nameof(initDisposeAction));

			initDisposeAction();
			return Disposable.Create(initDisposeAction);
		}

		/// <summary>
		/// Calls <paramref name="initDisposeAction"/> and
		/// creates <see cref="IDisposable"/> instance that calls <paramref name="initDisposeAction"/> on disposing.
		/// </summary>
		/// <param name="initDisposeAction">
		/// The init and dispose action.
		/// <paramref name="initDisposeAction"/> takes true if this is initAction.
		/// It takes false if this is disposeAction.
		/// </param>
		/// <returns>
		/// Instance of <see cref="IDisposable"/> that calls <paramref name="initDisposeAction"/> on disposing.
		/// </returns>
		[Pure, System.Diagnostics.Contracts.Pure]
		public static IDisposable Create(Action<bool> initDisposeAction)
		{
			Code.NotNull(initDisposeAction, nameof(initDisposeAction));

			initDisposeAction(true);
			return Disposable.Create(initDisposeAction, false);
		}
	}
}