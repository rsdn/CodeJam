using System;

using CodeJam.PerfTests.Configs;
using CodeJam.PerfTests.Configs.Factories;

namespace CodeJam.PerfTests
{
	/// <summary>
	/// Enables tracing for ClrExceptions
	/// </summary>
	/// <seealso cref="CompetitionModifierAttribute" />
	public class TraceClrExceptionsModifierAttribute : CompetitionModifierAttribute
	{
		private class ModifierImpl : ICompetitionModifier
		{
			public void Modify(ManualCompetitionConfig competitionConfig) => competitionConfig.Metrics.Add(WellKnownEtwMetrics.ClrExceptions);
		}

		/// <summary>Initializes a new instance of the <see cref="TraceClrExceptionsModifierAttribute"/> class.</summary>
		public TraceClrExceptionsModifierAttribute() : base(() => new ModifierImpl()) { }
	}


	/// <summary>
	/// Enables tracing for File IO
	/// </summary>
	/// <seealso cref="CompetitionModifierAttribute" />
	public class UseFileIoMetricModifierAttribute : CompetitionModifierAttribute
	{
		private class ModifierImpl : ICompetitionModifier
		{
			public void Modify(ManualCompetitionConfig competitionConfig)
			{
				competitionConfig.Metrics.Add(
					WellKnownEtwMetrics.FileIoRead);
				competitionConfig.Metrics.Add(
					WellKnownEtwMetrics.FileIoWrite);
			}
		}

		/// <summary>Initializes a new instance of the <see cref="UseFileIoMetricModifierAttribute"/> class.</summary>
		public UseFileIoMetricModifierAttribute() : base(() => new ModifierImpl()) { }
	}
}