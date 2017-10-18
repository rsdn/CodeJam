using System;

using CodeJam.PerfTests.Configs.Factories;
using CodeJam.PerfTests.Metrics;

namespace CodeJam.PerfTests.Configs
{
	/// <summary>
	/// Adds the <see cref="WellKnownMetrics.ExpectedTime"/> metric
	/// and GC metrics.
	/// </summary>
	/// <seealso cref="CodeJam.PerfTests.CompetitionModifierAttribute"/>
	public sealed class CompetitionMeasureAllAttribute : CompetitionModifierAttribute
	{
		private class ModifierImpl : ICompetitionModifier
		{
			public void Modify(ManualCompetitionConfig competitionConfig)
			{
				competitionConfig.Metrics.Add(WellKnownMetrics.GcAllocations);
				competitionConfig.Metrics.Add(WellKnownMetrics.Gc0);
				competitionConfig.Metrics.Add(WellKnownMetrics.Gc1);
				competitionConfig.Metrics.Add(WellKnownMetrics.Gc2);
				competitionConfig.Metrics.Add(WellKnownMetrics.ExpectedTime);
			}
		}

		/// <summary>Initializes a new instance of the <see cref="CompetitionMeasureAllAttribute"/> class.</summary>
		public CompetitionMeasureAllAttribute() : base(() => new ModifierImpl()) { }
	}

	/// <summary>
	/// Adds the <see cref="WellKnownMetrics.ExpectedTime"/> metric
	/// </summary>
	/// <seealso cref="CodeJam.PerfTests.CompetitionModifierAttribute"/>
	public sealed class CompetitionMeasureTimeAttribute : CompetitionModifierAttribute
	{
		private class ModifierImpl : ICompetitionModifier
		{
			public void Modify(ManualCompetitionConfig competitionConfig)
			{
				competitionConfig.Metrics.Add(WellKnownMetrics.ExpectedTime);
			}
		}

		/// <summary>Initializes a new instance of the <see cref="CompetitionMeasureTimeAttribute"/> class.</summary>
		public CompetitionMeasureTimeAttribute() : base(() => new ModifierImpl()) { }
	}

	/// <summary>
	/// Removes the <see cref="WellKnownMetrics.RelativeTime"/> metric.
	/// </summary>
	/// <seealso cref="CodeJam.PerfTests.CompetitionModifierAttribute"/>
	public sealed class CompetitionNoRelativeTimeAttribute : CompetitionModifierAttribute
	{
		private class ModifierImpl : ICompetitionModifier
		{
			public void Modify(ManualCompetitionConfig competitionConfig) =>
				competitionConfig.Metrics.RemoveAll(m => m == WellKnownMetrics.RelativeTime);
		}

		/// <summary>Initializes a new instance of the <see cref="CompetitionNoRelativeTimeAttribute"/> class.</summary>
		public CompetitionNoRelativeTimeAttribute() : base(() => new ModifierImpl()) { }
	}

	/// <summary>
	/// Adds the <see cref="WellKnownMetrics.GcAllocations"/> metric.
	/// </summary>
	/// <seealso cref="CodeJam.PerfTests.CompetitionModifierAttribute"/>
	public sealed class CompetitionMeasureAllocationsAttribute : CompetitionModifierAttribute
	{
		private class ModifierImpl : ICompetitionModifier
		{
			public void Modify(ManualCompetitionConfig competitionConfig) =>
				competitionConfig.Metrics.Add(WellKnownMetrics.GcAllocations);
		}

		/// <summary>Initializes a new instance of the <see cref="CompetitionNoRelativeTimeAttribute"/> class.</summary>
		public CompetitionMeasureAllocationsAttribute() : base(() => new ModifierImpl()) { }
	}

	/// <summary>
	/// Removes GC metrics (all with category equal to <see cref="GcMetricValuesProvider.Category"/>).
	/// </summary>
	/// <seealso cref="CodeJam.PerfTests.CompetitionModifierAttribute"/>
	public sealed class CompetitionIgnoreAllocationsAttribute : CompetitionModifierAttribute
	{
		private class ModifierImpl : ICompetitionModifier
		{
			public void Modify(ManualCompetitionConfig competitionConfig) =>
				competitionConfig.Metrics.RemoveAll(m => m.Category == GcMetricValuesProvider.Category);
		}

		/// <summary>Initializes a new instance of the <see cref="CompetitionNoRelativeTimeAttribute"/> class.</summary>
		public CompetitionIgnoreAllocationsAttribute() : base(() => new ModifierImpl()) { }
	}
}