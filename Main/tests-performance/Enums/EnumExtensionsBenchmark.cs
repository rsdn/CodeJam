using System;
using System.Diagnostics.CodeAnalysis;

using BenchmarkDotNet.NUnit;

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
	[TestFixture(Category = BenchmarkConstants.BenchmarkCategory + ": Enum extensions")]
	[Explicit(BenchmarkConstants.ExplicitExcludeReason)]
	[SuppressMessage("ReSharper", "SuggestVarOrType_BuiltInTypes")]
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
		public void BenchmarkEnumIsDefined() =>
			CompetitionBenchmarkRunner.Run<IsDefinedCase>(RunConfig);

		public class IsDefinedCase
		{
			[CompetitionBaseline]
			public bool Test00EnumExtensionsDefined()
			{
				bool a = false;
				for (int i = 0; i < Count; i++)
					a = EnumExtensions.IsDefined(F.C | F.D);
				return a;
			}

			[CompetitionBenchmark(0.61, 0.71)]
			public bool Test01EnumExtensionsUndefined()
			{
				bool a = false;
				for (int i = 0; i < Count; i++)
					a = EnumExtensions.IsDefined(F.B | F.C);
				return a;
			}

			[CompetitionBenchmark(28.59, 32.37)]
			public bool Test02EnumDefinedBaseline()
			{
				bool a = false;
				for (int i = 0; i < Count; i++)
					a = Enum.IsDefined(typeof(F), F.C | F.D);
				return a;
			}

			[CompetitionBenchmark(34.28, 38.30)]
			public bool Test03EnumUndefined()
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
			public F Test00EnumExtensionsDefined()
			{
				var a = F.Zero;
				for (int i = 0; i < Count; i++)
					EnumExtensions.TryParse(Fa, out a);
				return a;
			}

			[CompetitionBenchmark(4.98, 5.47)]
			public F Test01EnumExtensionsUndefined()
			{
				var a = F.Zero;
				for (int i = 0; i < Count; i++)
					EnumExtensions.TryParse(Fx, out a);
				return a;
			}

			[CompetitionBenchmark(7.52, 8.04)]
			public F Test02EnumDefinedBaseline()
			{
				var a = F.Zero;
				for (int i = 0; i < Count; i++)
					Enum.TryParse(Fa, out a);
				return a;
			}

			[CompetitionBenchmark(4.61, 4.94)]
			public F Test03EnumUndefined()
			{
				var a = F.Zero;
				for (int i = 0; i < Count; i++)
					Enum.TryParse(Fx, out a);
				return a;
			}
		}
	}
}