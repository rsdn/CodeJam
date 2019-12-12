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
		private static DateTime Create(DateTime origin, int year, int month, int day) =>
			new DateTime(year, month, day, 0, 0, 0, origin.Kind);

		private static DateTimeOffset Create(DateTimeOffset origin, int year, int month, int day) =>
			new DateTimeOffset(year, month, day, 0, 0, 0, origin.Offset);

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