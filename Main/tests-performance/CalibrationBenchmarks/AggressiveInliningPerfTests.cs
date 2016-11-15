using System;
using System.Runtime.CompilerServices;

using CodeJam.Arithmetic;
using CodeJam.PerfTests;

using NUnit.Framework;

namespace CodeJam
{
	/// <summary>
	/// Proof test: Aggressive inlining can be used to boost the code.
	/// </summary>
	[TestFixture(Category = CompetitionHelpers.PerfTestCategory + ": Self-testing")]
	[Explicit(CompetitionHelpers.TemporarilyExcludedReason)]
	public class AggressiveInliningPerfTests
	{
		// Use case:
		// 1. We have a complex logec split into multiple methods
		// 2. The logic is used on hotpath
		// 3. [MethodImpl(AggressiveInlining)] can be used to force the inlining and to speedup the code.
		// NB: use with care. In some cases inlining too much code can result in significant slowdown.
		// TODO: prooftest for slowdown, anyone?
		private const int Count = 1000 * 1000;

		[Test]
		public void RunCaseAggInlineNoEffect() => Competition.Run<CaseAggInlineNoEffect>();

		public class CaseAggInlineNoEffect
		{
			#region PerfTest helpers
			private static int CallManualInline(int a) => a + 5;

			// Will inline - auto
			private static int CallAuto1(int a) => CallAuto2(a) + 1;
			private static int CallAuto2(int a) => CallAuto3(a) + 1;
			private static int CallAuto3(int a) => CallAuto4(a) + 1;
			private static int CallAuto4(int a) => CallAuto5(a) + 1;
			private static int CallAuto5(int a) => a + 1;

			// Will NOT inline - forced
			[MethodImpl(MethodImplOptions.NoInlining)]
			private static int CallNoInline1(int a) => CallNoInline2(a) + 1;

			[MethodImpl(MethodImplOptions.NoInlining)]
			private static int CallNoInline2(int a) => CallNoInline3(a) + 1;

			[MethodImpl(MethodImplOptions.NoInlining)]
			private static int CallNoInline3(int a) => CallNoInline4(a) + 1;

			[MethodImpl(MethodImplOptions.NoInlining)]
			private static int CallNoInline4(int a) => CallNoInline5(a) + 1;

			[MethodImpl(MethodImplOptions.NoInlining)]
			private static int CallNoInline5(int a) => a + 1;

			// Will inline - forced
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			private static int CallInline1(int a) => CallInline2(a) + 1;

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			private static int CallInline2(int a) => CallInline3(a) + 1;

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			private static int CallInline3(int a) => CallInline4(a) + 1;

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			private static int CallInline4(int a) => CallInline5(a) + 1;

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			private static int CallInline5(int a) => a + 1;
			#endregion

			[CompetitionBaseline]
			public int Test00Baseline()
			{
				var a = 0;
				for (var i = 0; i < Count; i++)
					a = CallManualInline(a);
				return a;
			}

			[CompetitionBenchmark(0.89, 1.10)]
			public int Test01Auto()
			{
				var a = 0;
				for (var i = 0; i < Count; i++)
					a = CallAuto1(a);
				return a;
			}

			[CompetitionBenchmark(26.89, 30.60)]
			public int Test02NoInline()
			{
				var a = 0;
				for (var i = 0; i < Count; i++)
					a = CallNoInline1(a);
				return a;
			}

			[CompetitionBenchmark(0.90, 1.10)]
			public int Test03AggressiveInline()
			{
				var a = 0;
				for (var i = 0; i < Count; i++)
					a = CallInline1(a);
				return a;
			}
		}

		[Test]
		public void RunCaseAggInlineEffective() => Competition.Run<CaseAggInlineEffective>();

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

			private static int CallManualInline(int a) => a + 1;

			[MethodImpl(MethodImplOptions.NoInlining)]
			private static int CallAuto(int a) =>
				new StructAuto<int>(a, false).Value + 1;

			[MethodImpl(MethodImplOptions.NoInlining)]
			private static int CallNoInline(int a) =>
				new StructNoInline<int>(a, false).Value + 1;

			[MethodImpl(MethodImplOptions.NoInlining)]
			private static int CallInline(int a) =>
				new StructInline<int>(a, false).Value + 1;
			#endregion

			[CompetitionBaseline]
			public int Test00Baseline()
			{
				var a = 0;
				for (var i = 0; i < Count; i++)
					a = CallManualInline(a);
				return a;
			}

			[CompetitionBenchmark(15.22, 16.94)]
			public int Test01Auto()
			{
				var a = 0;
				for (var i = 0; i < Count; i++)
					a = CallAuto(a);
				return a;
			}

			[CompetitionBenchmark(16.34, 18.08)]
			public int Test02NoInline()
			{
				var a = 0;
				for (var i = 0; i < Count; i++)
					a = CallNoInline(a);
				return a;
			}

			[CompetitionBenchmark(7.21, 8.04)]
			public int Test03AggressiveInline()
			{
				var a = 0;
				for (var i = 0; i < Count; i++)
					a = CallInline(a);
				return a;
			}
		}
	}
}