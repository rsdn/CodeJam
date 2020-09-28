using System;

using JetBrains.Annotations;

namespace CodeJam.Dates
{
	/// <summary>
	/// Helper methods for date manipulations
	/// </summary>
	[PublicAPI]
	public static partial class DateTimeExtensions
	{
		// DONTTOUCH: benchmark first.
		// This implementation is fast enough, ~1.5x compared to long division.
		// internal as covered by test.
		internal static long DivideRoundToEvenNaive([NonNegativeValue] long ticks, [NonNegativeValue] long ticksModule)
		{
			DebugCode.BugIf(ticks < 0, "value < 0");
			DebugCode.BugIf(ticksModule <= 0, "div <= 0");

#if TARGETS_NET || NETSTANDARD20_OR_GREATER || NETCOREAPP20_OR_GREATER
			var truncate = Math.DivRem(ticks, ticksModule, out var offset);
#else
			var truncate = ticks / ticksModule;
			var offset = ticks % ticksModule;
#endif

			if (offset == 0)
				return truncate;

			var doubleOffset = offset << 1;
			if (doubleOffset < ticksModule) // below midpoint
				return truncate;
			if (doubleOffset > ticksModule) // above midpoint
				return truncate + 1;

			// Tie breaker part, round to nearest even
			if (truncate % 2 == 0)
				return truncate;

			return truncate + 1;
		}

		private static DateTime Create(DateTime origin, [NonNegativeValue] int year, [NonNegativeValue] int month, int day) =>
			new DateTime(year, month, day, 0, 0, 0, origin.Kind);

		private static DateTimeOffset Create(DateTimeOffset origin, [NonNegativeValue] int year, [NonNegativeValue] int month, int day) =>
			new DateTimeOffset(year, month, day, 0, 0, 0, origin.Offset);

		private static DateTime Create(DateTime origin, [NonNegativeValue] long ticks) =>
			new DateTime(ticks, origin.Kind);

		private static DateTimeOffset Create(DateTimeOffset origin, [NonNegativeValue] long ticks) =>
			new DateTimeOffset(ticks, origin.Offset);

		/// <summary>Returns count of days in month.</summary>
		/// <param name="date">The date.</param>
		/// <returns>Count of days in month.</returns>
		[Pure]
		public static int DaysInMonth(this DateTime date) => DateTime.DaysInMonth(date.Year, date.Month);

		/// <summary>Returns count of days in month.</summary>
		/// <param name="date">The date.</param>
		/// <returns>Count of days in month.</returns>
		[Pure]
		public static int DaysInMonth(this DateTimeOffset date) => DateTime.DaysInMonth(date.Year, date.Month);

		/// <summary>Returns count of days in year.</summary>
		/// <param name="date">The date.</param>
		/// <returns>Count of days in year.</returns>
		[Pure]
		public static int DaysInYear(this DateTime date) => DateTime.IsLeapYear(date.Year) ? 366 : 365;

		/// <summary>Returns count of days in year.</summary>
		/// <param name="date">The date.</param>
		/// <returns>Count of days in year.</returns>
		[Pure]
		public static int DaysInYear(this DateTimeOffset date) => DateTime.IsLeapYear(date.Year) ? 366 : 365;

		/// <summary>
		/// Determines whether this instance is UTC date time (with zero <see cref="DateTimeOffset.Offset"/>).
		/// </summary>
		/// <param name="date">The date.</param>
		/// <returns>
		///   <c>true</c> if the specified date is UTC; otherwise, <c>false</c>.
		/// </returns>
		[Pure]
		public static bool IsUtc(this DateTimeOffset date) => date.Offset == TimeSpan.Zero;

		/// <summary>
		/// Returns value with Date and Offset components.
		/// </summary>
		[Pure]
		public static DateTimeOffset TruncateTime(this DateTimeOffset date) => new DateTimeOffset(date.Date, date.Offset);

		/// <summary>
		/// Converts value to <see cref="DateTimeOffset"/>.
		/// </summary>
		[Pure]
		public static DateTimeOffset ToOffset(this DateTime date) => new DateTimeOffset(date);

		/// <summary>
		/// Converts value to <see cref="DateTimeOffset"/>.
		/// </summary>
		[Pure]
		public static DateTimeOffset ToOffset(this DateTime date, TimeSpan offset) => new DateTimeOffset(date, offset);
	}
}