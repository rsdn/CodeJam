using System;
using System.Diagnostics.CodeAnalysis;

using BenchmarkDotNet.NUnit;

using CodeJam.Arithmetic;

using JetBrains.Annotations;

using NUnit.Framework;

using static CodeJam.AssemblyWideConfig;

// ReSharper disable once CheckNamespace

namespace CodeJam
{
	/// <summary>
	/// Proof test: <see cref="EnumExtensions"/> methods are faster than their framework counterparts.
	/// </summary>
	[PublicAPI]
	[TestFixture(Category = BenchmarkConstants.BenchmarkCategory + ": EnumExtensions")]
	[Explicit(BenchmarkConstants.ExplicitExcludeReason)]
	[SuppressMessage("ReSharper", "SuggestVarOrType_BuiltInTypes")]
	[SuppressMessage("ReSharper", "ConvertToConstant.Local")]
	public class EnumExtensionsBenchmark
	{
		private const int Count = 250 * 1000;

		[Flags, PublicAPI]
		public enum F
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

		[Test]
		public void BenchmarkIsDefined() =>
			CompetitionBenchmarkRunner.Run<IsDefinedCase>(RunConfig);

		public class IsDefinedCase
		{
			[CompetitionBaseline]
			public bool Test00IsDefined()
			{
				bool a = false;
				for (int i = 0; i < Count; i++)
					a = EnumExtensions.IsDefined(F.C | F.D);
				return a;
			}

			[CompetitionBenchmark(0.69, 0.74)]
			public bool Test01IsDefinedUndefined()
			{
				bool a = false;
				for (int i = 0; i < Count; i++)
					a = EnumExtensions.IsDefined(F.B | F.C);
				return a;
			}

			[CompetitionBenchmark(30.69, 32.61)]
			public bool Test02EnumIsDefined()
			{
				bool a = false;
				for (int i = 0; i < Count; i++)
					a = Enum.IsDefined(typeof(F), F.C | F.D);
				return a;
			}

			[CompetitionBenchmark(31.05, 33.10)]
			public bool Test03EnumIsDefinedUndefined()
			{
				bool a = false;
				for (int i = 0; i < Count; i++)
					a = Enum.IsDefined(typeof(F), F.B | F.C);
				return a;
			}
		}

		[Test]
		public void BenchmarkTryParse() =>
			CompetitionBenchmarkRunner.Run<TryParseCase>(RunConfig);

		public class TryParseCase
		{
			[CompetitionBaseline]
			public F Test00TryParse()
			{
				var a = F.Zero;
				for (int i = 0; i < Count; i++)
					EnumExtensions.TryParse(Fa, out a);
				return a;
			}

			[CompetitionBenchmark(5.86, 6.25)]
			public F Test01TryParseUndefined()
			{
				var a = F.Zero;
				for (int i = 0; i < Count; i++)
					EnumExtensions.TryParse(Fx, out a);
				return a;
			}

			[CompetitionBenchmark(8.29, 8.84)]
			public F Test02EnumTryParse()
			{
				var a = F.Zero;
				for (int i = 0; i < Count; i++)
					Enum.TryParse(Fa, out a);
				return a;
			}

			[CompetitionBenchmark(5.01, 5.36)]
			public F Test03EnumTryParseUndefined()
			{
				var a = F.Zero;
				for (int i = 0; i < Count; i++)
					Enum.TryParse(Fx, out a);
				return a;
			}
		}

		[Test]
		public void BenchmarkIsFlagSet() =>
			CompetitionBenchmarkRunner.Run<IsFlagSetCase>(RunConfig);

		public class IsFlagSetCase
		{
			private static readonly Func<F, F, bool> _isFlagSetEnumOp = OperatorsFactory.IsFlagSetOperator<F>();
			private static readonly Func<int, int, bool> _isFlagSetIntOp = OperatorsFactory.IsFlagSetOperator<int>();

			[CompetitionBaseline]
			public bool Test00Baseline()
			{
				var a = false;
				var value = F.CD;
				var flag = F.C;
				for (int i = 0; i < Count; i++)
				{
					a = (value & flag) == flag;
				}
				return a;
			}

			[CompetitionBenchmark(11.65, 12.38)]
			public bool Test01IsFlagSet()
			{
				var a = false;
				for (int i = 0; i < Count; i++)
					a = F.CD.IsFlagSet(F.C);
				return a;
			}

			[CompetitionBenchmark(6.64, 7.17)]
			public bool Test02IsFlagSetEnumOp()
			{
				var a = false;
				for (int i = 0; i < Count; i++)
					a = _isFlagSetEnumOp(F.CD, F.C);
				return a;
			}

			[CompetitionBenchmark(7.61, 8.16)]
			public bool Test03IsFlagSetIntOp()
			{
				var a = false;
				for (int i = 0; i < Count; i++)
					a = _isFlagSetIntOp(8 | 4, 4);
				return a;
			}

			[CompetitionBenchmark(76.44, 82.41)]
			public bool Test04EnumHasFlag()
			{
				var a = false;
				for (int i = 0; i < Count; i++)
					a = F.CD.HasFlag(F.C);
				return a;
			}
		}

		[Test]
		public void BenchmarkIsFlagMatch() =>
			CompetitionBenchmarkRunner.Run<IsFlagMatchCase>(RunConfig);

		public class IsFlagMatchCase
		{
			private static readonly Func<F, F, bool> _isFlagMatchEnumOp = OperatorsFactory.IsFlagMatchOperator<F>();
			private static readonly Func<int, int, bool> _isFlagMatchIntOp = OperatorsFactory.IsFlagMatchOperator<int>();

			[CompetitionBaseline]
			public bool Test00Baseline()
			{
				var a = false;
				var value = F.CD;
				var flag = F.B | F.C;
				for (int i = 0; i < Count; i++)
					a = flag == 0 || (value & flag) != 0;
				return a;
			}

			[CompetitionBenchmark(8.58, 9.19)]
			public bool Test01IsFlagMatch()
			{
				var a = false;
				for (int i = 0; i < Count; i++)
					a = F.CD.IsFlagSet(F.B | F.C);
				return a;
			}

			[CompetitionBenchmark(5.74, 6.13)]
			public bool Test02IsFlagMatchEnumOp()
			{
				var a = false;
				for (int i = 0; i < Count; i++)
					a = _isFlagMatchEnumOp(F.CD, F.B | F.C);
				return a;
			}

			[CompetitionBenchmark(5.72, 6.14)]
			public bool Test03IsFlagMatchIntOp()
			{
				var a = false;
				for (int i = 0; i < Count; i++)
					a = _isFlagMatchIntOp(8 | 4, 2 | 4);
				return a;
			}
		}
	}
}