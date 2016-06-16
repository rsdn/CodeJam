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
			var result = new CompetitionTarget(null, CompetitionLimit.Empty, false, null);
			Assert.AreEqual(result.MinRatio, CompetitionLimit.EmptyValue);
			Assert.AreEqual(result.MaxRatio, CompetitionLimit.EmptyValue);
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

			result.LooseLimits(0);
			result.MarkAsSaved();
			Assert.IsTrue(result.IsEmpty);
			Assert.IsFalse(result.IgnoreAll);
			Assert.IsFalse(result.HasUnsavedChanges);

			result.LooseLimits(99);
			result.MarkAsSaved();
			Assert.IsTrue(result.IsEmpty);
			Assert.IsFalse(result.IgnoreAll);
			Assert.IsFalse(result.HasUnsavedChanges);

			Assert.AreEqual(result.MinRatio, 0);
			Assert.AreEqual(result.MaxRatio, 0);

			Assert.Throws<ArgumentOutOfRangeException>(() => result.LooseLimits(-1));
			Assert.Throws<ArgumentOutOfRangeException>(() => result.LooseLimits(100));

			result.UnionWith(new CompetitionLimit(1, 2));
			Assert.AreEqual(result.MinRatio, 1);
			Assert.AreEqual(result.MaxRatio, 2);
			Assert.IsFalse(result.IsEmpty);
			Assert.IsFalse(result.IgnoreAll);
			Assert.IsTrue(result.HasUnsavedChanges);
			Assert.IsTrue(result.IsChanged(CompetitionLimitProperties.MinRatio));
			Assert.IsTrue(result.IsChanged(CompetitionLimitProperties.MaxRatio));

			result.LooseLimits(5);
			result.MarkAsSaved();
			Assert.AreEqual(result.MinRatio, 0.95); // -5%
			Assert.AreEqual(result.MaxRatio, 2.1); // + 5%
			Assert.IsFalse(result.IsEmpty);
			Assert.IsFalse(result.IgnoreAll);
			Assert.IsFalse(result.HasUnsavedChanges);
		}

		[Test]
		public static void TestCompetitionLimitIgnored()
		{
			var result = new CompetitionTarget(null, CompetitionLimit.Ignored, false, null);
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

			result.LooseLimits(0);
			result.MarkAsSaved();
			Assert.IsFalse(result.IsEmpty);
			Assert.IsTrue(result.IgnoreAll);
			Assert.IsFalse(result.HasUnsavedChanges);

			result.LooseLimits(99);
			result.MarkAsSaved();
			Assert.IsFalse(result.IsEmpty);
			Assert.IsTrue(result.IgnoreAll);
			Assert.IsFalse(result.HasUnsavedChanges);

			Assert.AreEqual(result.MinRatio, -1);
			Assert.AreEqual(result.MaxRatio, -1);

			Assert.Throws<ArgumentOutOfRangeException>(() => result.LooseLimits(-1));
			Assert.Throws<ArgumentOutOfRangeException>(() => result.LooseLimits(100));

			result.UnionWith(new CompetitionLimit(1, 2));
			Assert.IsFalse(result.IsEmpty);
			Assert.IsTrue(result.IgnoreAll);
			Assert.IsFalse(result.HasUnsavedChanges);
		}

		[Test]
		public static void TestCompetitionLimitWithValues()
		{
			var result = new CompetitionTarget(null, 1, 2, false, null);
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

			result.LooseLimits(0);
			result.MarkAsSaved();
			Assert.IsFalse(result.IsEmpty);
			Assert.IsFalse(result.IgnoreAll);
			Assert.IsFalse(result.HasUnsavedChanges);

			result.LooseLimits(10);
			result.MarkAsSaved();
			Assert.IsFalse(result.IsEmpty);
			Assert.IsFalse(result.IgnoreAll);
			Assert.IsFalse(result.HasUnsavedChanges);

			Assert.AreEqual(result.MinRatio, 1);
			Assert.AreEqual(result.MaxRatio, 2);

			Assert.Throws<ArgumentOutOfRangeException>(() => result.LooseLimits(-1));
			Assert.Throws<ArgumentOutOfRangeException>(() => result.LooseLimits(100));

			result.UnionWith(new CompetitionLimit(0.5, 3));
			Assert.AreEqual(result.MinRatio, 0.5);
			Assert.AreEqual(result.MaxRatio, 3);
			Assert.IsFalse(result.IsEmpty);
			Assert.IsFalse(result.IgnoreAll);
			Assert.IsTrue(result.HasUnsavedChanges);
			Assert.IsTrue(result.IsChanged(CompetitionLimitProperties.MinRatio));
			Assert.IsTrue(result.IsChanged(CompetitionLimitProperties.MaxRatio));
			Assert.IsTrue(
				result.IsChanged(
					CompetitionLimitProperties.MinRatio | CompetitionLimitProperties.MaxRatio));

			result.LooseLimits(10, CompetitionLimitProperties.MinRatio);
			result.MarkAsSaved(CompetitionLimitProperties.MinRatio);
			Assert.AreEqual(result.MinRatio, 0.45); // -10%
			Assert.AreEqual(result.MaxRatio, 3); // not changed
			Assert.IsFalse(result.IsEmpty);
			Assert.IsFalse(result.IgnoreAll);
			Assert.IsTrue(result.HasUnsavedChanges);
			Assert.IsFalse(result.IsChanged(CompetitionLimitProperties.MinRatio));
			Assert.IsTrue(result.IsChanged(CompetitionLimitProperties.MaxRatio));
			Assert.IsFalse(
				result.IsChanged(
					CompetitionLimitProperties.MinRatio | CompetitionLimitProperties.MaxRatio));

			result.LooseLimits(10, CompetitionLimitProperties.MaxRatio);
			result.MarkAsSaved(CompetitionLimitProperties.MaxRatio);
			Assert.AreEqual(result.MinRatio, 0.45); // updated earlier
			Assert.AreEqual(result.MaxRatio, 3.3); // + 10%
			Assert.IsFalse(result.IsEmpty);
			Assert.IsFalse(result.IgnoreAll);
			Assert.IsFalse(result.HasUnsavedChanges);
			Assert.IsFalse(result.IsChanged(CompetitionLimitProperties.MinRatio));
			Assert.IsFalse(result.IsChanged(CompetitionLimitProperties.MaxRatio));
			Assert.IsFalse(
				result.IsChanged(
					CompetitionLimitProperties.MinRatio | CompetitionLimitProperties.MaxRatio));
		}
	}
}