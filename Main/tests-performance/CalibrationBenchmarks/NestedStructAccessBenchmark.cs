using System;

using BenchmarkDotNet.NUnit;

using JetBrains.Annotations;

using NUnit.Framework;

using static CodeJam.AssemblyWideConfig;

namespace CodeJam
{
	/// <summary>
	/// Proof test: readonly struct fields ARE copied on write
	/// as in http://blog.nodatime.org/2014/07/micro-optimization-surprising.html
	/// </summary>
	[TestFixture(Category = BenchmarkConstants.BenchmarkCategory + ": Self-testing")]
	[PublicAPI]
	public class NestedStructAccessBenchmark
	{
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
			public decimal Test() => _a;
		}

		private struct HeavyStructWrapperReadonly
		{
#pragma warning disable 649
			private readonly HeavyStruct _h;
#pragma warning restore 649

			public decimal Test() => _h.Test();
		}

		private struct HeavyStructWrapperMutable
		{
#pragma warning disable 649
			private HeavyStruct _h;
#pragma warning restore 649

			public decimal Test() => _h.Test();
		}

		[Test]
		[Explicit(BenchmarkConstants.ExplicitExcludeReason)]
		public void BenchmarkNestedStructAccess() => CompetitionBenchmarkRunner.Run(this, RunConfig);

		private const int Count = 10 * 1000 * 1000;

		[CompetitionBaseline]
		public decimal Test00Mutable()
		{
			var sum = 0m;
			var s = new HeavyStructWrapperMutable();
			for (var i = 0; i < Count; i++)
			{
				sum = s.Test();
			}

			return sum;
		}

		[CompetitionBenchmark(23.28, 26.00)]
		public decimal Test01Readonly()
		{
			var sum = 0m;
			var s = new HeavyStructWrapperReadonly();
			for (var i = 0; i < Count; i++)
			{
				sum = s.Test();
			}

			return sum;
		}
	}
}