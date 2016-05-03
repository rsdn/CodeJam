using System;

using BenchmarkDotNet.Attributes;

using JetBrains.Annotations;

// ReSharper disable CheckNamespace
// ReSharper disable once RedundantAttributeUsageProperty

namespace BenchmarkDotNet.UnitTesting
{
	/// <summary>
	/// Marks the baseline competition benchmark
	/// </summary>
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