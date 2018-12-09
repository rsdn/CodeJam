
using JetBrains.Annotations;

// ReSharper disable once CheckNamespace

namespace BenchmarkDotNet.Horology
{
	/// <summary>
	/// <see cref="IClock"/> implementation over QueryThreadCycleTime().
	/// WARNING: results are inaccurate (up to +/- 30% to actual time),
	/// see https://blogs.msdn.microsoft.com/oldnewthing/20160429-00/?p=93385 for more.
	/// </summary>
	/// <seealso cref="IClock"/>
	[PublicAPI]
	public sealed class ThreadCycleTimeClock : IClock
	{
		#region Static members
		private static readonly bool _isAvailable;
		private static readonly long _frequency;

		static ThreadCycleTimeClock() =>
			_isAvailable = CycleClockHelpers.EstimateThreadCycleTimeFrequency(
				Chronometer.BestClock,
				1000 * 1000, out _frequency);

		/// <summary>Default instance of <see cref="ThreadCycleTimeClock"/>.</summary>
		public static readonly IClock Instance = new ThreadCycleTimeClock();
		#endregion

		/// <summary>Gets the title.</summary>
		/// <value>The title.</value>
		public string Title => "CPU cycles (Thread)";

		/// <summary>Gets a value indicating whether this instance is available.</summary>
		/// <value>
		/// <c>true</c> if this instance is available; otherwise, <c>false</c>.
		/// </value>
		public bool IsAvailable => _isAvailable;

		/// <summary>Gets the frequency.</summary>
		/// <value>The frequency.</value>
		public Frequency Frequency => new Frequency(_frequency);

		/// <summary>Gets the timestamp.</summary>
		/// <returns>Timestamp</returns>
		public long GetTimestamp() => CycleClockHelpers.GetCurrentThreadTimestamp();

		/// <summary>
		/// Returns a <see cref="string"/> that represents this instance.
		/// </summary>
		/// <returns>
		/// A <see cref="string"/> that represents this instance.
		/// </returns>
		public override string ToString() => GetType().Name;
	}
}