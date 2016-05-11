using System;
using System.Runtime.CompilerServices;

using BenchmarkDotNet.UnitTesting;

using CodeJam.Arithmetic;

using NUnit.Framework;

using static CodeJam.AssemblyWideConfig;

namespace CodeJam
{
	/// <summary>
	/// Proof test: Aggressive inlining can be used to boost the code.
	/// </summary>
	[TestFixture(Category = PerfTestsConstants.PerfTestCategory + ": Self-testing")]
	[Explicit(PerfTestsConstants.ExplicitExcludeReason)]
	public class AggressiveInliningPerfTests
	{
		// Use case:
		// 1. We have a complex logec split into multiple methods
		// 2. The logic is used on hotpath
		// 3. [MethodImpl(AggressiveInlining)] can be used to force the inlining and to speedup the code.
		// NB: use with care. In some cases inlining can result in significant slowdown.
		private const int Count = 10 * 1000 * 1000;

		[Test]
		public void RunCaseAggInlineNoEffect() =>
			CompetitionBenchmarkRunner.Run<CaseAggInlineNoEffect>(RunConfig);

		public class CaseAggInlineNoEffect
		{
			#region PerfTest helpers
			private static int CallManualInline(int i) => i + 5;

			// Will inline - auto
			private static int CallAuto1(int i) => CallAuto2(i) + 1;
			private static int CallAuto2(int i) => CallAuto3(i) + 1;
			private static int CallAuto3(int i) => CallAuto4(i) + 1;
			private static int CallAuto4(int i) => CallAuto5(i) + 1;
			private static int CallAuto5(int i) => i + 1;

			// Will NOT inline - forced
			[MethodImpl(MethodImplOptions.NoInlining)]
			private static int CallNoInline1(int i) => CallNoInline2(i) + 1;

			[MethodImpl(MethodImplOptions.NoInlining)]
			private static int CallNoInline2(int i) => CallNoInline3(i) + 1;

			[MethodImpl(MethodImplOptions.NoInlining)]
			private static int CallNoInline3(int i) => CallNoInline4(i) + 1;

			[MethodImpl(MethodImplOptions.NoInlining)]
			private static int CallNoInline4(int i) => CallNoInline5(i) + 1;

			[MethodImpl(MethodImplOptions.NoInlining)]
			private static int CallNoInline5(int i) => i + 1;

			// Will inline - forced
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			private static int CallInline1(int i) => CallInline2(i) + 1;

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			private static int CallInline2(int i) => CallInline3(i) + 1;

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			private static int CallInline3(int i) => CallInline4(i) + 1;

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			private static int CallInline4(int i) => CallInline5(i) + 1;

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			private static int CallInline5(int i) => i + 1;
			#endregion

			[CompetitionBaseline]
			public int Test00Baseline()
			{
				var sum = 0;
				for (var i = 0; i < Count; i++)
				{
					sum += CallManualInline(i);
				}
				return sum;
			}

			[CompetitionBenchmark(0.97, 1.04)]
			public int Test01Auto()
			{
				var sum = 0;
				for (var i = 0; i < Count; i++)
				{
					sum += CallAuto1(i);
				}
				return sum;
			}

			[CompetitionBenchmark(10.33, 11.01)]
			public int Test02NoInline()
			{
				var sum = 0;
				for (var i = 0; i < Count; i++)
				{
					sum += CallNoInline1(i);
				}
				return sum;
			}


			[CompetitionBenchmark(0.96, 1.04)]
			public int Test03AggressiveInline()
			{
				var sum = 0;
				for (var i = 0; i < Count; i++)
				{
					sum += CallInline1(i);
				}
				return sum;
			}
		}

		[Test]
		public void RunCaseAggInlineEffective() =>
			CompetitionBenchmarkRunner.Run<CaseAggInlineEffective>(RunConfig);

		public class CaseAggInlineEffective
		{
			#region PerfTest helpers
			// Will NOT inline - auto
			private struct StructAuto<T>
			{
				private static readonly Func<T, T, int> _compareFunc = Operators<T>.Compare;

				public StructAuto(T value, bool validate)
				{
					if (validate)
					{
						if (_compareFunc(value, default(T)) != 0)
						{
							throw new Exception("1");
						}
					}
					else
					{
						if (value == null)
						{
							throw new Exception("2");
						}
					}
					Value = value;
				}

				public T Value { get; }
			}

			// Will NOT inline - forced
			private struct StructNoInline<T>
			{
				private static readonly Func<T, T, int> _compareFunc = Operators<T>.Compare;

				public StructNoInline(T value, bool validate)
				{
					if (validate)
					{
						if (_compareFunc(value, default(T)) != 0)
						{
							throw new Exception("1");
						}
					}
					else
					{
						if (value == null)
						{
							throw new Exception("2");
						}
					}
					Value = value;
				}

				public T Value { get; }
			}

			// Will inline - forced
			private struct StructInline<T>
			{
				private static readonly Func<T, T, int> _compareFunc = Operators<T>.Compare;

				[MethodImpl(MethodImplOptions.AggressiveInlining)]
				public StructInline(T value, bool validate)
				{
					if (validate)
					{
						if (_compareFunc(value, default(T)) != 0)
						{
							throw new Exception("1");
						}
					}
					else
					{
						if (value == null)
						{
							throw new Exception("2");
						}
					}
					Value = value;
				}

				public T Value { get; }
			}

			private static int CallManualInline(int i) => i + 1;

			[MethodImpl(MethodImplOptions.NoInlining)]
			private static int CallAuto(int i) =>
				new StructAuto<int>(i, false).Value + 1;

			[MethodImpl(MethodImplOptions.NoInlining)]
			private static int CallNoInline(int i) =>
				new StructNoInline<int>(i, false).Value + 1;

			[MethodImpl(MethodImplOptions.NoInlining)]
			private static int CallInline(int i) =>
				new StructInline<int>(i, false).Value + 1;
			#endregion

			[CompetitionBaseline]
			public int Test00Baseline()
			{
				var sum = 0;
				for (var i = 0; i < Count; i++)
				{
					sum += CallManualInline(i);
				}
				return sum;
			}

			[CompetitionBenchmark(5.80, 6.18)]
			public int Test01Auto()
			{
				var sum = 0;
				for (var i = 0; i < Count; i++)
				{
					sum += CallAuto(i);
				}
				return sum;
			}

			[CompetitionBenchmark(5.80, 6.20)]
			public int Test02NoInline()
			{
				var sum = 0;
				for (var i = 0; i < Count; i++)
				{
					sum += CallNoInline(i);
				}
				return sum;
			}

			[CompetitionBenchmark(2.60, 2.77)]
			public int Test03AggressiveInline()
			{
				var sum = 0;
				for (var i = 0; i < Count; i++)
				{
					sum += CallInline(i);
				}
				return sum;
			}
		}
	}
}