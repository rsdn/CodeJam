using System;
using System.Collections.Generic;
#if NETSTANDARD21_OR_GREATER || NETCOREAPP30_OR_GREATER
using System.Threading.Tasks;
#endif
using CodeJam.Internal;

using JetBrains.Annotations;

namespace CodeJam
{
	/// <summary>The <see cref="IDisposable"/> extensions.</summary>
	[PublicAPI]
	public static class DisposableExtensions
	{
		/// <summary>Invokes the dispose for each item in the <paramref name="disposables"/>.</summary>
		/// <param name="disposables">The multiple <see cref="IDisposable"/> instances.</param>
		/// <exception cref="AggregateException"></exception>
		public static void DisposeAll([InstantHandle] this IEnumerable<IDisposable> disposables)
		{
			Code.NotNull(disposables, nameof(disposables));

			List<Exception>? exceptions = null;

			foreach (var item in disposables)
			{
				try
				{
					item.Dispose();
				}
				catch (Exception ex)
				{
					exceptions = new List<Exception> { ex };
				}
			}

			if (exceptions != null)
				throw new AggregateException(exceptions);
		}

		/// <summary>Invokes the dispose for each item in the <paramref name="disposables"/>.</summary>
		/// <param name="disposables">The multiple <see cref="IDisposable"/> instances.</param>
		/// <param name="exceptionHandler">The exception handler.</param>
		public static void DisposeAll(
			[InstantHandle] this IEnumerable<IDisposable> disposables,
			[InstantHandle] Func<Exception, bool> exceptionHandler)
		{
			Code.NotNull(disposables, nameof(disposables));
			Code.NotNull(exceptionHandler, nameof(exceptionHandler));

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
			}
		}

#if NETSTANDARD21_OR_GREATER || NETCOREAPP30_OR_GREATER
		/// <summary>
		/// Calls DisposeAsync if <paramref name="disposable"/> implements <see cref="IAsyncDisposable"/>, otherwise
		/// calls <see cref="IDisposable.Dispose"/>
		/// </summary>
		public static ValueTask DisposeAsync(this IDisposable disposable)
		{
			Code.NotNull(disposable, nameof(disposable));
			if (disposable is IAsyncDisposable asyncDisposable)
				return asyncDisposable.DisposeAsync();

			disposable.Dispose();
			return new ValueTask();
		}
#endif
	}
}