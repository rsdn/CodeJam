using System;
using System.Threading;

using JetBrains.Annotations;

#if NET45_OR_GREATER || TARGETS_NETSTANDARD || TARGETS_NETCOREAPP
using TaskEx = System.Threading.Tasks.Task;
#else
using TaskEx = System.Threading.Tasks.TaskEx;
#endif

namespace CodeJam.Threading
{
	/// <summary>
	/// Helper methods for timespan timeouts
	/// </summary>
	[PublicAPI]
	public static class TimeoutHelper
	{
		/// <summary>
		/// A constant used to specify an infinite waiting period, for methods that accept a TimeSpan parameter
		/// </summary>
		public static readonly TimeSpan InfiniteTimeSpan =
#if NET45_OR_GREATER || TARGETS_NETSTANDARD || TARGETS_NETCOREAPP
			Timeout.InfiniteTimeSpan;
#else
			new TimeSpan(0, 0, 0, 0, -1);
#endif

		/// <summary>
		/// Replaces negative <paramref name="timeout"/> value with <see cref="InfiniteTimeSpan"/>.
		/// If <paramref name="infiniteIfDefault"/> is <c>true</c>, <see cref="TimeSpan.Zero"/> value is treated as <see cref="InfiniteTimeSpan"/>
		/// </summary>
		/// <remarks>
		/// Use case scenario: methods that accept timeout often accept only <see cref="InfiniteTimeSpan"/>
		/// but not other negative values. Check <see cref="TaskEx.Delay(int)"/> as example.
		/// Motivation for <paramref name="infiniteIfDefault"/>:
		/// default timeout in configs often means 'infinite timeout', not 'do not wait and return immediately'.
		/// </remarks>
		public static TimeSpan AdjustTimeout(this TimeSpan timeout, bool infiniteIfDefault = false)
		{
			if (infiniteIfDefault)
				return timeout <= TimeSpan.Zero
					? InfiniteTimeSpan
					: timeout;

			return timeout < TimeSpan.Zero
				? InfiniteTimeSpan
				: timeout;
		}

		/// <summary>
		/// Limits timeout by upper limit.
		/// Replaces negative <paramref name="timeout"/> value with <see cref="InfiniteTimeSpan"/>.
		/// If <paramref name="infiniteIfDefault"/> is <c>true</c>, <see cref="TimeSpan.Zero"/> value is treated as <see cref="InfiniteTimeSpan"/>
		/// </summary>
		/// <remarks>
		/// Use case scenario: methods that accept timeout often accept only <see cref="InfiniteTimeSpan"/>
		/// but not other negative values. Check <see cref="TaskEx.Delay(int)"/> as example.
		/// Motivation for <paramref name="infiniteIfDefault"/>:
		/// default timeout in configs often means 'infinite timeout', not 'do not wait and return immediately'.
		/// </remarks>
		public static TimeSpan AdjustTimeout(this TimeSpan timeout, TimeSpan upperLimit, bool infiniteIfDefault = false)
		{
			timeout = timeout.AdjustTimeout(infiniteIfDefault);
			upperLimit = upperLimit.AdjustTimeout(infiniteIfDefault);

			// Ignore upper limit if negative
			if (upperLimit < TimeSpan.Zero)
				return timeout;

			// Ignore timeout if negative or exceeds upper limit
			if (timeout < TimeSpan.Zero || timeout > upperLimit)
				return upperLimit;

			return timeout;
		}
	}
}