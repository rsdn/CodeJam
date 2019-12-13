using System;

namespace CodeJam.Dates
{
	/// <summary>
	/// Helper methods for timespan manipulations
	/// </summary>
	public static class TimeSpanHelpers
	{
		private const long _ticksPerMicrosecond = TimeSpan.TicksPerMillisecond / 1000;
		private const double _microsecondsPerTick = 1d / _ticksPerMicrosecond;
		private const long _maxMicroseconds = long.MaxValue / _ticksPerMicrosecond;
		private const long _minMicroseconds = long.MinValue / _ticksPerMicrosecond;

		private const double _ticksPerNanosecond = _ticksPerMicrosecond / 1000d;
		private const double _nanosecondsPerTick = 1d / _ticksPerNanosecond;
		private const double _maxNanoseconds = long.MaxValue / _ticksPerNanosecond;
		private const double _minNanoseconds = long.MinValue / _ticksPerNanosecond;

		private static TimeSpan FromTicksChecked(double ticks)
		{
			if (double.IsNaN(ticks))
				throw CodeExceptions.Argument(nameof(ticks), "TimeSpan does not accept floating point Not-a-Number values");
			if (ticks <= TimeSpan.MinValue.Ticks || ticks >= TimeSpan.MaxValue.Ticks)
				throw new OverflowException("TimeSpan overflowed because the duration is too long.");

			return new TimeSpan(checked((long)ticks));
		}

		/// <summary>
		/// Returns a TimeSpan that represents a specified number of microseconds.
		/// </summary>
		/// <param name="microseconds">A number of microseconds.</param>
		/// <returns>A TimeSpan that represents value.</returns>
		/// <exception cref="OverflowException">
		/// value is less than <see cref="TimeSpan.MinValue"/> or greater than <see cref="TimeSpan.MaxValue"/>.
		/// -or-
		/// value is<see cref="double.PositiveInfinity"/>.
		/// -or-
		/// value is <see cref="double.NegativeInfinity"/>.
		///</exception>
		/// <exception cref="ArgumentException">
		/// value is equal to <see cref="double.NaN"/>.
		///</exception>
		public static TimeSpan FromMicroseconds(double microseconds) =>
			FromTicksChecked(microseconds * _ticksPerMicrosecond);

		/// <summary>
		/// Returns a TimeSpan that represents a specified number of nanoseconds.
		/// </summary>
		/// <param name="nanoseconds">A number of nanoseconds.</param>
		/// <returns>A TimeSpan that represents value.</returns>
		/// <exception cref="OverflowException">
		/// value is less than <see cref="TimeSpan.MinValue"/> or greater than <see cref="TimeSpan.MaxValue"/>.
		/// -or-
		/// value is<see cref="double.PositiveInfinity"/>.
		/// -or-
		/// value is <see cref="double.NegativeInfinity"/>.
		///</exception>
		/// <exception cref="ArgumentException">
		/// value is equal to <see cref="double.NaN"/>.
		///</exception>
		public static TimeSpan FromNanoseconds(double nanoseconds) =>
			FromTicksChecked(nanoseconds * _ticksPerNanosecond);

		/// <summary>
		/// Returns a TimeSpan that represents value multiplied to specified multiplier.
		/// </summary>
		/// <param name="timeSpan">The time span.</param>
		/// <param name="multiplier">The multiplier.</param>
		/// <returns>A System.TimeSpan that represents value multiplied to specified multiplier.</returns>
		/// <exception cref="OverflowException">value is less than <see cref="TimeSpan.MinValue" /> or greater than <see cref="TimeSpan.MaxValue" />.
		/// -or-
		/// value is<see cref="double.PositiveInfinity" />.
		/// -or-
		/// value is <see cref="double.NegativeInfinity" />.</exception>
		/// <exception cref="ArgumentException">value is equal to <see cref="double.NaN" />.</exception>
		public static TimeSpan Multiply(this TimeSpan timeSpan, double multiplier) =>
			FromTicksChecked(timeSpan.Ticks * multiplier);

		/// <summary>
		/// Returns a TimeSpan that represents value divided to specified divisor.
		/// </summary>
		/// <param name="timeSpan">The time span.</param>
		/// <param name="divisor">The divisor.</param>
		/// <returns>A System.TimeSpan that represents value divided to specified divisor.</returns>
		/// <exception cref="OverflowException">value is less than <see cref="TimeSpan.MinValue" /> or greater than <see cref="TimeSpan.MaxValue" />.
		/// -or-
		/// value is<see cref="double.PositiveInfinity" />.
		/// -or-
		/// value is <see cref="double.NegativeInfinity" />.</exception>
		/// <exception cref="ArgumentException">value is equal to <see cref="double.NaN" />.</exception>
		public static TimeSpan Divide(this TimeSpan timeSpan, double divisor) =>
			FromTicksChecked(timeSpan.Ticks / divisor);

		/// <summary>
		/// Gets the value of the current TimeSpan structure expressed in whole
		/// and fractional microseconds.
		/// </summary>
		/// <param name="timeSpan">The time span.</param>
		/// <returns></returns>
		public static double TotalMicroseconds(this TimeSpan timeSpan)
		{
			var temp = timeSpan.Ticks * _microsecondsPerTick;

			if (temp > _maxMicroseconds)
				return _maxMicroseconds;

			if (temp < _minMicroseconds)
				return _minMicroseconds;

			return temp;
		}

		/// <summary>
		/// Gets the value of the current TimeSpan structure expressed in whole
		/// and fractional nanoseconds.
		/// </summary>
		/// <param name="timeSpan">The time span.</param>
		/// <returns></returns>
		public static double TotalNanoseconds(this TimeSpan timeSpan)
		{
			var temp = timeSpan.Ticks * _nanosecondsPerTick;

			if (temp > _maxNanoseconds)
				return _maxNanoseconds;

			if (temp < _minNanoseconds)
				return _minNanoseconds;

			return temp;
		}
	}
}