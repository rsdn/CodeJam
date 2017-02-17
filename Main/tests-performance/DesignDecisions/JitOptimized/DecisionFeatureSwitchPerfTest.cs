using System;
using System.Diagnostics.CodeAnalysis;

using CodeJam.PerfTests;

using NUnit.Framework;

namespace CodeJam.DesignDecisions.JitOptimized
{
	/// <summary>
	/// Proofs that JIT eliminates code brnaches that are under static readonly variable condition
	/// Use case:
	/// 1. We have a generic class that handles some features depending on the type args.
	/// 2. We want to skip feature-related code as fast as it is possible.
	/// </summary>
	[TestFixture(Category = CompetitionHelpers.PerfTestCategory + ": Design decisions")]
	[CompetitionBurstMode]
	public class DecisionFeatureSwitchPerfTest
	{
		#region PerfTest helpers
		// DONTTOUCH: public and non-static to reproduce the main use case.
		public class AnotherClass
		{
			public static readonly bool FeatureDisabledReadonly = bool.Parse("false"); // calculated at runtime
		}
		private const bool FeatureDisabledConst = false;
		private static readonly bool _featureEnabledReadonly = bool.Parse("true"); // calculated at runtime

		// ReSharper disable once FieldCanBeMadeReadOnly.Local
		private static bool _featureMutableFlag = bool.Parse("false"); // calculated at runtime

		private static readonly bool _featureDisabledReadonly = bool.Parse("false"); // calculated at runtime
		
		private static bool AdditionalFeatureCheck(int a) => a > 1;
		private static int OnFeatureEnabled(int a) => a * a / a;
		private static int DefaultAction(int a) => a + 1;
		#endregion

		private static readonly int Count = CompetitionHelpers.RecommendedSpinCount;

		[Test]
		public void RunDecisionFeatureSwitchPerfTest() => Competition.Run(this);

		[CompetitionBaseline]
		public int Test0000NoFeatureHardcoded()
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
		public int Test0101FeatureDisabledConst()
		{
			var a = 0;
			for (var i = 0; i < Count; i++)
			{
#pragma warning disable 162
				if (FeatureDisabledConst && AdditionalFeatureCheck(a))
					a = OnFeatureEnabled(a);
#pragma warning restore 162
				a = DefaultAction(a);
			}
			return a;
		}

		[CompetitionBenchmark(0.91, 1.08)]
		[SuppressMessage("ReSharper", "HeuristicUnreachableCode")]
		[SuppressMessage("ReSharper", "ConditionIsAlwaysTrueOrFalse")]
		public int Test0102FeatureDisabledConstTwoIf()
		{
			var a = 0;
			for (var i = 0; i < Count; i++)
			{
#pragma warning disable 162
				if (FeatureDisabledConst)
					if (AdditionalFeatureCheck(a))
						a = OnFeatureEnabled(a);
#pragma warning restore 162
				a = DefaultAction(a);
			}
			return a;
		}

		[CompetitionBenchmark(0.94, 1.05)]
		public int Test0201FeatureDisabledReadonly()
		{
			var a = 0;
			for (var i = 0; i < Count; i++)
			{
				if (_featureDisabledReadonly && AdditionalFeatureCheck(a))
					a = OnFeatureEnabled(a);
				a = DefaultAction(a);
			}
			return a;
		}

		[CompetitionBenchmark(0.93, 1.04)]
		public int Test0202FeatureDisabledReadonlyTwoIf()
		{
			var a = 0;
			for (var i = 0; i < Count; i++)
			{
				if (_featureDisabledReadonly)
					if (AdditionalFeatureCheck(a))
						a = OnFeatureEnabled(a);
				a = DefaultAction(a);
			}
			return a;
		}

		[CompetitionBenchmark(0.96, 1.08)]
		public int Test0203AnotherClassFeatureDisabledReadonly()
		{
			var a = 0;
			for (var i = 0; i < Count; i++)
			{
				if (AnotherClass.FeatureDisabledReadonly && AdditionalFeatureCheck(a))
					a = OnFeatureEnabled(a);
				a = DefaultAction(a);
			}
			return a;
		}

		[CompetitionBenchmark(0.94, 1.06)]
		public int Test0204NotFeatureEnabledReadonly()
		{
			var a = 0;
			for (var i = 0; i < Count; i++)
			{
				if (!_featureEnabledReadonly && AdditionalFeatureCheck(a))
					a = OnFeatureEnabled(a);
				a = DefaultAction(a);
			}
			return a;
		}

		[CompetitionBenchmark(1.77, 1.98)]
		public int Test0301FeatureMutableFlag()
		{
			var a = 0;
			for (var i = 0; i < Count; i++)
			{
				if (_featureMutableFlag && AdditionalFeatureCheck(a))
					a = OnFeatureEnabled(a);
				a = DefaultAction(a);
			}
			return a;
		}

		[CompetitionBenchmark(1.84, 2.05)]
		public int Test0302FeatureMutableFlagTwoIf()
		{
			var a = 0;
			for (var i = 0; i < Count; i++)
			{
				if (_featureMutableFlag)
					if (AdditionalFeatureCheck(a))
						a = OnFeatureEnabled(a);
				a = DefaultAction(a);
			}
			return a;
		}

		[CompetitionBenchmark(14.17, 16.58)]
		public int Test0401FeatureEnabledReadonly()
		{
			var a = 0;
			for (var i = 0; i < Count; i++)
			{
				if (_featureEnabledReadonly && AdditionalFeatureCheck(a))
					a = OnFeatureEnabled(a);
				a = DefaultAction(a);
			}
			return a;
		}

		[CompetitionBenchmark(14.18, 16.52)]
		public int Test0401FeatureEnabledReadonlyTwoIf()
		{
			var a = 0;
			for (var i = 0; i < Count; i++)
			{
				if (_featureEnabledReadonly)
					if (AdditionalFeatureCheck(a))
						a = OnFeatureEnabled(a);
				a = DefaultAction(a);
			}
			return a;
		}
	}
}