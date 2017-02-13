using System;

using CodeJam.PerfTests.Configs.Factories;
using CodeJam.PerfTests.Metrics;

namespace CodeJam.PerfTests.Configs
{
	/// <summary>
	/// Adds the <see cref="CompetitionMetricInfo.AbsoluteTime"/> metric
	/// and the <see cref="CompetitionMetricInfo.GcAllocations"/> metric.
	/// </summary>
	/// <seealso cref="CodeJam.PerfTests.CompetitionModifierAttribute" />
	public sealed class CompetitionMeasureAllAttribute : CompetitionModifierAttribute
	{
		private class ModifierImpl : ICompetitionModifier
		{
			public void Modify(ManualCompetitionConfig competitionConfig)
			{
				competitionConfig.Metrics.Add(CompetitionMetricInfo.AbsoluteTime);
				competitionConfig.Metrics.Add(CompetitionMetricInfo.GcAllocations);
				competitionConfig.Metrics.Add(CompetitionMetricInfo.Gc0);
				competitionConfig.Metrics.Add(CompetitionMetricInfo.Gc1);
				competitionConfig.Metrics.Add(CompetitionMetricInfo.Gc2);
			}
		}

		/// <summary>Initializes a new instance of the <see cref="CompetitionMeasureAllAttribute" /> class.</summary>
		public CompetitionMeasureAllAttribute() : base(() => new ModifierImpl()) { }
	}

	/// <summary>
	/// Adds the <see cref="CompetitionMetricInfo.AbsoluteTime"/> metric.
	/// </summary>
	/// <seealso cref="CodeJam.PerfTests.CompetitionModifierAttribute" />
	public sealed class CompetitionNoRelativeTimeAttribute : CompetitionModifierAttribute
	{
		private class ModifierImpl : ICompetitionModifier
		{
			public void Modify(ManualCompetitionConfig competitionConfig) =>
				competitionConfig.Metrics.RemoveAll(m => m == CompetitionMetricInfo.RelativeTime);
		}

		/// <summary>Initializes a new instance of the <see cref="CompetitionNoRelativeTimeAttribute" /> class.</summary>
		public CompetitionNoRelativeTimeAttribute() : base(() => new ModifierImpl()) { }
	}

	/// <summary>
	/// Adds the <see cref="CompetitionMetricInfo.AbsoluteTime"/> metric.
	/// </summary>
	/// <seealso cref="CodeJam.PerfTests.CompetitionModifierAttribute" />
	public sealed class CompetitionNoGcModifierAttribute : CompetitionModifierAttribute
	{
		private class ModifierImpl : ICompetitionModifier
		{
			public void Modify(ManualCompetitionConfig competitionConfig) =>
				competitionConfig.Metrics.RemoveAll(m => m.Category == GcMetricValuesProvider.Category);
		}

		/// <summary>Initializes a new instance of the <see cref="CompetitionNoRelativeTimeAttribute" /> class.</summary>
		public CompetitionNoGcModifierAttribute() : base(() => new ModifierImpl()) { }
	}
}
