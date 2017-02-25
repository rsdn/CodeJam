using System;
using System.Diagnostics.CodeAnalysis;

using CodeJam.PerfTests.Analysers;
using CodeJam.PerfTests.Metrics;
using NUnit.Framework;

namespace CodeJam.PerfTests
{
	[TestFixture(Category = "BenchmarkDotNet")]
	[SuppressMessage("ReSharper", "HeapView.BoxingAllocation")]
	public static class CompetitionTargetTests
	{/*
		[Test]
		public static void TestCompetitionMetricUnion()
		{
			var metric = CompetitionMetricInfo.AbsoluteTime;
			var msUnit = metric.MetricUnits[TimeUnit.Millisecond];
			var secUnit = metric.MetricUnits[TimeUnit.Second];

			var result = new CompetitionMetricValue(metric, MetricRange.Empty, msUnit);
			Assert.AreEqual(result.ToString(), "∅");
			Assert.AreEqual(result.ValuesRange.Min, double.NaN);
			Assert.AreEqual(result.ValuesRange.Max, double.NaN);
			Assert.IsTrue(result.ValuesRange.IsEmpty);
			Assert.IsFalse(result.HasUnsavedChanges);

			result.UnionWith(
				new CompetitionMetricValue(metric), true);
			Assert.IsTrue(result.ValuesRange.IsEmpty);
			Assert.IsFalse(result.HasUnsavedChanges);

			result.UnionWith(
				new CompetitionMetricValue(
					metric,
					MetricRange.Create(1 * (int)TimeUnit.Millisecond, 2 * (int)TimeUnit.Millisecond),
					secUnit),
				true);
			Assert.AreEqual(result.ToString(), "[0.00100..0.00200] sec");
			Assert.AreEqual(result.ValuesRange.Min, 1 * 1000 * 1000);
			Assert.AreEqual(result.ValuesRange.Max, 2 * 1000 * 1000);
			Assert.IsFalse(result.ValuesRange.IsEmpty);
			Assert.IsTrue(result.HasUnsavedChanges);

			result.UnionWith(new CompetitionMetricValue(metric), true);
			Assert.AreEqual(result.ToString(), "[0.00100..0.00200] sec");
			Assert.AreEqual(result.ValuesRange.Min, 1 * 1000 * 1000);
			Assert.AreEqual(result.ValuesRange.Max, 2 * 1000 * 1000);
			Assert.IsFalse(result.ValuesRange.IsEmpty);
			Assert.IsTrue(result.HasUnsavedChanges);


			result.UnionWith(
				new CompetitionMetricValue(metric, MetricRange.Create(0.0005, null).ToNormalizedMetricValues(secUnit), msUnit),
				true);
			Assert.AreEqual(result.ToString(), "[0.50..+∞) ms");
			Assert.AreEqual(result.ValuesRange.Min, 0.5 * 1000 * 1000);
			Assert.AreEqual(result.ValuesRange.Max, double.PositiveInfinity);
			Assert.IsFalse(result.ValuesRange.IsEmpty);
			Assert.IsTrue(result.HasUnsavedChanges);

			result.UnionWith(
				new CompetitionMetricValue(metric, MetricRange.Create(null, 1), MetricUnit.Empty),
				true);
			Assert.AreEqual(result.ToString(), "(-∞..+∞) sec");
			Assert.AreEqual(result.ValuesRange.Min, double.NegativeInfinity);
			Assert.AreEqual(result.ValuesRange.Max, double.PositiveInfinity);
			Assert.IsFalse(result.ValuesRange.IsEmpty);
			Assert.IsTrue(result.HasUnsavedChanges);

			result.MarkAsSaved();
			Assert.IsFalse(result.ValuesRange.IsEmpty);
			Assert.IsFalse(result.HasUnsavedChanges);
		}
	*/}
}