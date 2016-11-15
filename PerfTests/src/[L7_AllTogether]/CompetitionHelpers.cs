using System;
using System.Threading;

using JetBrains.Annotations;

namespace CodeJam.PerfTests
{
	/// <summary>
	/// Reusable API for performance tests.
	/// </summary>
	[PublicAPI]
	public static class CompetitionHelpers
	{
		#region Constants
		/// <summary>Default category prefix for performance tests.</summary>
		public const string PerfTestCategory = "Performance";

		/// <summary>Default explanation for bad performance tests.</summary>
		public const string TemporarilyExcludedReason =
			"Temporary disabled as the results are unstable. Please, run the test manually from the Test Explorer window.";
		#endregion

		#region Benchmark-related
		/// <summary>Default count for performance test loops.</summary>
		public const int DefaultCount = 10 * 1000;

		/// <summary>Default delay implementation. Performs delay for specified number of cycles.</summary>
		/// <param name="cycles">The number of cycles to delay.</param>
		public static void Delay(int cycles) => Thread.SpinWait(cycles);
		#endregion
	}
}