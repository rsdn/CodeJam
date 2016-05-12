using System;

using BenchmarkDotNet.UnitTesting;

using JetBrains.Annotations;

using NUnit.Framework;

using static CodeJam.AssemblyWideConfig;

namespace CodeJam
{
	/// <summary>
	/// Proof test: readonly struct fields ARE copied on write
	/// as in http://blog.nodatime.org/2014/07/micro-optimization-surprising.html
	/// </summary>
	[TestFixture(Category = PerfTestsConstants.PerfTestCategory + ": Self-testing")]
	[Explicit(PerfTestsConstants.ExplicitExcludeReason)]
	public class NestedStructAccessPerfTests
	{
		#region PerfTest helpers
		private struct HeavyStruct
		{
#pragma warning disable 169
#pragma warning disable 649
			private readonly decimal _a;
			private readonly decimal _b;
			private readonly decimal _c;
			private readonly decimal _d;
			private readonly decimal _e;
			private readonly decimal _f;
			private readonly decimal _g;
			private readonly decimal _h;
#pragma warning restore 649
#pragma warning restore 169

			[Pure]
			public decimal Test(decimal i) => _a + i + 1;
		}

		private struct HeavyStructWrapperReadonly
		{
#pragma warning disable 649
			private readonly HeavyStruct _h;
#pragma warning restore 649

			public decimal Test(decimal i) => _h.Test(i);
		}

		private struct HeavyStructWrapperMutable
		{
#pragma warning disable 649
			private HeavyStruct _h;
#pragma warning restore 649

			public decimal Test(decimal i) => _h.Test(i);
		}
		#endregion

		private const int Count = 100 * 1000;

		[Test]
		public void RunNestedStructAccessPerfTests() => CompetitionBenchmarkRunner.Run(this, RunConfig);

		[CompetitionBaseline]
		public decimal Test00Mutable()
		{
			var a = 0m;
			var s = new HeavyStructWrapperMutable();
			for (var i = 0; i < Count; i++)
			{
				a = s.Test(a);
			}
			return a;
		}

		[CompetitionBenchmark(23.28, 26.00)]
		public decimal Test01Readonly()
		{
			var a = 0m;
			var s = new HeavyStructWrapperReadonly();
			for (var i = 0; i < Count; i++)
			{
				a = s.Test(a);
			}
			return a;
		}
	}
}