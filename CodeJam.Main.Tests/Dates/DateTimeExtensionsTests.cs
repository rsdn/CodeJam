using System;

using NUnit.Framework;

using static NUnit.Framework.Assert;

using Range = CodeJam.Ranges.Range;

namespace CodeJam.Dates
{
	// TODO: tests with lastDayOfMonth / tests with DST
	public static class DateTimeExtensionsTests
	{
		private static DateTime DateWithSameOddityDay(DateTime date, int day)
		{
			// Ensure that day is as even as value
			var dateDay = date.WithDay(day);
			var totalDays = (long)(dateDay - DateTime.MinValue).TotalDays;
			if (totalDays % 2 != day % 2)
				dateDay = date.WithDay(day + 1);

			return dateDay;
		}

		private static DateTimeOffset DateWithSameOddityDay(DateTimeOffset date, int day) =>
			DateWithSameOddityDay(date.Date, day);

		[Test]
		public static void TestDivideRoundToEvenNaive()
		{
			AreEqual(DateTimeExtensions.DivideRoundToEvenNaive(6, 2), 3);
			AreEqual(DateTimeExtensions.DivideRoundToEvenNaive(7, 3), 2);
			AreEqual(DateTimeExtensions.DivideRoundToEvenNaive(11, 3), 4);
			AreEqual(DateTimeExtensions.DivideRoundToEvenNaive(14, 3), 5);

			AreEqual(DateTimeExtensions.DivideRoundToEvenNaive(7, 2), 4);
			AreEqual(DateTimeExtensions.DivideRoundToEvenNaive(9, 2), 4);

			AreEqual(DateTimeExtensions.DivideRoundToEvenNaive(14000, 4000), 4);
			AreEqual(DateTimeExtensions.DivideRoundToEvenNaive(18000, 4000), 4);
		}

		[Test]
		public static void TestDivideRoundToEvenNaiveRandom()
		{
			var rnd = TestTools.GetTestRandom();
			for (int i = 0; i < 100; i++)
			{
				var a = (long)rnd.Next(1, int.MaxValue);
				var b = (long)rnd.Next(1, 10);
				var b2 = (long)rnd.Next((int)TimeSpan.TicksPerSecond, 200 * (int)TimeSpan.TicksPerSecond);

				var div1 = DateTimeExtensions.DivideRoundToEvenNaive(a, b);
				var div2 = (long)Math.Round(1m * a / b);
				AreEqual(div1, div2);

				div1 = DateTimeExtensions.DivideRoundToEvenNaive(a, b2);
				div2 = (long)Math.Round(1m * a / b2);
				AreEqual(div1, div2);
			}
		}

		[Test]
		public static void TestDateTimeExtensions()
		{
			var d = DateTime.Today.FirstDayOfMonth().AddHours(12);

			AreEqual(d.NextDay().PrevDay(), d);
			AreEqual(d.NextMonth().PrevMonth(), d);
			AreEqual(d.NextYear().PrevYear(), d);
			AreEqual(d.LastDayOfMonth().Day, d.DaysInMonth());
			AreEqual(d.WithMonth(1).WithDay(1), d.FirstDayOfYear().AddHours(12));
			AreEqual(d.WithYearAndMonth(2000, 1).WithDay(1), new DateTime(2000, 1, 1).AddHours(12));
			AreEqual(d.WithMonthAndDay(1, 1), d.FirstDayOfYear().AddHours(12));

			AreEqual(d.GetMonthRange().CountOfDays(), d.DaysInMonth());

			AreEqual(Range.Create(d, d).DifferenceInDays(), 0);
			AreEqual(Range.Create(d, d).CountOfDays(), 1);

			AreEqual(Range.Create(d, d).DifferenceInMonths(), 0);
			AreEqual(Range.Create(d, d).CountOfMonths(), 1);

			AreEqual(d.GetYearRange().CountOfMonths(), 12);
			AreEqual(d.GetYearRange().CountOfDays(), d.DaysInYear());
		}

		[Test]
		public static void TestDateTimeOffsetExtensions()
		{
			var d = DateTime.Today.FirstDayOfMonth().AddHours(12).ToOffset();

			AreEqual(d.NextDay().PrevDay(), d);
			AreEqual(d.NextMonth().PrevMonth(), d);
			AreEqual(d.NextYear().PrevYear(), d);
			AreEqual(d.LastDayOfMonth().Day, d.DaysInMonth());
			AreEqual(d.WithMonth(1).WithDay(1), d.FirstDayOfYear().AddHours(12));
			AreEqual(d.WithYearAndMonth(2000, 1).WithDay(1), new DateTime(2000, 1, 1).AddHours(12).ToOffset());
			AreEqual(d.WithMonthAndDay(1, 1), d.FirstDayOfYear().AddHours(12));

			AreEqual(d.GetMonthRange().CountOfDays(), d.DaysInMonth());

			AreEqual(Range.Create(d, d).DifferenceInDays(), 0);
			AreEqual(Range.Create(d, d).CountOfDays(), 1);

			AreEqual(Range.Create(d, d).DifferenceInMonths(), 0);
			AreEqual(Range.Create(d, d).CountOfMonths(), 1);

			AreEqual(d.GetYearRange().CountOfMonths(), 12);
			AreEqual(d.GetYearRange().CountOfDays(), d.DaysInYear());
		}

		[Test]
		public static void TestIsUtc()
		{
			var now = DateTimeOffset.Now.ToOffset(TimeSpan.FromHours(1));
			var utcNow = DateTimeOffset.UtcNow;
			IsFalse(now.IsUtc());
			IsTrue(utcNow.IsUtc());
			IsTrue(now.ToUniversalTime().IsUtc());
		}

		[Test]
		public static void TestTruncateTime()
		{
			var now = DateTimeOffset.Now.ToOffset(TimeSpan.FromHours(1));
			var utcNow = DateTimeOffset.UtcNow;
			AreEqual(now.TruncateTime(), now - now.TimeOfDay);
			AreEqual(utcNow.TruncateTime(), utcNow - utcNow.TimeOfDay);
		}

		[Test]
		public static void TestUseLastDayMode()
		{
			var d = new DateTime(2017, 2, 28);
			AreEqual(31, d.AddMonths(1, true).Day, "AddMonths");
			AreEqual(31, d.WithMonth(3, true).Day, "WithMonth");
			AreEqual(29, d.AddYears(3, true).Day, "AddYears");
			AreEqual(29, d.WithYear(2020, true).Day, "WithYear");
		}

		[TestCase(DateTimeKind.Utc)]
		[TestCase(DateTimeKind.Local)]
		[TestCase(DateTimeKind.Unspecified)]
		public static void PreserveOriginalDateTimeKind(DateTimeKind kind)
		{
			var d = DateTime.SpecifyKind(new DateTime(2017, 1, 15), kind);
			AreEqual(kind, d.FirstDayOfMonth().Kind, "#1");
			AreEqual(kind, d.LastDayOfMonth().Kind, "#2");
			AreEqual(kind, d.FirstDayOfYear().Kind, "#3");
			AreEqual(kind, d.LastDayOfYear().Kind, "#4");
		}

		[TestCase(0d)]
		[TestCase(3d)]
		[TestCase(-2d)]
		[TestCase(1.5d)]
		public static void PreserveOriginalOffset(double offset)
		{
			var timespan = TimeSpan.FromHours(offset);
			var d = new DateTimeOffset(2017, 1, 15, 0, 0, 0, timespan);

			AreEqual(timespan, d.FirstDayOfMonth().Offset, "#1");
			AreEqual(timespan, d.LastDayOfMonth().Offset, "#2");
			AreEqual(timespan, d.FirstDayOfYear().Offset, "#3");
			AreEqual(timespan, d.LastDayOfYear().Offset, "#4");
		}

		[Test]
		public static void TestTruncate()
		{
			var date = new DateTime(1, 2, 3, 4, 55, 6, DateTimeKind.Local);
			var dateOffset = date.ToOffset();

			AreEqual(date.AddMilliseconds(500).TruncateMilliseconds(), date);
			AreEqual(date.AddMilliseconds(499).TruncateMilliseconds(), date);
			AreEqual(date.AddMilliseconds(1600).TruncateMilliseconds(), date.AddSeconds(1));

			AreEqual(date.Truncate(TimeSpan.FromDays(2)), date.Date.AddDays(-1));
			AreEqual(date.Truncate(TimeSpan.FromHours(1)), date.Date.AddHours(4));
			AreEqual(date.Truncate(TimeSpan.FromMinutes(30)), date.Date.AddHours(4).AddMinutes(30));

			AreEqual(dateOffset.AddMilliseconds(500).TruncateMilliseconds(), dateOffset);
			AreEqual(dateOffset.AddMilliseconds(499).TruncateMilliseconds(), dateOffset);
			AreEqual(dateOffset.AddMilliseconds(1600).TruncateMilliseconds(), dateOffset.AddSeconds(1));

			AreEqual(dateOffset.Truncate(TimeSpan.FromDays(2)), dateOffset.TruncateTime().AddDays(-1));
			AreEqual(dateOffset.Truncate(TimeSpan.FromHours(1)), dateOffset.TruncateTime().AddHours(4));
			AreEqual(dateOffset.Truncate(TimeSpan.FromMinutes(30)), dateOffset.TruncateTime().AddHours(4).AddMinutes(30));
		}

		[Test]
		public static void TestCeiling()
		{
			var date = new DateTime(1, 2, 3, 4, 55, 6, DateTimeKind.Local);
			var dateOffset = date.ToOffset();

			AreEqual(date.AddMilliseconds(500).CeilingMilliseconds(), date.AddSeconds(1));
			AreEqual(date.AddMilliseconds(499).CeilingMilliseconds(), date.AddSeconds(1));
			AreEqual(date.AddMilliseconds(1600).CeilingMilliseconds(), date.AddSeconds(2));

			AreEqual(date.Ceiling(TimeSpan.FromDays(2)), date.Date.AddDays(1));
			AreEqual(date.Ceiling(TimeSpan.FromHours(1)), date.Date.AddHours(5));
			AreEqual(date.Ceiling(TimeSpan.FromMinutes(30)), date.Date.AddHours(5));

			AreEqual(dateOffset.AddMilliseconds(500).CeilingMilliseconds(), dateOffset.AddSeconds(1));
			AreEqual(dateOffset.AddMilliseconds(499).CeilingMilliseconds(), dateOffset.AddSeconds(1));
			AreEqual(dateOffset.AddMilliseconds(1600).CeilingMilliseconds(), dateOffset.AddSeconds(2));

			AreEqual(dateOffset.Ceiling(TimeSpan.FromDays(2)), dateOffset.TruncateTime().AddDays(1));
			AreEqual(dateOffset.Ceiling(TimeSpan.FromHours(1)), dateOffset.TruncateTime().AddHours(5));
			AreEqual(dateOffset.Ceiling(TimeSpan.FromMinutes(30)), dateOffset.TruncateTime().AddHours(5));
		}

		[Test]
		public static void TestRound()
		{
			var date = new DateTime(1, 2, 3, 4, 55, 6, DateTimeKind.Local);
			var dateOffset = date.ToOffset();

			AreEqual(date.AddMilliseconds(500).RoundMilliseconds(), date);
			AreEqual(date.AddMilliseconds(499).RoundMilliseconds(), date);
			AreEqual(date.AddMilliseconds(1600).RoundMilliseconds(), date.AddSeconds(2));

			AreEqual(date.Round(TimeSpan.FromDays(2)), date.Date.AddDays(1));
			AreEqual(date.Round(TimeSpan.FromHours(1)), date.Date.AddHours(5));
			AreEqual(date.Round(TimeSpan.FromMinutes(30)), date.Date.AddHours(5));

			AreEqual(dateOffset.AddMilliseconds(500).RoundMilliseconds(), dateOffset);
			AreEqual(dateOffset.AddMilliseconds(499).RoundMilliseconds(), dateOffset);
			AreEqual(dateOffset.AddMilliseconds(1600).RoundMilliseconds(), dateOffset.AddSeconds(2));

			AreEqual(dateOffset.Round(TimeSpan.FromDays(2)), dateOffset.TruncateTime().AddDays(1));
			AreEqual(dateOffset.Round(TimeSpan.FromHours(1)), dateOffset.TruncateTime().AddHours(5));
			AreEqual(dateOffset.Round(TimeSpan.FromMinutes(30)), dateOffset.TruncateTime().AddHours(5));
		}

		[TestCase(MidpointRounding.ToEven, 1)]
		[TestCase(MidpointRounding.ToEven, 2)]
		[TestCase(MidpointRounding.ToEven, 10)]
		[TestCase(MidpointRounding.ToEven, 5)]
		[TestCase(MidpointRounding.AwayFromZero, 1)]
		[TestCase(MidpointRounding.AwayFromZero, 2)]
		[TestCase(MidpointRounding.AwayFromZero, 10)]
		[TestCase(MidpointRounding.AwayFromZero, 5)]
#if NETCOREAPP30_OR_GREATER
		[TestCase(MidpointRounding.ToZero, 1)]
		[TestCase(MidpointRounding.ToZero, 2)]
		[TestCase(MidpointRounding.ToZero, 10)]
		[TestCase(MidpointRounding.ToZero, 5)]
		[TestCase(MidpointRounding.ToNegativeInfinity, 1)]
		[TestCase(MidpointRounding.ToNegativeInfinity, 2)]
		[TestCase(MidpointRounding.ToNegativeInfinity, 10)]
		[TestCase(MidpointRounding.ToNegativeInfinity, 5)]
		[TestCase(MidpointRounding.ToPositiveInfinity, 1)]
		[TestCase(MidpointRounding.ToPositiveInfinity, 2)]
		[TestCase(MidpointRounding.ToPositiveInfinity, 10)]
		[TestCase(MidpointRounding.ToPositiveInfinity, 5)]
#endif
		public static void TestRounding(MidpointRounding mode, int value)
		{
			var date = DateTime.Today.FirstDayOfMonth();
			var dateDay = DateWithSameOddityDay(date, value);
			var dateHours = date.AddHours(value);
			var dateMinutes = date.AddMinutes(value);
			var dateSeconds = date.AddSeconds(value);

			var minRoundOffset = (int)Math.Round(value + 0.1, mode) - value;
			var roundOffset = (int)Math.Round(value + 0.5, mode) - value;
			var maxRoundOffset = (int)Math.Round(value + 0.9, mode) - value;

			AreEqual(dateSeconds.AddMilliseconds(1).RoundMilliseconds(mode), dateSeconds.AddSeconds(minRoundOffset));
			AreEqual(dateSeconds.AddMilliseconds(500).RoundMilliseconds(mode), dateSeconds.AddSeconds(roundOffset));
			AreEqual(dateSeconds.AddMilliseconds(999).RoundMilliseconds(mode), dateSeconds.AddSeconds(maxRoundOffset));

			AreEqual(dateMinutes.AddSeconds(1).Round(TimeSpan.FromMinutes(1), mode), dateMinutes.AddMinutes(minRoundOffset));
			AreEqual(dateMinutes.AddSeconds(30).Round(TimeSpan.FromMinutes(1), mode), dateMinutes.AddMinutes(roundOffset));
			AreEqual(dateMinutes.AddSeconds(59).Round(TimeSpan.FromMinutes(1), mode), dateMinutes.AddMinutes(maxRoundOffset));

			AreEqual(dateHours.AddMinutes(1).Round(TimeSpan.FromHours(1), mode), dateHours.AddHours(minRoundOffset));
			AreEqual(dateHours.AddMinutes(30).Round(TimeSpan.FromHours(1), mode), dateHours.AddHours(roundOffset));
			AreEqual(dateHours.AddMinutes(59).Round(TimeSpan.FromHours(1), mode), dateHours.AddHours(maxRoundOffset));

			AreEqual(dateDay.AddHours(1).Round(TimeSpan.FromDays(1), mode), dateDay.AddDays(minRoundOffset));
			AreEqual(dateDay.AddHours(12).Round(TimeSpan.FromDays(1), mode), dateDay.AddDays(roundOffset));
			AreEqual(dateDay.AddHours(23).Round(TimeSpan.FromDays(1), mode), dateDay.AddDays(maxRoundOffset));
		}

		[TestCase(MidpointRounding.ToEven, 1)]
		[TestCase(MidpointRounding.ToEven, 2)]
		[TestCase(MidpointRounding.ToEven, 10)]
		[TestCase(MidpointRounding.ToEven, 5)]
		[TestCase(MidpointRounding.AwayFromZero, 1)]
		[TestCase(MidpointRounding.AwayFromZero, 2)]
		[TestCase(MidpointRounding.AwayFromZero, 10)]
		[TestCase(MidpointRounding.AwayFromZero, 5)]
#if NETCOREAPP30_OR_GREATER
		[TestCase(MidpointRounding.ToZero, 1)]
		[TestCase(MidpointRounding.ToZero, 2)]
		[TestCase(MidpointRounding.ToZero, 10)]
		[TestCase(MidpointRounding.ToZero, 5)]
		[TestCase(MidpointRounding.ToNegativeInfinity, 1)]
		[TestCase(MidpointRounding.ToNegativeInfinity, 2)]
		[TestCase(MidpointRounding.ToNegativeInfinity, 10)]
		[TestCase(MidpointRounding.ToNegativeInfinity, 5)]
		[TestCase(MidpointRounding.ToPositiveInfinity, 1)]
		[TestCase(MidpointRounding.ToPositiveInfinity, 2)]
		[TestCase(MidpointRounding.ToPositiveInfinity, 10)]
		[TestCase(MidpointRounding.ToPositiveInfinity, 5)]
#endif
		public static void TestRoundingOffset(MidpointRounding mode, int value)
		{
			var date = DateTime.Today.FirstDayOfMonth().ToOffset();
			var dateDay = DateWithSameOddityDay(date, value);
			var dateHours = date.AddHours(value);
			var dateMinutes = date.AddMinutes(value);
			var dateSeconds = date.AddSeconds(value);

			var minRoundOffset = (int)Math.Round(value + 0.1, mode) - value;
			var roundOffset = (int)Math.Round(value + 0.5, mode) - value;
			var maxRoundOffset = (int)Math.Round(value + 0.9, mode) - value;

			AreEqual(dateSeconds.AddMilliseconds(1).RoundMilliseconds(mode), dateSeconds.AddSeconds(minRoundOffset));
			AreEqual(dateSeconds.AddMilliseconds(500).RoundMilliseconds(mode), dateSeconds.AddSeconds(roundOffset));
			AreEqual(dateSeconds.AddMilliseconds(999).RoundMilliseconds(mode), dateSeconds.AddSeconds(maxRoundOffset));

			AreEqual(dateMinutes.AddSeconds(1).Round(TimeSpan.FromMinutes(1), mode), dateMinutes.AddMinutes(minRoundOffset));
			AreEqual(dateMinutes.AddSeconds(30).Round(TimeSpan.FromMinutes(1), mode), dateMinutes.AddMinutes(roundOffset));
			AreEqual(dateMinutes.AddSeconds(59).Round(TimeSpan.FromMinutes(1), mode), dateMinutes.AddMinutes(maxRoundOffset));

			AreEqual(dateHours.AddMinutes(1).Round(TimeSpan.FromHours(1), mode), dateHours.AddHours(minRoundOffset));
			AreEqual(dateHours.AddMinutes(30).Round(TimeSpan.FromHours(1), mode), dateHours.AddHours(roundOffset));
			AreEqual(dateHours.AddMinutes(59).Round(TimeSpan.FromHours(1), mode), dateHours.AddHours(maxRoundOffset));

			AreEqual(dateDay.AddHours(1).Round(TimeSpan.FromDays(1), mode), dateDay.AddDays(minRoundOffset));
			AreEqual(dateDay.AddHours(12).Round(TimeSpan.FromDays(1), mode), dateDay.AddDays(roundOffset));
			AreEqual(dateDay.AddHours(23).Round(TimeSpan.FromDays(1), mode), dateDay.AddDays(maxRoundOffset));
		}
	}
}