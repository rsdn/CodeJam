using System;

using JetBrains.Annotations;

namespace CodeJam.Dates
{
	/// <summary>
	/// Helper methods for date manipulations
	/// </summary>
	public static partial class DateTimeExtensions
	{
		#region With methods
		/// <summary>Updates the day of the date.</summary>
		/// <param name="date">The date.</param>
		/// <param name="day">The day.</param>
		/// <returns>Updated date.</returns>
		[Pure]
		public static DateTime WithDay(this DateTime date, int day)
		{
			Code.InRange(day, nameof(day), 1, date.DaysInMonth());

			var dayOffset = day - date.Day;
			return date.AddDays(dayOffset);
		}

		/// <summary>Updates the month of the date.</summary>
		/// <param name="date">The date.</param>
		/// <param name="month">The month.</param>
		/// <returns>Updated date.</returns>
		[Pure]
		public static DateTime WithMonth(this DateTime date, int month) => WithMonth(date, month, false);

		/// <summary>Updates the month of the date.</summary>
		/// <param name="date">The date.</param>
		/// <param name="month">The month.</param>
		/// <param name="useLastDay">if set to <c>true</c>, the call preserves last day of the month, 2000/02/28 => 2000/01/31.</param>
		/// <returns>Updated date.</returns>
		[Pure]
		public static DateTime WithMonth(this DateTime date, int month, bool useLastDay)
		{
			Code.InRange(month, nameof(month), 1, 12);

			var monthOffset = month - date.Month;
			return AddMonths(date, monthOffset, useLastDay);
		}

		/// <summary>Returns the date resulting from adding the given number of months to this date.</summary>
		/// <param name="date">The date.</param>
		/// <param name="monthOffset">The month offset.</param>
		/// <param name="useLastDay">if set to <c>true</c>, the call preserves last day of the month, 2000/02/28 => 2000/01/31.</param>
		/// <returns>Updated date.</returns>
		[Pure]
		public static DateTime AddMonths(this DateTime date, int monthOffset, bool useLastDay)
		{
			var result = date.AddMonths(monthOffset);
			if (useLastDay)
			{
				if (date.Day == date.DaysInMonth())
				{
					result = result.WithDay(result.DaysInMonth());
				}
			}

			return result;
		}

		/// <summary>Updates the year of the date.</summary>
		/// <param name="date">The date.</param>
		/// <param name="year">The year.</param>
		/// <returns>Updated date.</returns>
		[Pure]
		public static DateTime WithYear(this DateTime date, int year) => WithYear(date, year, false);

		/// <summary>Updates the year of the date.</summary>
		/// <param name="date">The date.</param>
		/// <param name="year">The year.</param>
		/// <param name="useLastDay">if set to <c>true</c>, the call preserves last day of the month, 2001/02/28 => 2004/02/29.</param>
		/// <returns>Updated date.</returns>
		[Pure]
		public static DateTime WithYear(this DateTime date, int year, bool useLastDay)
		{
			var yearOffset = year - date.Year;
			return AddYears(date, yearOffset, useLastDay);
		}

		/// <summary>Returns the date resulting from adding the given number of years to this date.</summary>
		/// <param name="date">The date.</param>
		/// <param name="yearOffset">The year offset.</param>
		/// <param name="useLastDay">if set to <c>true</c>, the call preserves last day of the month, 2001/02/28 => 2004/02/29.</param>
		/// <returns>Updated date.</returns>
		[Pure]
		public static DateTime AddYears(this DateTime date, int yearOffset, bool useLastDay)
		{
			var result = date.AddYears(yearOffset);
			if (useLastDay)
			{
				if (date.Day == date.DaysInMonth())
				{
					result = result.WithDay(result.DaysInMonth());
				}
			}

			return result;
		}

		/// <summary>Updates the month and the day of the date.</summary>
		/// <param name="date">The date.</param>
		/// <param name="month">The month.</param>
		/// <param name="day">The day.</param>
		/// <returns>Updated date.</returns>
		[Pure]
		public static DateTime WithMonthAndDay(this DateTime date, int month, int day) =>
			WithMonthAndDay(date, month, day, false);

		/// <summary>Updates the month and the day of the date.</summary>
		/// <param name="date">The date.</param>
		/// <param name="month">The month.</param>
		/// <param name="day">The day.</param>
		/// <param name="useLastDay">if set to <c>true</c>, the call preserves last day of the month, 2001/02/28 =&gt; 2004/02/29.</param>
		/// <returns>Updated date.</returns>
		[Pure]
		public static DateTime WithMonthAndDay(this DateTime date, int month, int day, bool useLastDay) =>
			date
				.WithMonth(month, useLastDay)
				.WithDay(day);

		/// <summary>Updates the year and the month of the date.</summary>
		/// <param name="date">The date.</param>
		/// <param name="month">The month.</param>
		/// <param name="year">The year.</param>
		/// <returns>Updated date.</returns>
		[Pure]
		public static DateTime WithYearAndMonth(this DateTime date, int year, int month) =>
			WithYearAndMonth(date, year, month, false);

		/// <summary>Updates the month and the year of the date.</summary>
		/// <param name="date">The date.</param>
		/// <param name="month">The month.</param>
		/// <param name="year">The year.</param>
		/// <param name="useLastDay">if set to <c>true</c>, the call preserves last day of the month, 2001/02/28 => 2004/02/29.</param>
		/// <returns>Updated date.</returns>
		[Pure]
		public static DateTime WithYearAndMonth(this DateTime date, int year, int month, bool useLastDay) =>
			date
				.WithYear(year, useLastDay)
				.WithMonth(month, useLastDay);
		#endregion

		#region Prev/Next
		/// <summary>Returns previous day of the date.</summary>
		/// <param name="date">The date.</param>
		/// <returns>Previous date.</returns>
		[Pure]
		public static DateTime PrevDay(this DateTime date) => date.AddDays(-1);

		/// <summary>Returns next day of the date.</summary>
		/// <param name="date">The date.</param>
		/// <returns>Next day.</returns>
		[Pure]
		public static DateTime NextDay(this DateTime date) => date.AddDays(1);

		/// <summary>Returns previous month of the date.</summary>
		/// <param name="date">The date.</param>
		/// <returns>Previous month.</returns>
		[Pure]
		public static DateTime PrevMonth(this DateTime date) => date.AddMonths(-1);

		/// <summary>Returns next month of the date.</summary>
		/// <param name="date">The date.</param>
		/// <returns>Next month.</returns>
		[Pure]
		public static DateTime NextMonth(this DateTime date) => date.AddMonths(1);

		/// <summary>Returns previous year of the date.</summary>
		/// <param name="date">The date.</param>
		/// <returns>Previous year.</returns>
		[Pure]
		public static DateTime PrevYear(this DateTime date) => date.AddYears(-1);

		/// <summary>Returns next year of the date.</summary>
		/// <param name="date">The date.</param>
		/// <returns>Next year.</returns>
		[Pure]
		public static DateTime NextYear(this DateTime date) => date.AddYears(1);
		#endregion

		#region First/Last
		/// <summary>Returns the first day of month.</summary>
		/// <param name="date">The date.</param>
		/// <returns>The first day of month.</returns>
		[Pure]
		public static DateTime FirstDayOfMonth(this DateTime date) => Create(date, date.Year, date.Month, 1);

		/// <summary>Returns the last day of month.</summary>
		/// <param name="date">The date.</param>
		/// <returns>The last day of month.</returns>
		[Pure]
		public static DateTime LastDayOfMonth(this DateTime date) => Create(
			date,
			date.Year,
			date.Month,
			date.DaysInMonth());

		/// <summary>Returns the first day of year.</summary>
		/// <param name="date">The date.</param>
		/// <returns>The first day of year.</returns>
		[Pure]
		public static DateTime FirstDayOfYear(this DateTime date) => Create(date, date.Year, 1, 1);

		/// <summary>Returns the last day of year.</summary>
		/// <param name="date">The date.</param>
		/// <returns>The last day of year.</returns>
		[Pure]
		public static DateTime LastDayOfYear(this DateTime date) => Create(date, date.Year, 12, 31);
		#endregion

		#region Rounding
		// THANKSTO: aj.toulan, https://stackoverflow.com/a/18796370
		[Pure]
		private static DateTime FloorCore(DateTime dateTime, long ticksModule)
		{
			Code.InRange(ticksModule, nameof(ticksModule), 1, long.MaxValue);

			var overflow = dateTime.Ticks % ticksModule;
			return dateTime.AddTicks(-overflow);
		}

		// THANKSTO: aj.toulan, https://stackoverflow.com/a/18796370
		[Pure]
		private static DateTime CeilingCore(DateTime dateTime, long ticksModule)
		{
			Code.InRange(ticksModule, nameof(ticksModule), 1, long.MaxValue);

			var overflow = dateTime.Ticks % ticksModule;
			return overflow == 0 ? dateTime : dateTime.AddTicks(ticksModule - overflow);
		}

		// THANKSTO: aj.toulan, https://stackoverflow.com/a/18796370
		[Pure]
		private static DateTime RoundAwayFromZeroCore(DateTime dateTime, long ticksModule)
		{
			Code.InRange(ticksModule, nameof(ticksModule), 1, long.MaxValue);

			var halfIntervalTicks = (ticksModule + 1) >> 1;
			var halfIntervalOverflow = (dateTime.Ticks + halfIntervalTicks) % ticksModule;
			return dateTime.AddTicks(halfIntervalTicks - halfIntervalOverflow);
		}

		[Pure]
		private static DateTime RoundToEvenCore(DateTime dateTime, long ticksModule)
		{
			Code.InRange(ticksModule, nameof(ticksModule), 1, long.MaxValue);

			var round = DivideRoundToEvenNaive(dateTime.Ticks, ticksModule);
			return Create(dateTime, round * ticksModule);
		}

		[Pure]
		private static DateTime RoundCore(DateTime dateTime, long ticksModule, MidpointRounding mode)
		{
			return mode switch
			{
				MidpointRounding.ToEven => RoundToEvenCore(dateTime, ticksModule),
				MidpointRounding.AwayFromZero => RoundAwayFromZeroCore(dateTime, ticksModule),
#if TARGETS_NET || TARGETS_NETSTANDARD || LESSTHAN_NETCOREAPP30
				// Some MidpointRounding values are missing if targeting to these frameworks
#else
				MidpointRounding.ToZero => FloorCore(dateTime, ticksModule),
				MidpointRounding.ToNegativeInfinity => FloorCore(dateTime, ticksModule),
				MidpointRounding.ToPositiveInfinity => CeilingCore(dateTime, ticksModule),
#endif
				_ => throw CodeExceptions.UnexpectedArgumentValue(nameof(mode), mode)
			};
		}

		/// <summary>Returns the datetime rounded down to specified interval.</summary>
		[Pure]
		public static DateTime Truncate(this DateTime dateTime, TimeSpan interval) => FloorCore(dateTime, interval.Ticks);

		/// <summary>Returns the datetime rounded down to nearest second.</summary>
		[Pure]
		public static DateTime TruncateMilliseconds(this DateTime dateTime) => FloorCore(dateTime, TimeSpan.TicksPerSecond);

		/// <summary>Returns the datetime rounded up to specified interval.</summary>
		[Pure]
		public static DateTime Ceiling(this DateTime dateTime, TimeSpan interval) => CeilingCore(dateTime, interval.Ticks);

		/// <summary>Returns the datetime rounded up to nearest second.</summary>
		[Pure]
		public static DateTime CeilingMilliseconds(this DateTime dateTime) => CeilingCore(dateTime, TimeSpan.TicksPerSecond);

		/// <summary>Returns the datetime rounded by specified interval. Uses <see cref="MidpointRounding.ToEven"/> rounding.</summary>
		[Pure]
		public static DateTime Round(this DateTime dateTime, TimeSpan interval) =>
			RoundCore(dateTime, interval.Ticks, MidpointRounding.ToEven);

		/// <summary>Returns the datetime rounded by specified interval.</summary>
		[Pure]
		public static DateTime Round(this DateTime dateTime, TimeSpan interval, MidpointRounding mode) =>
			RoundCore(dateTime, interval.Ticks, mode);

		/// <summary>Returns the datetime rounded to nearest second. Uses <see cref="MidpointRounding.ToEven"/> rounding.</summary>
		[Pure]
		public static DateTime RoundMilliseconds(this DateTime dateTime) =>
			RoundCore(dateTime, TimeSpan.TicksPerSecond, MidpointRounding.ToEven);

		/// <summary>Returns the datetime rounded to nearest second.</summary>
		[Pure]
		public static DateTime RoundMilliseconds(this DateTime dateTime, MidpointRounding mode) =>
			RoundCore(dateTime, TimeSpan.TicksPerSecond, mode);
		#endregion
	}
}