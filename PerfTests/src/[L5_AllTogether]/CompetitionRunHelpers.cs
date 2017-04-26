using System.Threading;

using CodeJam.PerfTests.Configs;

namespace CodeJam.PerfTests
{
	/// <summary>
	/// Reusable API for code  that used during perftest run.
	/// </summary>
	public static class CompetitionRunHelpers
	{
		/// <summary>
		/// Empirically found constant loop count that provides accurate results
		/// for <see cref="WellKnownMetrics.RelativeTime"/> metric on different hardware.
		/// Equals to 16384;
		/// Best if used together with <see cref="ICompetitionFeatures.BurstMode"/>=<c>true</c> (<see cref="CompetitionBurstModeAttribute"/>).
		/// </summary>
		public const int BurstModeLoopCount = 16 * 1024;

		/// <summary>
		/// Empirically found short loop count that provides accurate results
		/// for <see cref="WellKnownMetrics.RelativeTime"/> metric on different hardware.
		/// Equals to 128;
		/// May provide inaccurate results if used together with <see cref="ICompetitionFeatures.BurstMode"/>=<c>true</c>.
		/// </summary>
		public const int SmallLoopCount = 128;

		/// <summary>Default delay implementation. Performs delay for specified number of cycles.</summary>
		/// <param name="cycles">The number of cycles to delay.</param>
		public static void Delay(int cycles) => Thread.SpinWait(cycles);
	}
}