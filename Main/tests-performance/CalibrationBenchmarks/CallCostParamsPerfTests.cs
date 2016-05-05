using System;
using System.Runtime.CompilerServices;

using BenchmarkDotNet.UnitTesting;

using NUnit.Framework;

using static CodeJam.AssemblyWideConfig;

namespace CodeJam
{
	/// <summary>
	/// Estimates average cost of params calls
	/// </summary>
	[TestFixture(Category = PerfTestsConstants.PerfTestCategory + ": Self-testing")]
	[Explicit(PerfTestsConstants.ExplicitExcludeReason)]
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
		public void RunCallCostParamsPerfTests() => CompetitionBenchmarkRunner.Run(this, RunConfig);

		[CompetitionBaseline]
		public int Test00CallBaseline()
		{
			var a = 0;
			for (var i = 0; i < Count; i++)
				a = Call(a);
			return a;
		}

		[CompetitionBenchmark(0.97, 1.05)]
		public int Test01Params()
		{
			var a = 0;
			for (var i = 0; i < Count; i++)
				a = CallParams(a);
			return a;
		}

		[CompetitionBenchmark(0.98, 1.06)]
		public int Test02ParamsCached()
		{
			var a = 0;
			var b = Array.Empty<int>();
			for (var i = 0; i < Count; i++)
				a = CallParams(a, b);
			return a;
		}

		[CompetitionBenchmark(15.97, 16.97)]
		public int Test03ParamsNew()
		{
			var a = 0;
			for (var i = 0; i < Count; i++)
				// ReSharper disable once RedundantExplicitParamsArrayCreation
				a = CallParams(a, new int[0]);
			return a;
		}

		[CompetitionBenchmark(7.58, 8.06)]
		public int Test04CallNoInline()
		{
			var a = 0;
			for (var i = 0; i < Count; i++)
				a = CallNoInline(a);
			return a;
		}

		[CompetitionBenchmark(25.77, 27.42)]
		public int Test05CallParamsNoInline()
		{
			var a = 0;
			for (var i = 0; i < Count; i++)
				a = CallParamsNoInline(a);
			return a;
		}

		[CompetitionBenchmark(8.29, 8.82)]
		public int Test06CallParamsNoInlineCached()
		{
			var a = 0;
			var b = Array.Empty<int>();
			for (var i = 0; i < Count; i++)
				a = CallParamsNoInlineCached(a, b);
			return a;
		}

		[CompetitionBenchmark(22.81, 24.28)]
		public int Test07CallParamsNoInlineNew()
		{
			var a = 0;
			for (var i = 0; i < Count; i++)
				a = CallParamsNoInlineNew(a);
			return a;
		}
	}
}