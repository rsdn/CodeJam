using System;
using System.Collections.Generic;
#if TARGETS_NET || LESSTHAN_NETSTANDARD21 || LESSTHAN_NETCOREAPP30
#else
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
					if (exceptions == null)
						exceptions = new List<Exception>();

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

#if TARGETS_NET || LESSTHAN_NETSTANDARD21 || LESSTHAN_NETCOREAPP30
#else
		/// <summary>
		/// Calls DisposeAsync if <paramref name="disposable"/> implements <see cref="IAsyncDisposable"/>, otherwise
		/// calls <see cref="IDisposable.Dispose"/>
		/// </summary>
		public static async Task DisposeAsync([NotNull] this IDisposable disposable)
		{
			Code.NotNull(disposable, nameof(disposable));
			if (disposable is IAsyncDisposable asyncDisposable)
				await asyncDisposable.DisposeAsync();
			else
				disposable.Dispose();
		}
#endif
	}
}