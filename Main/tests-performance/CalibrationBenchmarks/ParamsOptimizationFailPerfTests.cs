using System;

using BenchmarkDotNet.NUnit;

using NUnit.Framework;

using static CodeJam.AssemblyWideConfig;

namespace CodeJam
{
	/// <summary>
	/// Proof test: params overloads DOES NOT use Array.Empty{T}
	/// ( https://github.com/dotnet/roslyn/issues/1103 )
	/// </summary>
	[TestFixture(Category = PerfTestsConstants.PerfTestCategory + ": Self-testing")]
	[Explicit(PerfTestsConstants.ExplicitExcludeReason)]
	public class ParamsOptimizationFailPerfTests
	{
		#region PerfTest helpers
		private static int Call(int a) => a + 1;
		// ReSharper disable once UnusedParameter.Local
		private static int CallParams(int a, params int[] args) => a + 1;
		// ReSharper disable once UnusedParameter.Local
		private static int CallArray(int a, int[] args) => a + 1;
		#endregion

		private const int Count = 10 * 1000 * 1000;

		[Test]
		public void RunParamsOptimizationFailPerfTests() => CompetitionBenchmarkRunner.Run(this, RunConfig);

		[CompetitionBaseline]
		public int Test00CallBaseline()
		{
			var a = 0;
			for (var i = 0; i < Count; i++)
				a = Call(a);
			return a;
		}

		[CompetitionBenchmark(14.21, 15.37)]
		public int Test01Params()
		{
			var a = 0;
			for (var i = 0; i < Count; i++)
				a = CallParams(a);
			return a;
		}

		[CompetitionBenchmark(13.89, 14.78)]
		public int Test02Array()
		{
			var a = 0;
			for (var i = 0; i < Count; i++)
				a = CallArray(a, new int[0]);
			return a;
		}

		[CompetitionBenchmark(0.94, 1.01)]
		public int Test03CachedArray()
		{
			var a = 0;
			var b = new int[0];
			for (var i = 0; i < Count; i++)
				a = CallArray(a, b);
			return a;
		}
	}
}