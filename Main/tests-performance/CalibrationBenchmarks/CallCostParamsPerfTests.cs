using System;
using System.Runtime.CompilerServices;

using CodeJam.PerfTests;

using NUnit.Framework;

using static CodeJam.AssemblyWideConfig;

namespace CodeJam
{
	/// <summary>
	/// Estimates average cost of params calls
	/// </summary>
	[TestFixture(Category = CompetitionHelpers.PerfTestCategory + ": Self-testing")]
	[Explicit(CompetitionHelpers.ExplicitExcludeReason)]
	public class CallCostParamsPerfTests
	{
		#region PerfTest helpers
		private static int Call(int a) => a + 1;

		// ReSharper disable once UnusedParameter.Local
		private static int CallParams(int a, params int[] args) => a + 1;

		[MethodImpl(MethodImplOptions.NoInlining)]
		private static int CallNoInline(int a) => Call(a);

		[MethodImpl(MethodImplOptions.NoInlining)]
		private static int CallParamsNoInline(int a) => CallParams(a);

		[MethodImpl(MethodImplOptions.NoInlining)]
		// ReSharper disable once RedundantExplicitParamsArrayCreation
		private static int CallParamsNoInlineNew(int a) => CallParams(a, new int[0]);

		[MethodImpl(MethodImplOptions.NoInlining)]
		private static int CallParamsNoInlineCached(int a, int[] args) => CallParams(a, args);
		#endregion

		private const int Count = 1000 * 1000;

		[Test]
		public void RunCallCostParamsPerfTests() =>
			Competition.Run(this, RunConfig);

		[CompetitionBaseline]
		public int Test00CallBaseline()
		{
			var a = 0;
			for (var i = 0; i < Count; i++)
				a = Call(a);
			return a;
		}

		[CompetitionBenchmark(0.88, 1.09)]
		public int Test01Params()
		{
			var a = 0;
			for (var i = 0; i < Count; i++)
				a = CallParams(a);
			return a;
		}

		[CompetitionBenchmark(0.90, 1.08)]
		public int Test02ParamsCached()
		{
			var a = 0;
			var b = Array.Empty<int>();
			for (var i = 0; i < Count; i++)
				a = CallParams(a, b);
			return a;
		}

		[CompetitionBenchmark(16.00, 22.29)]
		public int Test03ParamsNew()
		{
			var a = 0;
			for (var i = 0; i < Count; i++)
				// ReSharper disable once RedundantExplicitParamsArrayCreation
				a = CallParams(a, new int[0]);
			return a;
		}

		[CompetitionBenchmark(6.68, 8.37)]
		public int Test04CallNoInline()
		{
			var a = 0;
			for (var i = 0; i < Count; i++)
				a = CallNoInline(a);
			return a;
		}

		[CompetitionBenchmark(6.73, 8.40)]
		public int Test05CallParamsNoInline()
		{
			var a = 0;
			for (var i = 0; i < Count; i++)
				a = CallParamsNoInline(a);
			return a;
		}

		[CompetitionBenchmark(7.26, 9.42)]
		public int Test06CallParamsNoInlineCached()
		{
			var a = 0;
			var b = Array.Empty<int>();
			for (var i = 0; i < Count; i++)
				a = CallParamsNoInlineCached(a, b);
			return a;
		}

		[CompetitionBenchmark(22.05, 28.76)]
		public int Test07CallParamsNoInlineNew()
		{
			var a = 0;
			for (var i = 0; i < Count; i++)
				a = CallParamsNoInlineNew(a);
			return a;
		}
	}
}