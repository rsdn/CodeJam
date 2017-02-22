using System;
using System.Diagnostics.CodeAnalysis;

using CodeJam.PerfTests;
using CodeJam.PerfTests.Configs;

using NUnit.Framework;

namespace CodeJam.DesignDecisions.JitOptimized
{
	/// <summary>
	/// Proofs that JIT eliminates code branches that are under static readonly variable condition
	/// Use case:
	/// 1. We have a generic class that handles some features depending on the type args.
	/// 2. We want to skip feature-related code as fast as it is possible.
	/// </summary>
	[TestFixture(Category = CompetitionHelpers.PerfTestCategory + ": Design decisions")]
	[SuppressMessage("ReSharper", "ConditionIsAlwaysTrueOrFalse")]
	[SuppressMessage("ReSharper", "HeuristicUnreachableCode")]
	public class DecisionFeatureSwitchPerfTest
	{
		#region PerfTest helpers
		// DONTTOUCH: public and non-static to reproduce the main use case.
		public class AnotherClass
		{
			public static readonly bool FeatureDisabledReadonly = bool.Parse("false"); // calculated at runtime
		}

		private const bool FeatureDisabledConstValue = false;
		private static readonly bool _featureEnabledReadonly = !AnotherClass.FeatureDisabledReadonly;

		// ReSharper disable once FieldCanBeMadeReadOnly.Local
		private static bool _featureMutableFlag = AnotherClass.FeatureDisabledReadonly;

		private static readonly bool _featureDisabledReadonly = AnotherClass.FeatureDisabledReadonly;

		private static readonly int _featureInt5Readonly = int.Parse("5"); // calculated at runtime
		
		private static bool AdditionalFeatureCheck(int a) => a > 1;
		private static int OnFeatureEnabled(int a) => a * a / a;
		private static int DefaultAction(int a) => a + 1;
		#endregion

		private static readonly int Count = CompetitionHelpers.SmallLoopCount;

		[Test]
		public void RunDecisionFeatureSwitchPerfTest() => Competition.Run(this);

		[CompetitionBaseline]
		[GcAllocations(0)]
		public int NoFeatureHardcoded()
		{
			var a = 0;
			for (var i = 0; i < Count; i++)
			{
				a = DefaultAction(a);
			}
			return a;
		}

		[CompetitionBenchmark(0.97, 1.01)]
		[GcAllocations(0)]
		public int FeatureDisabledConst()
		{
			var a = 0;
			for (var i = 0; i < Count; i++)
			{
#pragma warning disable 162
				if (FeatureDisabledConstValue && AdditionalFeatureCheck(a))
					a = OnFeatureEnabled(a);
#pragma warning restore 162
				a = DefaultAction(a);
			}
			return a;
		}

		[CompetitionBenchmark(0.99, 1.03)]
		[GcAllocations(0)]
		public int FeatureDisabledConstTwoIf()
		{
			var a = 0;
			for (var i = 0; i < Count; i++)
			{
#pragma warning disable 162
				if (FeatureDisabledConstValue)
					if (AdditionalFeatureCheck(a))
						a = OnFeatureEnabled(a);
#pragma warning restore 162
				a = DefaultAction(a);
			}
			return a;
		}

		[CompetitionBenchmark(0.95, 1.02)]
		[GcAllocations(0)]
		public int FeatureDisabledReadonly()
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

		[CompetitionBenchmark(0.95, 1.02)]
		[GcAllocations(0)]
		public int FeatureDisabledReadonlyTwoIf()
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

		[CompetitionBenchmark(0.94, 1.03)]
		[GcAllocations(0)]
		public int AnotherClassFeatureDisabledReadonly()
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

		[CompetitionBenchmark(0.96, 1.03)]
		[GcAllocations(0)]
		public int NotFeatureEnabledReadonly()
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

		[CompetitionBenchmark(0.95, 1.01)]
		[GcAllocations(0)]
		public int FeatureIntGreaterThanReadonly()
		{
			var a = 0;
			for (var i = 0; i < Count; i++)
			{
				if (_featureInt5Readonly > 10 && AdditionalFeatureCheck(a))
					a = OnFeatureEnabled(a);
				a = DefaultAction(a);
			}
			return a;
		}

		[CompetitionBenchmark(1.58, 1.72)]
		[GcAllocations(0)]
		public int FeatureMutableFlag()
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

		[CompetitionBenchmark(1.59, 1.73)]
		[GcAllocations(0)]
		public int FeatureMutableFlagTwoIf()
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

		[CompetitionBenchmark(18.19, 19.37)]
		[GcAllocations(0)]
		public int FeatureEnabledReadonly()
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

		[CompetitionBenchmark(18.14, 19.62)]
		[GcAllocations(0)]
		public int FeatureEnabledReadonlyTwoIf()
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