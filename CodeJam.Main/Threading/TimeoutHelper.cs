using System;
using System.Threading;

using JetBrains.Annotations;

#if !(NETCOREAPP20_OR_GREATER || NETSTANDARD21_OR_GREATER)
using CodeJam.Dates;
#endif

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
			new(0, 0, 0, 0, -1);
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

		/// <summary>
		/// Calculates exponential backoff timeout for specific retry attempt.
		/// Returns timeout equal to (2^(<paramref name="retryAttempt"/>-1)) limited to <paramref name="maxRetryInterval"/>
		/// Method applies ±20% jitter to the return value to provide even distribution of the timeouts.
		/// </summary>
		/// <remarks>
		/// SEE https://brooker.co.za/blog/2015/03/21/backoff.html
		/// https://aws.amazon.com/ru/blogs/architecture/exponential-backoff-and-jitter/
		/// for explanation
		/// </remarks>
		public static TimeSpan ExponentialBackoffTimeout(int retryAttempt, TimeSpan retryInterval, TimeSpan maxRetryInterval)
		{
			if (retryInterval <= TimeSpan.Zero)
				return retryInterval;

			if (retryAttempt <= 0)
				retryAttempt = 1;

			// 0.8..1.2
			var jitter = 0.8 + Objects.Random.NextDouble() * 0.4;

			// (2^retryCount - 1) * jitter
			var scale = Math.Pow(2.0, retryAttempt - 1.0);

			// limit before multiply to ensure we are in valid values range.
			var maxScale = maxRetryInterval.TotalMilliseconds / retryInterval.TotalMilliseconds;

			// Apply scale coefficient and jitter
			var resultScale = Math.Min(scale, maxScale) * jitter;

			// Truncate by max retry interval
			return retryInterval.Multiply(resultScale);
		}
	}
}