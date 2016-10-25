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
	}
}