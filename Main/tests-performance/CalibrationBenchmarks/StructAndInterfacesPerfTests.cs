using System;
using System.Runtime.CompilerServices;

using CodeJam.PerfTests;

using NUnit.Framework;

using static CodeJam.AssemblyWideConfig;

namespace CodeJam
{
	/// <summary>
	/// Proof test: there's effective way to use interfaces over struct
	/// Used as a proof that the fluent API via extension methods will be effective enough
	/// to see what I'm talking about: https://blogs.msdn.microsoft.com/alexj/2009/07/31/another-c-trick-fluents-inheritance-and-extension-methods/
	/// </summary>
	[TestFixture(Category = PerfTestsConstants.PerfTestCategory + ": Self-testing")]
	[Explicit(PerfTestsConstants.ExplicitExcludeReason)]
	public class StructAndInterfacesPerfTests
	{
		#region PerfTest helpers
		private struct Struct : IStruct
		{
			// ReSharper disable once MemberCanBeMadeStatic.Local
			public int Call(int a) => a + 1;

			public int CallInterface(int a) => a + 1;
		}

		private interface IStruct
		{
			int CallInterface(int a);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static int CallExt(IStruct s, int a) => s.CallInterface(a);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static int CallGeneric<TStruct>(TStruct s, int a)
			where TStruct : IStruct => s.CallInterface(a);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static int CallGenericStruct<TStruct>(TStruct s, int a)
			where TStruct : struct, IStruct => s.CallInterface(a);
		#endregion

		private const int Count = 1000 * 1000;

		[Test]
		public void RunStructAndInterfacesPerfTests() =>
			CompetitionBenchmarkRunner.Run(this, RunConfig);

		[CompetitionBaseline]
		public int Test00Baseline()
		{
			var a = 0;
			for (var i = 0; i < Count; i++)
				a = a + 1;
			return Count;
		}

		[CompetitionBenchmark(0.96, 1.08)]
		public int Test01Struct()
		{
			var a = 0;
			var s = new Struct();
			for (var i = 0; i < Count; i++)
				a = s.Call(a);
			return Count;
		}

		[CompetitionBenchmark(0.93, 1.04)]
		public int Test02StructInterfaceCall()
		{
			var a = 0;
			var s = new Struct();
			for (var i = 0; i < Count; i++)
				a = s.CallInterface(a);
			return Count;
		}

		[CompetitionBenchmark(23.62, 29.26)]
		public int Test03InterfaceCall()
		{
			var a = 0;
			var s = new Struct();
			for (var i = 0; i < Count; i++)
				a = CallExt(s, a);
			return Count;
		}

		[CompetitionBenchmark(1.82, 2.13)]
		public int Test04GenericCall()
		{
			var a = 0;
			var s = new Struct();
			for (var i = 0; i < Count; i++)
				a = CallGeneric(s, a);
			return Count;
		}

		[CompetitionBenchmark(1.78, 1.98)]
		public int Test05GenericStructCall()
		{
			var a = 0;
			var s = new Struct();
			for (var i = 0; i < Count; i++)
				a = CallGenericStruct(s, a);
			return Count;
		}
	}
}