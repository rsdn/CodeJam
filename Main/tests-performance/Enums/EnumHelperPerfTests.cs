using System;
using System.Runtime.CompilerServices;

using CodeJam.Arithmetic;
using CodeJam.PerfTests;

using JetBrains.Annotations;

using NUnit.Framework;

using static CodeJam.PerfTests.CompetitionHelpers;

// ReSharper disable once CheckNamespace

namespace CodeJam
{
	/// <summary>
	/// Proof test: <see cref="EnumHelper"/> methods are faster than their framework counterparts.
	/// </summary>
	[PublicAPI]
	[TestFixture(Category = PerfTestCategory + ": EnumHelper")]
	[CompetitionBurstMode]
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

		private static readonly int Count = CompetitionRunHelpers.BurstModeLoopCount / 16;

		[Test]
		public void RunIsDefinedCase() => Competition.Run<IsDefinedCase>();

		public class IsDefinedCase
		{
			[CompetitionBaseline]
			[GcAllocations(0)]
			public bool Test00IsDefined()
			{
				var a = false;
				for (var i = 0; i < Count; i++)
					a = EnumHelper.IsDefined(F.C | F.D);
				return a;
			}

			[CompetitionBenchmark(0.44, 1.38)]
			[GcAllocations(0)]
			public bool Test01IsDefinedUndefined()
			{
				var a = false;
				for (var i = 0; i < Count; i++)
					a = EnumHelper.IsDefined(F.B | F.C);
				return a;
			}

			[CompetitionBenchmark(20.99, 56.07)]
			[GcAllocations(29.31, 48.02, BinarySizeUnit.Kilobyte)]
			public bool Test02EnumIsDefined()
			{
				var a = false;
				for (var i = 0; i < Count; i++)
					a = Enum.IsDefined(typeof(F), F.C | F.D);
				return a;
			}

			[CompetitionBenchmark(14.57, 62.21)]
			[GcAllocations(29.31, 48.02, BinarySizeUnit.Kilobyte)]
			public bool Test03EnumIsDefinedUndefined()
			{
				var a = false;
				for (var i = 0; i < Count; i++)
					a = Enum.IsDefined(typeof(F), F.B | F.C);
				return a;
			}
		}

		[Test]
		public void RunTryParseCase() => Competition.Run<TryParseCase>();

		public class TryParseCase
		{
			[CompetitionBaseline]
			[GcAllocations(0)]
			public F Test00TryParse()
			{
				var a = F.Zero;
				for (var i = 0; i < Count; i++)
					EnumHelper.TryParse(Fa, out a);
				return a;
			}

			[CompetitionBenchmark(3.80, 4.67)]
			[GcAllocations(160, BinarySizeUnit.Kilobyte)]
			public F Test02CheckViaEnumInfo()
			{
				var a = F.Zero;
				for (var i = 0; i < Count; i++)
					EnumHelper.IsDefined<F>(Fa);
				return a;
			}

			[CompetitionBenchmark(5.20, 11.34)]
			[GcAllocations(53.73, 88.02, BinarySizeUnit.Kilobyte)]
			public F Test01TryParseUndefined()
			{
				var a = F.Zero;
				for (var i = 0; i < Count; i++)
					EnumHelper.TryParse(Fx, out a);
				return a;
			}

			[CompetitionBenchmark(6.30, 18.75)]
			[GcAllocations(68.37, 112.02, BinarySizeUnit.Kilobyte)]
			public F Test02EnumTryParse()
			{
				var a = F.Zero;
				for (var i = 0; i < Count; i++)
					Enum.TryParse(Fa, out a);
				return a;
			}

			[CompetitionBenchmark(3.69, 8.37)]
			[GcAllocations(53.73, 88.03, BinarySizeUnit.Kilobyte)]
			public F Test03EnumTryParseUndefined()
			{
				var a = F.Zero;
				for (var i = 0; i < Count; i++)
					Enum.TryParse(Fx, out a);
				return a;
			}
		}

		[Test]
		public void RunIsFlagSetCase() => Competition.Run<IsFlagSetCase>();

		public class IsFlagSetCase
		{
			[MethodImpl(MethodImplOptions.NoInlining)]
			private static bool IsFlagSet(F value, F flag) => (value & flag) == flag;

			private static readonly Func<F, F, bool> _isFlagSetEnumOp = OperatorsFactory.IsFlagSetOperator<F>();
			private static readonly Func<int, int, bool> _isFlagSetIntOp = OperatorsFactory.IsFlagSetOperator<int>();

			[CompetitionBaseline]
			[GcAllocations(0)]
			public bool Test00Baseline()
			{
				var a = false;
				for (var i = 0; i < Count; i++)
					a = IsFlagSet(F.CD, F.C);
				return a;
			}

			[CompetitionBenchmark(0.99, 1.85)]
			[GcAllocations(0)]
			public bool Test01IsFlagSet()
			{
				var a = false;
				for (var i = 0; i < Count; i++)
					a = F.CD.IsFlagSet(F.C);
				return a;
			}

			[CompetitionBenchmark(0.68, 1.65)]
			[GcAllocations(0)]
			public bool Test02IsFlagSetEnumOp()
			{
				var a = false;
				for (var i = 0; i < Count; i++)
					a = _isFlagSetEnumOp(F.CD, F.C);
				return a;
			}

			[CompetitionBenchmark(0.56, 1.24)]
			[GcAllocations(0)]
			public bool Test03IsFlagSetIntOp()
			{
				var a = false;
				for (var i = 0; i < Count; i++)
					a = _isFlagSetIntOp(8 | 4, 4);
				return a;
			}

			[CompetitionBenchmark(7.18, 20.77)]
			[GcAllocations(29.30, 48.02, BinarySizeUnit.Kilobyte)]
			public bool Test04EnumHasFlag()
			{
				var a = false;
				for (var i = 0; i < Count; i++)
					a = F.CD.HasFlag(F.C);
				return a;
			}
		}

		[Test]
		public void RunIsAnyFlagSetCase() => Competition.Run<IsAnyFlagSetCase>();

		public class IsAnyFlagSetCase
		{
			[MethodImpl(MethodImplOptions.NoInlining)]
			private static bool IsAnyFlagSet(F value, F flag) => flag == 0 || (value & flag) != 0;

			private static readonly Func<F, F, bool> _isAnyFlagSetEnumOp = OperatorsFactory.IsAnyFlagSetOperator<F>();
			private static readonly Func<int, int, bool> _isAnyFlagSetIntOp = OperatorsFactory.IsAnyFlagSetOperator<int>();

			[CompetitionBaseline]
			[GcAllocations(0)]
			public bool Test00Baseline()
			{
				var a = false;
				for (var i = 0; i < Count; i++)
					a = IsAnyFlagSet(F.CD, F.B | F.C);
				return a;
			}

			[CompetitionBenchmark(0.86, 2.27)]
			[GcAllocations(0)]
			public bool Test01IsAnyFlagSet()
			{
				var a = false;
				for (var i = 0; i < Count; i++)
					a = F.CD.IsFlagSet(F.B | F.C);
				return a;
			}

			[CompetitionBenchmark(0.53, 1.22)]
			[GcAllocations(0)]
			public bool Test02IsAnyFlagSetEnumOp()
			{
				var a = false;
				for (var i = 0; i < Count; i++)
					a = _isAnyFlagSetEnumOp(F.CD, F.B | F.C);
				return a;
			}

			[CompetitionBenchmark(0.51, 1.95)]
			[GcAllocations(0)]
			public bool Test03IsAnyFlagSetIntOp()
			{
				var a = false;
				for (var i = 0; i < Count; i++)
					a = _isAnyFlagSetIntOp(8 | 4, 2 | 4);
				return a;
			}
		}

		[Test]
		public void RunSetFlagCase() => Competition.Run<SetFlagCase>();

		public class SetFlagCase
		{
			[MethodImpl(MethodImplOptions.NoInlining)]
			private static F SetFlag(F value, F flag) => value | flag;

			private static readonly Func<F, F, F> _setFlagEnumOp = OperatorsFactory.SetFlagOperator<F>();
			private static readonly Func<int, int, int> _setFlagIntOp = OperatorsFactory.SetFlagOperator<int>();

			[CompetitionBaseline]
			[GcAllocations(0)]
			public F Test00Baseline()
			{
				var a = F.A;
				for (var i = 0; i < Count; i++)
					a = SetFlag(F.CD, F.B | F.C);
				return a;
			}

			[CompetitionBenchmark(0.59, 1.93)]
			[GcAllocations(0)]
			public F Test01SetFlag()
			{
				var a = F.A;
				for (var i = 0; i < Count; i++)
					a = F.CD.SetFlag(F.B | F.C);
				return a;
			}

			[CompetitionBenchmark(0.35, 1.16)]
			[GcAllocations(0)]
			public F Test02SetFlagEnumOp()
			{
				var a = F.A;
				for (var i = 0; i < Count; i++)
					a = _setFlagEnumOp(F.CD, F.B | F.C);
				return a;
			}

			[CompetitionBenchmark(0.34, 1.07)]
			[GcAllocations(0)]
			public int Test03SetFlagIntOp()
			{
				var a = 1;
				for (var i = 0; i < Count; i++)
					a = _setFlagIntOp(8 | 4, 2 | 4);
				return a;
			}
		}

		[Test]
		public void RunClearFlagCase() => Competition.Run<ClearFlagCase>();

		public class ClearFlagCase
		{
			[MethodImpl(MethodImplOptions.NoInlining)]
			private static F ClearFlag(F value, F flag) => value & ~flag;

			private static readonly Func<F, F, F> _clearFlagEnumOp = OperatorsFactory.ClearFlagOperator<F>();
			private static readonly Func<int, int, int> _clearFlagIntOp = OperatorsFactory.ClearFlagOperator<int>();

			[CompetitionBaseline]
			[GcAllocations(0)]
			public F Test00Baseline()
			{
				var a = F.A;
				for (var i = 0; i < Count; i++)
					a = ClearFlag(F.CD, F.B | F.C);
				return a;
			}

			[CompetitionBenchmark(1.05, 2.16)]
			[GcAllocations(0)]
			public F Test01ClearFlag()
			{
				var a = F.A;
				for (var i = 0; i < Count; i++)
					a = F.CD.ClearFlag(F.B | F.C);
				return a;
			}

			[CompetitionBenchmark(0.76, 1.29)]
			[GcAllocations(0)]
			public F Test02ClearFlagEnumOp()
			{
				var a = F.A;
				for (var i = 0; i < Count; i++)
					a = _clearFlagEnumOp(F.CD, F.B | F.C);
				return a;
			}

			[CompetitionBenchmark(0.59, 1.00)]
			[GcAllocations(0)]
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