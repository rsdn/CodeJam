using System;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

using BenchmarkDotNet.Running;

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
			var method = (MethodInfo)MethodBase.GetCurrentMethod();
			var target = new Target(method.DeclaringType, method);
			var result = new CompetitionTarget(target, CompetitionLimit.Empty);
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
			Assert.AreEqual(result.MinRatio, 0);
			Assert.AreEqual(result.MaxRatio, 0);

			result.MarkAsSaved();
			Assert.AreEqual(result.MinRatio, 0);
			Assert.AreEqual(result.MinRatioRounded, 0);
			Assert.AreEqual(result.MinRatioText, "0.00");
			Assert.AreEqual(result.MaxRatio, 0);
			Assert.AreEqual(result.MaxRatioRounded, 0);
			Assert.AreEqual(result.MaxRatioText, "0.00");
			Assert.IsTrue(result.IsEmpty);
			Assert.IsFalse(result.IgnoreAll);
			Assert.IsFalse(result.HasUnsavedChanges);

			result.UnionWith(new CompetitionLimit(1, 2));
			Assert.AreEqual(result.MinRatio, 1);
			Assert.AreEqual(result.MinRatioRounded, 1);
			Assert.AreEqual(result.MinRatioText, "1.00");
			Assert.AreEqual(result.MaxRatio, 2);
			Assert.AreEqual(result.MaxRatioRounded, 2);
			Assert.AreEqual(result.MaxRatioText, "2.00");
			Assert.IsFalse(result.IsEmpty);
			Assert.IsFalse(result.IgnoreAll);
			Assert.IsTrue(result.HasUnsavedChanges);
			Assert.IsTrue(result.IsChanged(CompetitionLimitProperties.MinRatio));
			Assert.IsTrue(result.IsChanged(CompetitionLimitProperties.MaxRatio));

			result.MarkAsSaved();
			Assert.AreEqual(result.MinRatio, 1);
			Assert.AreEqual(result.MinRatioRounded, 1);
			Assert.AreEqual(result.MinRatioText, "1.00");
			Assert.AreEqual(result.MaxRatio, 2);
			Assert.AreEqual(result.MaxRatioRounded, 2);
			Assert.AreEqual(result.MaxRatioText, "2.00");
			Assert.IsFalse(result.IsEmpty);
			Assert.IsFalse(result.IgnoreAll);
			Assert.IsFalse(result.HasUnsavedChanges);
		}

		[Test]
		public static void TestCompetitionLimitIgnored()
		{
			var method = (MethodInfo)MethodBase.GetCurrentMethod();
			var target = new Target(method.DeclaringType, method);
			var result = new CompetitionTarget(target, CompetitionLimit.Ignored);
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

			result.MarkAsSaved();
			Assert.AreEqual(result.MinRatio, -1);
			Assert.AreEqual(result.MaxRatio, -1);
			Assert.IsFalse(result.IsEmpty);
			Assert.IsTrue(result.IgnoreAll);
			Assert.IsFalse(result.HasUnsavedChanges);

			result.UnionWith(new CompetitionLimit(1, 2));
			Assert.AreEqual(result.MinRatio, -1);
			Assert.AreEqual(result.MinRatioRounded, -1);
			Assert.AreEqual(result.MinRatioText, "-1");
			Assert.AreEqual(result.MaxRatio, -1);
			Assert.AreEqual(result.MaxRatioRounded, -1);
			Assert.AreEqual(result.MaxRatioText, "-1");
			Assert.IsFalse(result.IsEmpty);
			Assert.IsTrue(result.IgnoreAll);
			Assert.IsFalse(result.HasUnsavedChanges);
		}

		[Test]
		public static void TestCompetitionLimitWithValues()
		{
			var method = (MethodInfo)MethodBase.GetCurrentMethod();
			var target = new Target(method.DeclaringType, method);
			var result = new CompetitionTarget(target, new CompetitionLimit(1.005, 2.015));
			Assert.AreEqual(result.MinRatio, 1.005);
			Assert.AreEqual(result.MaxRatio, 2.015);
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

			result.MarkAsSaved();
			Assert.AreEqual(result.MinRatio, 1.005);
			Assert.AreEqual(result.MinRatioRounded, 1.00, 0.0001);
			Assert.AreEqual(result.MinRatioText, "1.00");
			Assert.AreEqual(result.MaxRatio, 2.015);
			Assert.AreEqual(result.MaxRatioRounded, 2.02, 0.0001);
			Assert.AreEqual(result.MaxRatioText, "2.02");
			Assert.IsFalse(result.IsEmpty);
			Assert.IsFalse(result.IgnoreAll);
			Assert.IsFalse(result.HasUnsavedChanges);

			result.UnionWith(new CompetitionLimit(1.005, 3));
			Assert.AreEqual(result.MinRatio, 1);
			Assert.AreEqual(result.MinRatioRounded, 1);
			Assert.AreEqual(result.MinRatioText, "1.00");
			Assert.AreEqual(result.MaxRatio, 3);
			Assert.AreEqual(result.MaxRatioRounded, 3);
			Assert.AreEqual(result.MaxRatioText, "3.00");
			Assert.IsFalse(result.IsEmpty);
			Assert.IsFalse(result.IgnoreAll);
			Assert.IsTrue(result.HasUnsavedChanges);
			Assert.IsTrue(result.IsChanged(CompetitionLimitProperties.MinRatio));
			Assert.IsTrue(result.IsChanged(CompetitionLimitProperties.MaxRatio));
			Assert.IsTrue(
				result.IsChanged(
					CompetitionLimitProperties.MinRatio | CompetitionLimitProperties.MaxRatio));

			result.UnionWith(new CompetitionLimit(0.555, 3));
			Assert.AreEqual(result.MinRatioRounded, 0.56, 0.0001);
			Assert.AreEqual(result.MinRatioRounded, 0.56, 0.0001);
			Assert.AreEqual(result.MinRatioText, "0.56");
			Assert.AreEqual(result.MaxRatio, 3);
			Assert.AreEqual(result.MaxRatioRounded, 3);
			Assert.AreEqual(result.MaxRatioText, "3.00");
			Assert.IsFalse(result.IsEmpty);
			Assert.IsFalse(result.IgnoreAll);
			Assert.IsTrue(result.HasUnsavedChanges);
			Assert.IsTrue(result.IsChanged(CompetitionLimitProperties.MinRatio));
			Assert.IsTrue(result.IsChanged(CompetitionLimitProperties.MaxRatio));
			Assert.IsTrue(
				result.IsChanged(
					CompetitionLimitProperties.MinRatio | CompetitionLimitProperties.MaxRatio));
		}
	}
}