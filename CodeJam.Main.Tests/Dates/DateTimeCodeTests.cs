using System;
using System.Diagnostics.CodeAnalysis;

using NUnit.Framework;

namespace CodeJam.Dates
{
	[TestFixture(Category = "Assertions")]
	[SuppressMessage("ReSharper", "NotResolvedInText")]
	public class DateTimeCodeTests
	{
		[Test]
		public void TestDateOnlyAssertion()
		{
			var dateTimeNow = DateTime.Now;
			var dateTimeOffsetNow = DateTimeOffset.Now;

			var ex = Assert.Throws<ArgumentException>(() => DateTimeCode.DateOnly(dateTimeNow, "arg00"));
			Assert.That(ex.Message, Does.Contain("arg00"));
			Assert.That(ex.Message, Does.Contain("argument should be a date without time component"));

			ex = Assert.Throws<ArgumentException>(() => DateTimeCode.DateOnly(dateTimeOffsetNow, "arg00"));
			Assert.That(ex.Message, Does.Contain("arg00"));
			Assert.That(ex.Message, Does.Contain("argument should be a date without time component"));

			Assert.DoesNotThrow(() => DateTimeCode.DateOnly(dateTimeNow.Date, "arg00"));
			Assert.DoesNotThrow(() => DateTimeCode.DateOnly(dateTimeOffsetNow.Date, "arg00"));
		}

		[Test]
		public void TestDebugDateOnlyAssertion()
		{
			var dateTimeNow = DateTime.Now;
			var dateTimeOffsetNow = DateTimeOffset.Now;
#if DEBUG
			var ex = Assert.Throws<ArgumentException>(() => DebugDateTimeCode.DateOnly(dateTimeNow, "arg00"));
			Assert.That(ex.Message, Does.Contain("arg00"));
			Assert.That(ex.Message, Does.Contain("argument should be a date without time component"));

			ex = Assert.Throws<ArgumentException>(() => DebugDateTimeCode.DateOnly(dateTimeOffsetNow, "arg00"));
			Assert.That(ex.Message, Does.Contain("arg00"));
			Assert.That(ex.Message, Does.Contain("argument should be a date without time component"));
#else
			// ReSharper disable once InvocationIsSkipped
			Assert.DoesNotThrow(() => DebugDateTimeCode.DateOnly(dateTimeNow, "arg00"));
			Assert.DoesNotThrow(() => DebugDateTimeCode.DateOnly(dateTimeOffsetNow, "arg00"));
#endif

			// ReSharper disable once InvocationIsSkipped
			Assert.DoesNotThrow(() => DebugDateTimeCode.DateOnly(dateTimeNow.Date, "arg00"));
		}

		[Test]
		public void TestIsUtcAssertion()
		{
			var dateTimeNow = DateTime.Now;
			var dateTimeOffsetNow = DateTimeOffset.Now.ToOffset(TimeSpan.FromHours(1));

			var ex = Assert.Throws<ArgumentException>(() => DateTimeCode.IsUtc(dateTimeNow, "arg00"));
			Assert.That(ex.Message, Does.Contain("arg00"));
			Assert.That(ex.Message, Does.Contain("argument should be in UTC"));

			ex = Assert.Throws<ArgumentException>(() => DateTimeCode.IsUtc(dateTimeOffsetNow, "arg00"));
			Assert.That(ex.Message, Does.Contain("arg00"));
			Assert.That(ex.Message, Does.Contain("argument should be in UTC"));

			Assert.DoesNotThrow(() => DateTimeCode.IsUtc(dateTimeNow.ToUniversalTime(), "arg00"));
			Assert.DoesNotThrow(() => DateTimeCode.IsUtc(dateTimeOffsetNow.ToUniversalTime(), "arg00"));
		}

		[Test]
		public void TestIsUtcAndDateOnlyAssertion()
		{
			var dateTimeNow = DateTime.Now;
			var dateTimeOffsetNow = DateTimeOffset.Now.ToOffset(TimeSpan.FromHours(1));

			var ex = Assert.Throws<ArgumentException>(() => DateTimeCode.IsUtcAndDateOnly(dateTimeNow, "arg00"));
			Assert.That(ex.Message, Does.Contain("arg00"));
			Assert.That(ex.Message, Does.Contain("argument should be in UTC"));

			ex = Assert.Throws<ArgumentException>(() => DateTimeCode.IsUtcAndDateOnly(dateTimeOffsetNow, "arg00"));
			Assert.That(ex.Message, Does.Contain("arg00"));
			Assert.That(ex.Message, Does.Contain("argument should be in UTC"));

			ex = Assert.Throws<ArgumentException>(() => DateTimeCode.IsUtcAndDateOnly(dateTimeNow.ToUniversalTime(), "arg00"));
			Assert.That(ex.Message, Does.Contain("arg00"));
			Assert.That(ex.Message, Does.Contain("argument should be a date without time component"));

			ex =
				Assert.Throws<ArgumentException>(() => DateTimeCode.IsUtcAndDateOnly(dateTimeOffsetNow.ToUniversalTime(), "arg00"));
			Assert.That(ex.Message, Does.Contain("arg00"));
			Assert.That(ex.Message, Does.Contain("argument should be a date without time component"));

			Assert.DoesNotThrow(() => DateTimeCode.IsUtcAndDateOnly(dateTimeNow.ToUniversalTime().Date, "arg00"));
			Assert.DoesNotThrow(() => DateTimeCode.IsUtcAndDateOnly(dateTimeOffsetNow.ToUniversalTime().TruncateTime(), "arg00"));
		}

		[Test]
		public void TestFirstDayOfMonth()
		{
			var dateTimeNow = DateTime.Now;
			var dateTimeOffsetNow = DateTimeOffset.Now;

			var ex = Assert.Throws<ArgumentException>(() => DateTimeCode.FirstDayOfMonth(dateTimeNow, "arg00"));
			Assert.That(ex.Message, Does.Contain("arg00"));
			Assert.That(ex.Message, Does.Contain("argument should be a first day of month"));

			ex = Assert.Throws<ArgumentException>(() => DateTimeCode.FirstDayOfMonth(dateTimeOffsetNow, "arg00"));
			Assert.That(ex.Message, Does.Contain("arg00"));
			Assert.That(ex.Message, Does.Contain("argument should be a first day of month"));

			Assert.DoesNotThrow(() => DateTimeCode.DateOnly(dateTimeNow.Date.FirstDayOfMonth(), "arg00"));
			Assert.DoesNotThrow(() => DateTimeCode.DateOnly(dateTimeOffsetNow.Date.FirstDayOfMonth(), "arg00"));
		}

		[Test]
		public void TestFirstDayOfYear()
		{
			var dateTimeNow = DateTime.Now;
			var dateTimeOffsetNow = DateTimeOffset.Now;

			var ex = Assert.Throws<ArgumentException>(() => DateTimeCode.FirstDayOfYear(dateTimeNow, "arg00"));
			Assert.That(ex.Message, Does.Contain("arg00"));
			Assert.That(ex.Message, Does.Contain("argument should be a first day of year"));

			ex = Assert.Throws<ArgumentException>(() => DateTimeCode.FirstDayOfYear(dateTimeOffsetNow, "arg00"));
			Assert.That(ex.Message, Does.Contain("arg00"));
			Assert.That(ex.Message, Does.Contain("argument should be a first day of year"));

			Assert.DoesNotThrow(() => DateTimeCode.DateOnly(dateTimeNow.Date.FirstDayOfYear(), "arg00"));
			Assert.DoesNotThrow(() => DateTimeCode.DateOnly(dateTimeOffsetNow.Date.FirstDayOfYear(), "arg00"));
		}

		[Test]
		public void TestLastDayOfMonth()
		{
			var dateTimeNow = DateTime.Now;
			var dateTimeOffsetNow = DateTimeOffset.Now;

			var ex = Assert.Throws<ArgumentException>(() => DateTimeCode.LastDayOfMonth(dateTimeNow, "arg00"));
			Assert.That(ex.Message, Does.Contain("arg00"));
			Assert.That(ex.Message, Does.Contain("argument should be a last day of month"));

			ex = Assert.Throws<ArgumentException>(() => DateTimeCode.LastDayOfMonth(dateTimeOffsetNow, "arg00"));
			Assert.That(ex.Message, Does.Contain("arg00"));
			Assert.That(ex.Message, Does.Contain("argument should be a last day of month"));

			Assert.DoesNotThrow(() => DateTimeCode.DateOnly(dateTimeNow.Date.LastDayOfMonth(), "arg00"));
			Assert.DoesNotThrow(() => DateTimeCode.DateOnly(dateTimeOffsetNow.Date.LastDayOfMonth(), "arg00"));
		}

		[Test]
		public void TestLastDayOfYear()
		{
			var dateTimeNow = DateTime.Now;
			var dateTimeOffsetNow = DateTimeOffset.Now;

			var ex = Assert.Throws<ArgumentException>(() => DateTimeCode.LastDayOfYear(dateTimeNow, "arg00"));
			Assert.That(ex.Message, Does.Contain("arg00"));
			Assert.That(ex.Message, Does.Contain("argument should be a last day of year"));

			ex = Assert.Throws<ArgumentException>(() => DateTimeCode.LastDayOfYear(dateTimeOffsetNow, "arg00"));
			Assert.That(ex.Message, Does.Contain("arg00"));
			Assert.That(ex.Message, Does.Contain("argument should be a last day of year"));

			Assert.DoesNotThrow(() => DateTimeCode.DateOnly(dateTimeNow.Date.LastDayOfYear(), "arg00"));
			Assert.DoesNotThrow(() => DateTimeCode.DateOnly(dateTimeOffsetNow.Date.LastDayOfYear(), "arg00"));
		}
	}
}