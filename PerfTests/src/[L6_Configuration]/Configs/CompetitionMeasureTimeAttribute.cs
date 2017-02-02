using System;

using CodeJam.PerfTests.Configs.Factories;
using CodeJam.PerfTests.Metrics;

namespace CodeJam.PerfTests.Configs
{
	/// <summary>
	/// Adds the <see cref="CompetitionMetricInfo.AbsoluteTime"/> metric.
	/// </summary>
	/// <seealso cref="CodeJam.PerfTests.CompetitionModifierAttribute" />
	public sealed class CompetitionMeasureTimeAttribute : CompetitionModifierAttribute
	{
		private class ModifierImpl : ICompetitionModifier
		{
			public void Modify(ManualCompetitionConfig competitionConfig)
			{
				if (!competitionConfig.Metrics.Contains(CompetitionMetricInfo.AbsoluteTime))
					competitionConfig.Metrics.Add(CompetitionMetricInfo.AbsoluteTime);
			}
		}

		/// <summary>Initializes a new instance of the <see cref="CompetitionMeasureTimeAttribute" /> class.</summary>
		public CompetitionMeasureTimeAttribute() : base(() => new ModifierImpl()) { }
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

		/// <summary>Initializes a new instance of the <see cref="CompetitionMeasureTimeAttribute" /> class.</summary>
		public CompetitionNoRelativeTimeAttribute() : base(() => new ModifierImpl()) { }
	}
}
