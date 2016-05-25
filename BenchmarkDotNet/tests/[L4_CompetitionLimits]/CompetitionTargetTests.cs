using System;
using System.Diagnostics.CodeAnalysis;

using BenchmarkDotNet.Competitions;
using BenchmarkDotNet.Running.Competitions.SourceAnnotations;

using NUnit.Framework;

namespace CodeJam.BenchmarkDotNet
{
	[TestFixture(Category = "BenchmarkDotNet")]
	[SuppressMessage("ReSharper", "HeapView.BoxingAllocation")]
	public static class CompetitionTargetTests
	{
		[Test]
		public static void TestCompetitionLimitsEmpty()
		{
			var result = new CompetitionTarget();

			Assert.IsTrue(result.IsEmpty);
			Assert.IsFalse(result.HasUnsavedChanges);

			result.UnionWith(CompetitionLimit.NoLimit);
			Assert.IsTrue(result.IsEmpty);
			Assert.IsFalse(result.HasUnsavedChanges);

			result.UnionWith(CompetitionLimit.Empty);
			Assert.IsTrue(result.IsEmpty);
			Assert.IsFalse(result.HasUnsavedChanges);

			result.LooseLimitsAndMarkAsSaved(0);
			Assert.IsTrue(result.IsEmpty);
			Assert.IsFalse(result.HasUnsavedChanges);

			result.LooseLimitsAndMarkAsSaved(99);
			Assert.IsTrue(result.IsEmpty);
			Assert.IsFalse(result.HasUnsavedChanges);

			Assert.Throws<ArgumentOutOfRangeException>(() => result.LooseLimitsAndMarkAsSaved(-1));
			Assert.Throws<ArgumentOutOfRangeException>(() => result.LooseLimitsAndMarkAsSaved(100));

			result.UnionWith(new CompetitionLimit(1, 2));
			Assert.AreEqual(result.Min, 1);
			Assert.AreEqual(result.Max, 2);
			Assert.IsFalse(result.IsEmpty);
			Assert.IsTrue(result.HasUnsavedChanges);
			Assert.IsTrue(result.IsChanged(CompetitionTargetProperties.MinRatio));
			Assert.IsTrue(result.IsChanged(CompetitionTargetProperties.MaxRatio));

			result.LooseLimitsAndMarkAsSaved(5);
			Assert.AreEqual(result.Min, 0.95); // -5%
			Assert.AreEqual(result.Max, 2.1);  // + 5%
			Assert.IsFalse(result.HasUnsavedChanges);
		}

		[Test]
		public static void TestCompetitionLimitsNoLimit()
		{
			var result = new CompetitionTarget(null, CompetitionLimit.NoLimit, false);
			Assert.IsFalse(result.IsEmpty);
			Assert.IsFalse(result.HasUnsavedChanges);

			result.UnionWith(CompetitionLimit.NoLimit);
			Assert.IsFalse(result.HasUnsavedChanges);

			result.UnionWith(CompetitionLimit.Empty);
			Assert.IsFalse(result.HasUnsavedChanges);

			result.LooseLimitsAndMarkAsSaved(0);
			Assert.IsFalse(result.HasUnsavedChanges);

			result.LooseLimitsAndMarkAsSaved(99);
			Assert.IsFalse(result.HasUnsavedChanges);

			Assert.Throws<ArgumentOutOfRangeException>(() => result.LooseLimitsAndMarkAsSaved(-1));
			Assert.Throws<ArgumentOutOfRangeException>(() => result.LooseLimitsAndMarkAsSaved(100));

			result.UnionWith(new CompetitionLimit(1, 2));
			Assert.IsFalse(result.HasUnsavedChanges);
		}

		[Test]
		public static void TestCompetitionLimitsWithValues()
		{
			var result = new CompetitionTarget(null, 1, 2, false);
			Assert.IsFalse(result.IsEmpty);
			Assert.IsFalse(result.HasUnsavedChanges);

			result.UnionWith(CompetitionLimit.NoLimit);
			Assert.IsFalse(result.HasUnsavedChanges);

			result.UnionWith(CompetitionLimit.Empty);
			Assert.IsFalse(result.HasUnsavedChanges);

			result.UnionWith(new CompetitionLimit(1.1, 1.9));
			Assert.IsFalse(result.HasUnsavedChanges);

			result.UnionWith(new CompetitionLimit(1, 2));
			Assert.IsFalse(result.HasUnsavedChanges);

			result.LooseLimitsAndMarkAsSaved(0);
			Assert.IsFalse(result.HasUnsavedChanges);

			result.LooseLimitsAndMarkAsSaved(10);
			Assert.IsFalse(result.HasUnsavedChanges);

			Assert.Throws<ArgumentOutOfRangeException>(() => result.LooseLimitsAndMarkAsSaved(-1));
			Assert.Throws<ArgumentOutOfRangeException>(() => result.LooseLimitsAndMarkAsSaved(100));

			result.UnionWith(new CompetitionLimit(0.5, 3));
			Assert.AreEqual(result.Min, 0.5);
			Assert.AreEqual(result.Max, 3);
			Assert.IsTrue(result.HasUnsavedChanges);
			Assert.IsTrue(result.IsChanged(CompetitionTargetProperties.MinRatio));
			Assert.IsTrue(result.IsChanged(CompetitionTargetProperties.MaxRatio));

			result.LooseLimitsAndMarkAsSaved(10);
			Assert.AreEqual(result.Min, 0.45); // -10%
			Assert.AreEqual(result.Max, 3.3);  // + 10%
			Assert.IsFalse(result.HasUnsavedChanges);
			Assert.IsFalse(result.IsChanged(CompetitionTargetProperties.MinRatio));
			Assert.IsFalse(result.IsChanged(CompetitionTargetProperties.MaxRatio));
		}
	}
}