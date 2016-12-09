using System;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

using BenchmarkDotNet.Running;

using CodeJam.PerfTests.Running.Limits;
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
			var result = new CompetitionTarget(target, LimitRange.Empty, false);
			Assert.AreEqual(result.Limits.ToDisplayString(), "∅");
			Assert.AreEqual(result.Limits.MinRatioText, null);
			Assert.AreEqual(result.Limits.MaxRatioText, null);
			Assert.IsTrue(result.Limits.IsEmpty);
			Assert.IsFalse(result.HasUnsavedChanges);

			result.UnionWith(LimitRange.Empty);
			Assert.IsTrue(result.Limits.IsEmpty);
			Assert.IsFalse(result.HasUnsavedChanges);

			result.UnionWith(LimitRange.CreateRatioLimit(1, 2));
			Assert.AreEqual(result.Limits.ToDisplayString(), "[1..2]");
			Assert.AreEqual(result.Limits.MinRatioText, "1.00");
			Assert.AreEqual(result.Limits.MaxRatioText, "2.00");
			Assert.IsFalse(result.Limits.IsEmpty);
			Assert.IsTrue(result.HasUnsavedChanges);

			result.UnionWith(LimitRange.CreateRatioLimit(0.5, null));
			Assert.AreEqual(result.Limits.ToDisplayString(), "[0.5..+∞)");
			Assert.AreEqual(result.Limits.MinRatioText, "0.50");
			Assert.AreEqual(result.Limits.MaxRatioText, "-1");
			Assert.IsFalse(result.Limits.IsEmpty);
			Assert.IsTrue(result.HasUnsavedChanges);

			result.UnionWith(LimitRange.CreateRatioLimit(null, 0.25));
			Assert.AreEqual(result.Limits.ToDisplayString(), "(-∞..+∞)");
			Assert.AreEqual(result.Limits.MinRatioText, "-1");
			Assert.AreEqual(result.Limits.MaxRatioText, "-1");
			Assert.IsFalse(result.Limits.IsEmpty);
			Assert.IsTrue(result.HasUnsavedChanges);

			result.MarkAsSaved();
			Assert.IsFalse(result.Limits.IsEmpty);
			Assert.IsFalse(result.HasUnsavedChanges);
		}

		[Test]
		public static void TestCompetitionLimitInfinite()
		{
			var method = (MethodInfo)MethodBase.GetCurrentMethod();
			var target = new Target(method.DeclaringType, method);
			var result = new CompetitionTarget(target, LimitRange.CreateRatioLimit(-1, -1), false);
			Assert.AreEqual(result.Limits.ToDisplayString(), "(-∞..+∞)");
			Assert.AreEqual(result.Limits.MinRatioText, "-1");
			Assert.AreEqual(result.Limits.MaxRatioText, "-1");
			Assert.IsFalse(result.Limits.IsEmpty);
			Assert.IsFalse(result.HasUnsavedChanges);

			result.UnionWith(LimitRange.Empty);
			Assert.IsFalse(result.Limits.IsEmpty);
			Assert.IsFalse(result.HasUnsavedChanges);

			result.UnionWith(LimitRange.CreateRatioLimit(1, 2));
			Assert.AreEqual(result.Limits.ToDisplayString(), "(-∞..+∞)");
			Assert.AreEqual(result.Limits.MinRatioText, "-1");
			Assert.AreEqual(result.Limits.MaxRatioText, "-1");
			Assert.IsFalse(result.Limits.IsEmpty);
			Assert.IsFalse(result.HasUnsavedChanges);

			result.UnionWith(LimitRange.CreateRatioLimit(0.5, null));
			Assert.AreEqual(result.Limits.ToDisplayString(), "(-∞..+∞)");
			Assert.AreEqual(result.Limits.MinRatioText, "-1");
			Assert.AreEqual(result.Limits.MaxRatioText, "-1");
			Assert.IsFalse(result.Limits.IsEmpty);
			Assert.IsFalse(result.HasUnsavedChanges);

			result.UnionWith(LimitRange.CreateRatioLimit(null, 0.25));
			Assert.AreEqual(result.Limits.ToDisplayString(), "(-∞..+∞)");
			Assert.AreEqual(result.Limits.MinRatioText, "-1");
			Assert.AreEqual(result.Limits.MaxRatioText, "-1");
			Assert.IsFalse(result.Limits.IsEmpty);
			Assert.IsFalse(result.HasUnsavedChanges);

			result.MarkAsSaved();
			Assert.IsFalse(result.Limits.IsEmpty);
			Assert.IsFalse(result.HasUnsavedChanges);
		}

		[Test]
		public static void TestCompetitionLimitWithValues()
		{
			var method = (MethodInfo)MethodBase.GetCurrentMethod();
			var target = new Target(method.DeclaringType, method);
			var result = new CompetitionTarget(target, LimitRange.CreateRatioLimit(3, 4), false);
			Assert.AreEqual(result.Limits.ToDisplayString(), "[3..4]");
			Assert.AreEqual(result.Limits.MinRatioText, "3.00");
			Assert.AreEqual(result.Limits.MaxRatioText, "4.00");
			Assert.IsFalse(result.Limits.IsEmpty);
			Assert.IsFalse(result.HasUnsavedChanges);

			result.UnionWith(LimitRange.Empty);
			Assert.IsFalse(result.Limits.IsEmpty);
			Assert.IsFalse(result.HasUnsavedChanges);

			result.UnionWith(LimitRange.CreateRatioLimit(1, 2));
			Assert.AreEqual(result.Limits.ToDisplayString(), "[1..4]");
			Assert.AreEqual(result.Limits.MinRatioText, "1.00");
			Assert.AreEqual(result.Limits.MaxRatioText, "4.00");
			Assert.IsFalse(result.Limits.IsEmpty);
			Assert.IsTrue(result.HasUnsavedChanges);

			result.UnionWith(LimitRange.CreateRatioLimit(null, 0.25));
			Assert.AreEqual(result.Limits.ToDisplayString(), "(-∞..4]");
			Assert.AreEqual(result.Limits.MinRatioText, null);
			Assert.AreEqual(result.Limits.MaxRatioText, "4.00");
			Assert.IsFalse(result.Limits.IsEmpty);
			Assert.IsTrue(result.HasUnsavedChanges);

			result.UnionWith(LimitRange.CreateRatioLimit(6, null));
			Assert.AreEqual(result.Limits.ToDisplayString(), "(-∞..+∞)");
			Assert.AreEqual(result.Limits.MinRatioText, "-1");
			Assert.AreEqual(result.Limits.MaxRatioText, "-1");
			Assert.IsFalse(result.Limits.IsEmpty);
			Assert.IsTrue(result.HasUnsavedChanges);

			result.MarkAsSaved();
			Assert.IsFalse(result.Limits.IsEmpty);
			Assert.IsFalse(result.HasUnsavedChanges);
		}
	}
}