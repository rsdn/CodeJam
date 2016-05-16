using System;

using BenchmarkDotNet.Attributes;

using JetBrains.Annotations;

namespace BenchmarkDotNet.Competitions
{
	/// <summary>
	/// Marks the baseline competition benchmark
	/// </summary>
	// ReSharper disable once RedundantAttributeUsageProperty
	[AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
	[PublicAPI, MeansImplicitUse]
	public class CompetitionBaselineAttribute : BenchmarkAttribute
	{
		/// <summary>
		/// Marks the baseline competition benchmark
		/// </summary>
		public CompetitionBaselineAttribute()
		{
			Baseline = true;
		}
	}
}