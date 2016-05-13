using System;
using System.Diagnostics.CodeAnalysis;

using BenchmarkDotNet.UnitTesting;

using JetBrains.Annotations;

using NUnit.Framework;

using static CodeJam.AssemblyWideConfig;

namespace CodeJam
{
	/// <summary>
	/// Proof test: JIT optimizations on static feature switches
	/// </summary>
	[TestFixture(Category = PerfTestsConstants.PerfTestCategory + ": Self-testing")]
	[PublicAPI]
	public class JitOptimizedFeatureSwitchPerfTests
	{
		// Use case:
		// 1. We have a generic class that handles some features depending on the type args.
		// 2. We want to skip feature-related code as fast as it is possible.

		#region PerfTest helpers
		// DONTTOUCH: public and non-static to reproduce the main use case.
		public class AnotherClass
		{
			public static readonly bool FeatureDisabled = bool.Parse("false"); // should be calculated at runtime
		}

		private const bool FeatureConst = false; // should be calculated at runtime
		private static readonly bool _featureEnabled = bool.Parse("true"); // should be calculated at runtime
		// ReSharper disable once FieldCanBeMadeReadOnly.Local
		private static bool _featureMutable = bool.Parse("false"); // should be calculated at runtime
		private static readonly bool _featureDisabled = bool.Parse("false"); // should be calculated at runtime

		private static bool FeatureCheck(int a) => a > 1;
		private static int FeatureAction(int a) => a * a / a;
		private static int DefaultAction(int a) => a + 1;
		#endregion

		private const int Count = 1000 * 1000;

		[Test]
		[Explicit(PerfTestsConstants.ExplicitExcludeReason)]
		public void RunJitOptimizedFeatureSwitchPerfTests() => CompetitionBenchmarkRunner.Run(this, RunConfig);

		[CompetitionBaseline]
		public int Test0000Baseline()
		{
			var a = 0;
			for (var i = 0; i < Count; i++)
			{
				a = DefaultAction(a);
			}
			return a;
		}

		[CompetitionBenchmark(0.95, 1.06)]
		[SuppressMessage("ReSharper", "HeuristicUnreachableCode")]
		[SuppressMessage("ReSharper", "ConditionIsAlwaysTrueOrFalse")]
		public int Test0101ConstDisabledFeature()
		{
			var a = 0;
			for (var i = 0; i < Count; i++)
			{
#pragma warning disable 162
				if (FeatureConst && FeatureCheck(a))
					a = FeatureAction(a);
#pragma warning restore 162
				a = DefaultAction(a);
			}
			return a;
		}

		[CompetitionBenchmark(0.91, 1.08)]
		[SuppressMessage("ReSharper", "HeuristicUnreachableCode")]
		[SuppressMessage("ReSharper", "ConditionIsAlwaysTrueOrFalse")]
		public int Test0102IfConstDisabledFeature()
		{
			var a = 0;
			for (var i = 0; i < Count; i++)
			{
#pragma warning disable 162
				if (FeatureConst)
					if (FeatureCheck(a))
						a = FeatureAction(a);
#pragma warning restore 162
				a = DefaultAction(a);
			}
			return a;
		}

		[CompetitionBenchmark(0.94, 1.05)]
		public int Test0201DisabledFeature()
		{
			var a = 0;
			for (var i = 0; i < Count; i++)
			{
				if (_featureDisabled && FeatureCheck(a))
					a = FeatureAction(a);
				a = DefaultAction(a);
			}
			return a;
		}

		[CompetitionBenchmark(0.93, 1.04)]
		public int Test0202IfDisabledFeature()
		{
			var a = 0;
			for (var i = 0; i < Count; i++)
			{
				if (_featureDisabled)
					if (FeatureCheck(a))
						a = FeatureAction(a);
				a = DefaultAction(a);
			}
			return a;
		}

		[CompetitionBenchmark(0.96, 1.08)]
		public int Test0203AnotherClassDisabledFeature()
		{
			var a = 0;
			for (var i = 0; i < Count; i++)
			{
				if (AnotherClass.FeatureDisabled && FeatureCheck(a))
					a = FeatureAction(a);
				a = DefaultAction(a);
			}
			return a;
		}

		[CompetitionBenchmark(0.94, 1.06)]
		public int Test0203NotEnabledFeature()
		{
			var a = 0;
			for (var i = 0; i < Count; i++)
			{
				if (!_featureEnabled && FeatureCheck(a))
					a = FeatureAction(a);
				a = DefaultAction(a);
			}
			return a;
		}

		[CompetitionBenchmark(1.77, 1.98)]
		public int Test0301MutableFeature()
		{
			var a = 0;
			for (var i = 0; i < Count; i++)
			{
				if (_featureMutable && FeatureCheck(a))
					a = FeatureAction(a);
				a = DefaultAction(a);
			}
			return a;
		}

		[CompetitionBenchmark(1.84, 2.05)]
		public int Test0301IfMutableFeature()
		{
			var a = 0;
			for (var i = 0; i < Count; i++)
			{
				if (_featureMutable)
					if (FeatureCheck(a))
						a = FeatureAction(a);
				a = DefaultAction(a);
			}
			return a;
		}

		[CompetitionBenchmark(14.17, 16.58)]
		public int Test0301EnabledFeature()
		{
			var a = 0;
			for (var i = 0; i < Count; i++)
			{
				if (_featureEnabled && FeatureCheck(a))
					a = FeatureAction(a);
				a = DefaultAction(a);
			}
			return a;
		}

		[CompetitionBenchmark(14.18, 16.52)]
		public int Test0302IfEnabledFeature()
		{
			var a = 0;
			for (var i = 0; i < Count; i++)
			{
				if (_featureEnabled)
					if (FeatureCheck(a))
						a = FeatureAction(a);
				a = DefaultAction(a);
			}
			return a;
		}
	}
}