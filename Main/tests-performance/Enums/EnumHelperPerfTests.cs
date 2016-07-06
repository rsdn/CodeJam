using System;
using System.Runtime.CompilerServices;

using CodeJam.Arithmetic;
using CodeJam.PerfTests;

using JetBrains.Annotations;

using NUnit.Framework;

using static CodeJam.AssemblyWideConfig;
using static CodeJam.PerfTests.CompetitionHelpers;

// ReSharper disable once CheckNamespace

namespace CodeJam
{
	/// <summary>
	/// Proof test: <see cref="EnumHelper"/> methods are faster than their framework counterparts.
	/// </summary>
	[PublicAPI]
	[TestFixture(Category = PerfTestCategory + ": EnumHelper")]
	public class EnumHelperPerfTests
	{
		#region PerfTest helpers
		[Flags, PublicAPI]
		public enum F : byte
		{
			Zero = 0x0,
			A = 0x1,
			B = 0x2,
			C = 0x4,
			D = 0x8,
			// ReSharper disable once InconsistentNaming
			CD = C | D
		}

		private const string Fa = nameof(F.A);
		private const string Fx = "X";
		#endregion

		private const int Count = DefaultCount;

		[Test]
		public void RunIsDefinedCase() => Competition.Run<IsDefinedCase>(RunConfig);

		public class IsDefinedCase
		{
			[CompetitionBaseline]
			public bool Test00IsDefined()
			{
				var a = false;
				for (var i = 0; i < Count; i++)
					a = EnumHelper.IsDefined(F.C | F.D);
				return a;
			}

			[CompetitionBenchmark(0.64, 0.71)]
			public bool Test01IsDefinedUndefined()
			{
				var a = false;
				for (var i = 0; i < Count; i++)
					a = EnumHelper.IsDefined(F.B | F.C);
				return a;
			}

			[CompetitionBenchmark(32.07, 37.31)]
			public bool Test02EnumIsDefined()
			{
				var a = false;
				for (var i = 0; i < Count; i++)
					a = Enum.IsDefined(typeof(F), F.C | F.D);
				return a;
			}

			[CompetitionBenchmark(31.75, 37.21)]
			public bool Test03EnumIsDefinedUndefined()
			{
				var a = false;
				for (var i = 0; i < Count; i++)
					a = Enum.IsDefined(typeof(F), F.B | F.C);
				return a;
			}
		}

		[Test]
		public void RunTryParseCase() => Competition.Run<TryParseCase>(RunConfig);

		public class TryParseCase
		{
			[CompetitionBaseline]
			public F Test00TryParse()
			{
				var a = F.Zero;
				for (var i = 0; i < Count; i++)
					EnumHelper.TryParse(Fa, out a);
				return a;
			}

			[CompetitionBenchmark(6.57, 6.98)]
			public F Test01TryParseUndefined()
			{
				var a = F.Zero;
				for (var i = 0; i < Count; i++)
					EnumHelper.TryParse(Fx, out a);
				return a;
			}

			[CompetitionBenchmark(8.86, 9.41)]
			public F Test02EnumTryParse()
			{
				var a = F.Zero;
				for (var i = 0; i < Count; i++)
					Enum.TryParse(Fa, out a);
				return a;
			}

			[CompetitionBenchmark(5.57, 5.91)]
			public F Test03EnumTryParseUndefined()
			{
				var a = F.Zero;
				for (var i = 0; i < Count; i++)
					Enum.TryParse(Fx, out a);
				return a;
			}
		}

		[Test]
		public void RunIsFlagSetCase() => Competition.Run<IsFlagSetCase>(RunConfig);

		public class IsFlagSetCase
		{
			[MethodImpl(MethodImplOptions.NoInlining)]
			private static bool IsFlagSet(F value, F flag) => (value & flag) == flag;

			private static readonly Func<F, F, bool> _isFlagSetEnumOp = OperatorsFactory.IsFlagSetOperator<F>();
			private static readonly Func<int, int, bool> _isFlagSetIntOp = OperatorsFactory.IsFlagSetOperator<int>();

			[CompetitionBaseline]
			public bool Test00Baseline()
			{
				var a = false;
				for (var i = 0; i < Count; i++)
					a = IsFlagSet(F.CD, F.C);
				return a;
			}

			[CompetitionBenchmark(1.07, 1.36)]
			public bool Test01IsFlagSet()
			{
				var a = false;
				for (var i = 0; i < Count; i++)
					a = F.CD.IsFlagSet(F.C);
				return a;
			}

			[CompetitionBenchmark(0.68, 0.90)]
			public bool Test02IsFlagSetEnumOp()
			{
				var a = false;
				for (var i = 0; i < Count; i++)
					a = _isFlagSetEnumOp(F.CD, F.C);
				return a;
			}

			[CompetitionBenchmark(0.76, 0.81)]
			public bool Test03IsFlagSetIntOp()
			{
				var a = false;
				for (var i = 0; i < Count; i++)
					a = _isFlagSetIntOp(8 | 4, 4);
				return a;
			}

			[CompetitionBenchmark(9.17, 11.72)]
			public bool Test04EnumHasFlag()
			{
				var a = false;
				for (var i = 0; i < Count; i++)
					a = F.CD.HasFlag(F.C);
				return a;
			}
		}

		[Test]
		public void RunIsFlagMatchCase() => Competition.Run<IsFlagMatchCase>(RunConfig);

		public class IsFlagMatchCase
		{
			[MethodImpl(MethodImplOptions.NoInlining)]
			private static bool IsFlagMatch(F value, F flag) => flag == 0 || (value & flag) != 0;

			private static readonly Func<F, F, bool> _isFlagMatchEnumOp = OperatorsFactory.IsFlagMatchOperator<F>();
			private static readonly Func<int, int, bool> _isFlagMatchIntOp = OperatorsFactory.IsFlagMatchOperator<int>();

			[CompetitionBaseline]
			public bool Test00Baseline()
			{
				var a = false;
				for (var i = 0; i < Count; i++)
					a = IsFlagMatch(F.CD, F.B | F.C);
				return a;
			}

			[CompetitionBenchmark(1.25, 1.65)]
			public bool Test01IsFlagMatch()
			{
				var a = false;
				for (var i = 0; i < Count; i++)
					a = F.CD.IsFlagSet(F.B | F.C);
				return a;
			}

			[CompetitionBenchmark(0.66, 1.02)]
			public bool Test02IsFlagMatchEnumOp()
			{
				var a = false;
				for (var i = 0; i < Count; i++)
					a = _isFlagMatchEnumOp(F.CD, F.B | F.C);
				return a;
			}

			[CompetitionBenchmark(0.76, 0.85)]
			public bool Test03IsFlagMatchIntOp()
			{
				var a = false;
				for (var i = 0; i < Count; i++)
					a = _isFlagMatchIntOp(8 | 4, 2 | 4);
				return a;
			}
		}

		[Test]
		public void RunSetFlagCase() =>
			Competition.Run<SetFlagCase>(RunConfig);

		public class SetFlagCase
		{
			[MethodImpl(MethodImplOptions.NoInlining)]
			private static F SetFlag(F value, F flag) => value | flag;

			private static readonly Func<F, F, F> _setFlagEnumOp = OperatorsFactory.SetFlagOperator<F>();
			private static readonly Func<int, int, int> _setFlagIntOp = OperatorsFactory.SetFlagOperator<int>();

			[CompetitionBaseline]
			public F Test00Baseline()
			{
				var a = F.A;
				for (var i = 0; i < Count; i++)
					a = SetFlag(F.CD, F.B | F.C);
				return a;
			}

			[CompetitionBenchmark(1.14, 1.52)]
			public F Test01SetFlag()
			{
				var a = F.A;
				for (var i = 0; i < Count; i++)
					a = F.CD.SetFlag(F.B | F.C);
				return a;
			}

			[CompetitionBenchmark(0.66, 0.94)]
			public F Test02SetFlagEnumOp()
			{
				var a = F.A;
				for (var i = 0; i < Count; i++)
					a = _setFlagEnumOp(F.CD, F.B | F.C);
				return a;
			}

			[CompetitionBenchmark(0.66, 0.79)]
			public int Test03SetFlagIntOp()
			{
				var a = 1;
				for (var i = 0; i < Count; i++)
					a = _setFlagIntOp(8 | 4, 2 | 4);
				return a;
			}
		}

		[Test]
		public void RunClearFlagCase() =>
			Competition.Run<ClearFlagCase>(RunConfig);

		public class ClearFlagCase
		{
			[MethodImpl(MethodImplOptions.NoInlining)]
			private static F ClearFlag(F value, F flag) => value & ~flag;

			private static readonly Func<F, F, F> _clearFlagEnumOp = OperatorsFactory.ClearFlagOperator<F>();
			private static readonly Func<int, int, int> _clearFlagIntOp = OperatorsFactory.ClearFlagOperator<int>();

			[CompetitionBaseline]
			public F Test00Baseline()
			{
				var a = F.A;
				for (var i = 0; i < Count; i++)
					a = ClearFlag(F.CD, F.B | F.C);
				return a;
			}

			[CompetitionBenchmark(1.22, 1.45)]
			public F Test01ClearFlag()
			{
				var a = F.A;
				for (var i = 0; i < Count; i++)
					a = F.CD.ClearFlag(F.B | F.C);
				return a;
			}

			[CompetitionBenchmark(0.78, 0.90)]
			public F Test02ClearFlagEnumOp()
			{
				var a = F.A;
				for (var i = 0; i < Count; i++)
					a = _clearFlagEnumOp(F.CD, F.B | F.C);
				return a;
			}

			[CompetitionBenchmark(0.69, 0.78)]
			public int Test03ClearFlagIntOp()
			{
				var a = 1;
				for (var i = 0; i < Count; i++)
					a = _clearFlagIntOp(8 | 4, 2 | 4);
				return a;
			}
		}
	}
}