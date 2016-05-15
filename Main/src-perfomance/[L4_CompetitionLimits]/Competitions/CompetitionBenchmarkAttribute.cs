using System;

using BenchmarkDotNet.Attributes;

using JetBrains.Annotations;

namespace BenchmarkDotNet.Competitions
{
	/// <summary>
	/// Marks the competition benchmark
	/// </summary>
// ReSharper disable once RedundantAttributeUsageProperty
	[AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
	[PublicAPI, MeansImplicitUse]
	public class CompetitionBenchmarkAttribute : BenchmarkAttribute
	{
		/// <summary>
		/// Marks the competition benchmark
		/// </summary>
		public CompetitionBenchmarkAttribute() { }

		/// <summary>
		/// Marks the competition benchmark
		/// </summary>
		public CompetitionBenchmarkAttribute(double maxRatio)
		{
			MinRatio = -1;
			MaxRatio = maxRatio;
		}

		/// <summary>
		/// Marks the competition benchmark
		/// </summary>
		public CompetitionBenchmarkAttribute(double minRatio, double maxRatio)
		{
			MinRatio = minRatio;
			MaxRatio = maxRatio;
		}

		/// <summary>
		/// Exclude the benchmark from competition
		/// </summary>
		public bool DoesNotCompete { get; set; }

		/// <summary>
		/// Max timing ratio (relative to the baseline benchmark)
		/// </summary>
		public double MaxRatio { get; private set; }

		/// <summary>
		/// Min timing ratio (relative to the baseline benchmark)
		/// </summary>
		public double MinRatio { get; private set; }
	}
}