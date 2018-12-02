using System;

using CodeJam.PerfTests.Metrics;

using JetBrains.Annotations;

namespace CodeJam.PerfTests.Analysers
{
	/// <summary>The competition metric value.</summary>
	public class CompetitionMetricValue
	{
		/// <summary>Initializes empty new instance of the <see cref="CompetitionMetricValue"/> class.</summary>
		/// <param name="metric">The metric information.</param>
		public CompetitionMetricValue([NotNull] MetricInfo metric)
		{
			Code.NotNull(metric, nameof(metric));

			Metric = metric;
			ValuesRange = MetricRange.Empty;
			DisplayMetricUnit = MetricUnit.Empty;
		}

		/// <summary>Initializes a new instance of the <see cref="CompetitionMetricValue"/> class.</summary>
		/// <param name="metric">The metric information.</param>
		/// <param name="valuesRange">The metric values range.</param>
		/// <param name="displayMetricUnit">The preferred metric unit for the values range.</param>
		public CompetitionMetricValue(
			[NotNull] MetricInfo metric,
			MetricRange valuesRange,
			[NotNull] MetricUnit displayMetricUnit)
		{
			Code.NotNull(metric, nameof(metric));
			Code.NotNull(displayMetricUnit, nameof(displayMetricUnit));

			Metric = metric;
			ValuesRange = valuesRange;
			DisplayMetricUnit = displayMetricUnit;
		}

		/// <summary>Gets the metric.</summary>
		/// <value>The metric.</value>
		[NotNull]
		public MetricInfo Metric { get; }

		/// <summary>Gets metric values range.</summary>
		/// <value>The metric values range.</value>
		public MetricRange ValuesRange { get; private set; }

		/// <summary>Gets preferred metric unit for the values range.</summary>
		/// <value>The preferred metric unit for the values range.</value>
		[NotNull]
		public MetricUnit DisplayMetricUnit { get; private set; }

		/// <summary>The metric value is updated but was not saved.</summary>
		/// <value><c>true</c> if the metric value has unsaved changes; otherwise, <c>false</c>.</value>
		public bool HasUnsavedChanges { get; private set; }

		/// <summary>Combines metric value with specified one.</summary>
		/// <param name="other">The metric value to merge.</param>
		/// <param name="forceMetricUnitUpdate">
		/// If set to <c>true</c> existing <see cref="DisplayMetricUnit"/> is updated even if it is not empty.
		/// </param>
		/// <returns><c>true</c> if was updated.</returns>
		public bool UnionWith([NotNull] CompetitionMetricValue other, bool forceMetricUnitUpdate)
		{
			if (other.Metric != Metric)
				throw CodeExceptions.Argument(
					nameof(other),
					$"Passed metric {other.Metric} does not match to this one {Metric}.");

			if (other.ValuesRange.IsEmpty)
				return false;

			var result = false;

			var newValues = ValuesRange.Union(other.ValuesRange);
			if (newValues != ValuesRange)
			{
				ValuesRange = newValues;
				HasUnsavedChanges = true;

				result = true;
			}

			if (DisplayMetricUnit.IsEmpty || forceMetricUnitUpdate)
			{
				var metricUnit = other.DisplayMetricUnit;
				if (metricUnit.IsEmpty)
				{
					metricUnit = Metric.MetricUnits[ValuesRange];
				}

				if (DisplayMetricUnit != metricUnit)
				{
					DisplayMetricUnit = metricUnit;
					HasUnsavedChanges = true;

					result = true;
				}
			}

			return result;
		}

		/// <summary>Marks value as saved (sets the <see cref="HasUnsavedChanges"/> to <c>false</c>).</summary>
		public void MarkAsSaved() => HasUnsavedChanges = false;

		/// <summary>Returns a <see cref="string"/> that represents this instance.</summary>
		/// <returns>A <see cref="string"/> that represents this instance.</returns>
		public override string ToString() => ValuesRange.ToString(DisplayMetricUnit);
	}
}