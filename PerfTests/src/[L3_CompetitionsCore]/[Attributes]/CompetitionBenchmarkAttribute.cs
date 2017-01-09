using System;

using BenchmarkDotNet.Attributes;

using JetBrains.Annotations;

namespace CodeJam.PerfTests
{
	/// <summary>Attribute for competition benchmark.</summary>
	/// <seealso cref="BenchmarkAttribute"/>
	// DONTTOUCH: DO NOT change Inherited = false as it will break annotation system
	// ReSharper disable once RedundantAttributeUsageProperty
	[AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
	[PublicAPI, MeansImplicitUse]
	public class CompetitionBenchmarkAttribute : BenchmarkAttribute
	{
		/// <summary>Constructor for competition benchmark attribute.</summary>
		public CompetitionBenchmarkAttribute() { }

		/// <summary>Marks the competition benchmark.</summary>
		/// <param name="maxRatio">
		/// The maximum timing ratio relative to the baseline.
		/// Use <c>0</c> (used by default)
		/// to mark the limit as unset but updateable during the annotation.
		/// Use negative value to ignore the limit.
		/// </param>
		public CompetitionBenchmarkAttribute(double maxRatio)
		{
			MinRatio = -1;
			MaxRatio = maxRatio;
		}

		/// <summary>Marks the competition benchmark.</summary>
		/// <param name="minRatio">
		/// The minimum timing ratio relative to the baseline.
		/// Use <c>0</c> (used by default)
		/// to mark the limit as unset but updateable during the annotation.
		/// Use negative value to ignore the limit.
		/// </param>
		/// <param name="maxRatio">
		/// The maximum timing ratio relative to the baseline.
		/// Use <c>0</c> (used by default)
		/// to mark the limit as unset but updateable during the annotation.
		/// Use negative value to ignore the limit.
		/// </param>
		public CompetitionBenchmarkAttribute(double minRatio, double maxRatio)
		{
			MinRatio = minRatio;
			MaxRatio = maxRatio;
		}

		/// <summary>Exclude the benchmark from competition.</summary>
		/// <value>
		/// <c>true</c> if the benchmark does not take part in competition
		/// and should not be validated.
		/// </value>
		public bool DoesNotCompete { get; set; }

		/// <summary>
		/// The minimum timing ratio relative to the baseline.
		/// Set to <c>0</c> (used by default)
		/// to mark the limit as unset but updateable during the annotation.
		/// Set to negative value to ignore the limit.
		/// </summary>
		/// <value>The minimum timing ratio relative to the baseline.</value>
		public double MinRatio { get; private set; }

		/// <summary>
		/// The maximum timing ratio relative to the baseline.
		/// Set to <c>0</c> (used by default)
		/// to mark the limit as unset but updateable during the annotation.
		/// Set to negative value to ignore the limit.
		/// </summary>
		/// <value>The maximum timing ratio relative to the baseline.</value>
		public double MaxRatio { get; private set; }
	}
}