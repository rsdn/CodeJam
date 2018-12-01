using System;

using BenchmarkDotNet.Attributes;

using JetBrains.Annotations;

namespace CodeJam.PerfTests
{
	/// <summary>Attribute for baseline competition benchmark.</summary>
	/// <seealso cref="BenchmarkAttribute"/>
	// DONTTOUCH: DO NOT change Inherited = false as it will break annotation system
	// ReSharper disable once RedundantAttributeUsageProperty
	[AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
	[PublicAPI, MeansImplicitUse]
	public class CompetitionBaselineAttribute : CompetitionBenchmarkAttribute
	{
		/// <summary>Constructor for baseline competition benchmark attribute.</summary>
		public CompetitionBaselineAttribute() => Baseline = true;
	}
}