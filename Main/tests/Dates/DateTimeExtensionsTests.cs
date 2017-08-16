using System;

using CodeJam.Ranges;

using NUnit.Framework;

using static NUnit.Framework.Assert;

namespace CodeJam.Dates
{
	// TODO: tests with lastDayOfMont / tests with DST
	public static class DateTimeExtensionsTests
	{
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
		public static void UseLastDayTests()
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
	}
}