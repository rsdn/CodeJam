using System;

using BenchmarkDotNet.Running;

using CodeJam.PerfTests.Metrics;
using CodeJam.PerfTests.Running.Core;
using CodeJam.PerfTests.Running.Limits;
using CodeJam.PerfTests.Running.Messages;
using CodeJam.Reflection;
using CodeJam.Strings;

using JetBrains.Annotations;

namespace CodeJam.PerfTests.Running.SourceAnnotations
{
	/// <summary>
	/// Helper class for attribute annotations
	/// </summary>
	internal static class AttributeAnnotations
	{
		/// <summary>The primary metric.</summary>
		public static readonly CompetitionMetricInfo PrimaryMetric = CompetitionMetricInfo.RelativeTime;

		/// <summary>
		/// Returns the name of target resource if defined in <see cref="CompetitionMetadataAttribute"/>.
		/// If the target type is nested all container types are checked too.
		/// </summary>
		/// <param name="targetType">Type of the benchmark to get resource name for.</param>
		/// <returns>
		/// Name of the resource containing xml document with competition limits
		/// or <c>null</c> if the target (or any container type) is not annotated with <see cref="CompetitionMetadataAttribute"/>
		/// </returns>
		[CanBeNull]
		public static CompetitionMetadata TryGetCompetitionMetadata([NotNull] Type targetType)
		{
			Code.NotNull(targetType, nameof(targetType));

			var attribute = targetType.TryGetMetadataAttribute<CompetitionMetadataAttribute>();

			if (attribute == null)
				return null;

			var resourceKey = new ResourceKey(
				targetType.Assembly,
				attribute.MetadataResourceName);
			return new CompetitionMetadata(
				resourceKey,
				attribute.MetadataResourcePath,
				attribute.UseFullTypeName);
		}

		/// <summary>Parses and checks the measurement unit value.</summary>
		/// <param name="target">The target.</param>
		/// <param name="unitValue">The unit value.</param>
		/// <param name="metricInfo">The metric information.</param>
		/// <param name="competitionState">State of the run.</param>
		public static MetricUnit ParseUnitValue(
			Target target,
			string unitValue,
			CompetitionMetricInfo metricInfo,
			CompetitionState competitionState)
		{
			var result = MetricUnit.Empty;
			if (metricInfo.MetricUnits.IsEmpty)
			{
				if (unitValue.NotNullNorEmpty())
				{
					competitionState.WriteMessage(
						MessageSource.Analyser, MessageSeverity.SetupError,
						$"XML annotation for {target.MethodDisplayInfo}, metric {metricInfo}. Unit value should be null as metric has empty units scale.");
				}
			}
			else
			{
				result = metricInfo.MetricUnits[unitValue];

				if (unitValue != null && result.IsEmpty)
				{
					competitionState.WriteMessage(
						MessageSource.Analyser, MessageSeverity.SetupError,
						$"XML annotation for {target.MethodDisplayInfo}, metric {metricInfo}: unknown unit value {unitValue}.");
				}
			}
			return result;
		}

		/// <summary>Parses and checks the measurement unit value.</summary>
		/// <param name="target">The target.</param>
		/// <param name="unitValue">The unit value.</param>
		/// <param name="metricInfo">The metric information.</param>
		/// <param name="competitionState">State of the run.</param>
		public static MetricUnit ParseUnitValue(
			Target target,
			Enum unitValue,
			CompetitionMetricInfo metricInfo,
			CompetitionState competitionState)
		{
			var result = MetricUnit.Empty;
			if (metricInfo.MetricUnits.IsEmpty)
			{
				if (unitValue != null)
				{
					competitionState.WriteMessage(
						MessageSource.Analyser, MessageSeverity.SetupError,
						$"Annotation for {target.MethodDisplayInfo}, metric {metricInfo}. Unit value should be null as metric has empty units scale.");
				}
			}
			else
			{
				result = metricInfo.MetricUnits[unitValue];

				if (unitValue != null && result.IsEmpty)
				{
					competitionState.WriteMessage(
						MessageSource.Analyser, MessageSeverity.SetupError,
						$"Annotation for {target.MethodDisplayInfo}, metric {metricInfo}: unknown unit value {unitValue}.");
				}
			}
			return result;
		}
	}
}