using System;
using System.Diagnostics.CodeAnalysis;

using CodeJam.PerfTests.Running.SourceAnnotations;

using NUnit.Framework;

namespace CodeJam.PerfTests
{
	[TestFixture(Category = "BenchmarkDotNet")]
	[SuppressMessage("ReSharper", "HeapView.BoxingAllocation")]
	public static class CompetitionTargetTests
	{
		[Test]
		public static void TestCompetitionLimitEmpty()
		{
			var result = new CompetitionTarget(null, CompetitionLimit.Empty, false);
			Assert.AreEqual(result.MinRatio, CompetitionLimit.NotSetValue);
			Assert.AreEqual(result.MaxRatio, CompetitionLimit.NotSetValue);
			Assert.IsTrue(result.IsEmpty);
			Assert.IsFalse(result.IgnoreAll);
			Assert.IsFalse(result.HasUnsavedChanges);

			result.UnionWith(CompetitionLimit.Empty);
			Assert.IsTrue(result.IsEmpty);
			Assert.IsFalse(result.IgnoreAll);
			Assert.IsFalse(result.HasUnsavedChanges);

			result.UnionWith(CompetitionLimit.Ignored);
			Assert.IsTrue(result.IsEmpty);
			Assert.IsFalse(result.IgnoreAll);
			Assert.IsFalse(result.HasUnsavedChanges);

			result.LooseLimitsAndMarkAsSaved(0);
			Assert.IsTrue(result.IsEmpty);
			Assert.IsFalse(result.IgnoreAll);
			Assert.IsFalse(result.HasUnsavedChanges);

			result.LooseLimitsAndMarkAsSaved(99);
			Assert.IsTrue(result.IsEmpty);
			Assert.IsFalse(result.IgnoreAll);
			Assert.IsFalse(result.HasUnsavedChanges);

			Assert.AreEqual(result.MinRatio, 0);
			Assert.AreEqual(result.MaxRatio, 0);

			Assert.Throws<ArgumentOutOfRangeException>(() => result.LooseLimitsAndMarkAsSaved(-1));
			Assert.Throws<ArgumentOutOfRangeException>(() => result.LooseLimitsAndMarkAsSaved(100));

			result.UnionWith(new CompetitionLimit(1, 2));
			Assert.AreEqual(result.MinRatio, 1);
			Assert.AreEqual(result.MaxRatio, 2);
			Assert.IsFalse(result.IsEmpty);
			Assert.IsFalse(result.IgnoreAll);
			Assert.IsTrue(result.HasUnsavedChanges);
			Assert.IsTrue(result.IsChanged(CompetitionTargetProperties.MinRatio));
			Assert.IsTrue(result.IsChanged(CompetitionTargetProperties.MaxRatio));

			result.LooseLimitsAndMarkAsSaved(5);
			Assert.AreEqual(result.MinRatio, 0.95); // -5%
			Assert.AreEqual(result.MaxRatio, 2.1); // + 5%
			Assert.IsFalse(result.IsEmpty);
			Assert.IsFalse(result.IgnoreAll);
			Assert.IsFalse(result.HasUnsavedChanges);
		}

		[Test]
		public static void TestCompetitionLimitIgnored()
		{
			var result = new CompetitionTarget(null, CompetitionLimit.Ignored, false);
			Assert.AreEqual(result.MinRatio, CompetitionLimit.IgnoreValue);
			Assert.AreEqual(result.MaxRatio, CompetitionLimit.IgnoreValue);
			Assert.IsFalse(result.IsEmpty);
			Assert.IsTrue(result.IgnoreAll);
			Assert.IsFalse(result.HasUnsavedChanges);

			result.UnionWith(CompetitionLimit.Empty);
			Assert.IsFalse(result.IsEmpty);
			Assert.IsTrue(result.IgnoreAll);
			Assert.IsFalse(result.HasUnsavedChanges);

			result.UnionWith(CompetitionLimit.Ignored);
			Assert.IsFalse(result.IsEmpty);
			Assert.IsTrue(result.IgnoreAll);
			Assert.IsFalse(result.HasUnsavedChanges);

			result.LooseLimitsAndMarkAsSaved(0);
			Assert.IsFalse(result.IsEmpty);
			Assert.IsTrue(result.IgnoreAll);
			Assert.IsFalse(result.HasUnsavedChanges);

			result.LooseLimitsAndMarkAsSaved(99);
			Assert.IsFalse(result.IsEmpty);
			Assert.IsTrue(result.IgnoreAll);
			Assert.IsFalse(result.HasUnsavedChanges);

			Assert.AreEqual(result.MinRatio, -1);
			Assert.AreEqual(result.MaxRatio, -1);

			Assert.Throws<ArgumentOutOfRangeException>(() => result.LooseLimitsAndMarkAsSaved(-1));
			Assert.Throws<ArgumentOutOfRangeException>(() => result.LooseLimitsAndMarkAsSaved(100));

			result.UnionWith(new CompetitionLimit(1, 2));
			Assert.IsFalse(result.IsEmpty);
			Assert.IsTrue(result.IgnoreAll);
			Assert.IsFalse(result.HasUnsavedChanges);
		}

		[Test]
		public static void TestCompetitionLimitWithValues()
		{
			var result = new CompetitionTarget(null, 1, 2, false);
			Assert.AreEqual(result.MinRatio, 1);
			Assert.AreEqual(result.MaxRatio, 2);
			Assert.IsFalse(result.IsEmpty);
			Assert.IsFalse(result.IgnoreAll);
			Assert.IsFalse(result.HasUnsavedChanges);

			result.UnionWith(CompetitionLimit.Empty);
			Assert.IsFalse(result.IsEmpty);
			Assert.IsFalse(result.IgnoreAll);
			Assert.IsFalse(result.HasUnsavedChanges);

			result.UnionWith(CompetitionLimit.Ignored);
			Assert.IsFalse(result.IsEmpty);
			Assert.IsFalse(result.IgnoreAll);
			Assert.IsFalse(result.HasUnsavedChanges);

			result.UnionWith(new CompetitionLimit(1.1, 1.9));
			Assert.IsFalse(result.IsEmpty);
			Assert.IsFalse(result.IgnoreAll);
			Assert.IsFalse(result.HasUnsavedChanges);

			result.UnionWith(new CompetitionLimit(1, 2));
			Assert.IsFalse(result.IsEmpty);
			Assert.IsFalse(result.IgnoreAll);
			Assert.IsFalse(result.HasUnsavedChanges);

			result.LooseLimitsAndMarkAsSaved(0);
			Assert.IsFalse(result.IsEmpty);
			Assert.IsFalse(result.IgnoreAll);
			Assert.IsFalse(result.HasUnsavedChanges);

			result.LooseLimitsAndMarkAsSaved(10);
			Assert.IsFalse(result.IsEmpty);
			Assert.IsFalse(result.IgnoreAll);
			Assert.IsFalse(result.HasUnsavedChanges);

			Assert.AreEqual(result.MinRatio, 1);
			Assert.AreEqual(result.MaxRatio, 2);

			Assert.Throws<ArgumentOutOfRangeException>(() => result.LooseLimitsAndMarkAsSaved(-1));
			Assert.Throws<ArgumentOutOfRangeException>(() => result.LooseLimitsAndMarkAsSaved(100));

			result.UnionWith(new CompetitionLimit(0.5, 3));
			Assert.AreEqual(result.MinRatio, 0.5);
			Assert.AreEqual(result.MaxRatio, 3);
			Assert.IsFalse(result.IsEmpty);
			Assert.IsFalse(result.IgnoreAll);
			Assert.IsTrue(result.HasUnsavedChanges);
			Assert.IsTrue(result.IsChanged(CompetitionTargetProperties.MinRatio));
			Assert.IsTrue(result.IsChanged(CompetitionTargetProperties.MaxRatio));

			result.LooseLimitsAndMarkAsSaved(10);
			Assert.AreEqual(result.MinRatio, 0.45); // -10%
			Assert.AreEqual(result.MaxRatio, 3.3); // + 10%
			Assert.IsFalse(result.IsEmpty);
			Assert.IsFalse(result.IgnoreAll);
			Assert.IsFalse(result.HasUnsavedChanges);
			Assert.IsFalse(result.IsChanged(CompetitionTargetProperties.MinRatio));
			Assert.IsFalse(result.IsChanged(CompetitionTargetProperties.MaxRatio));
		}
	}
}