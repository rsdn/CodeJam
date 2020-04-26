#if NETSTANDARD21_OR_GREATER || NETCOREAPP30_OR_GREATER
using System;
using System.Threading.Tasks;

using JetBrains.Annotations;

namespace CodeJam
{
	/// <summary>The <see cref="IAsyncDisposable"/> helpers.</summary>
	public static partial class AsyncDisposable
	{
		/// <summary>
		/// Calls DisposeAsync if <paramref name="disposable"/> implements <see cref="IAsyncDisposable"/>, otherwise
		/// calls <see cref="IDisposable.Dispose"/>
		/// </summary>
		public static IAsyncDisposable ToAsyncDisposable([NotNull] this IDisposable disposable)
		{
			Code.NotNull(disposable, nameof(disposable));
			if (disposable is IAsyncDisposable asyncDisposable)
				return asyncDisposable;

			disposable.Dispose();
			return Create(
				d =>
				{
					d.Dispose();
					return new ValueTask();
				},
				disposable);
		}

		/// <summary>
		/// Calls DisposeAsync if <paramref name="disposable"/> implements <see cref="IAsyncDisposable"/>, otherwise
		/// calls <see cref="IDisposable.Dispose"/>
		/// </summary>
		public static ValueTask DisposeAsync([NotNull] this IDisposable disposable)
		{
			Code.NotNull(disposable, nameof(disposable));
			if (disposable is IAsyncDisposable asyncDisposable)
				return asyncDisposable.DisposeAsync();

			disposable.Dispose();
			return new ValueTask();
		}
	}
}
#endif