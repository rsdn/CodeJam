using System;
using System.Reflection;
using System.Runtime.CompilerServices;

using CodeJam.PerfTests;

using NUnit.Framework;

using static CodeJam.AssemblyWideConfig;

namespace CodeJam
{
	/// <summary>
	/// Proof test: JIT optimizations on handwritten method dispatching
	/// </summary>
	[TestFixture(Category = PerfTestsConstants.PerfTestCategory + ": Self-testing")]
	public class JitOptimizedDispatchPerfTests
	{
		// Use case:
		// 1. We have multiple implementations for the same algorithm.
		// 2. We want to choose implementation depending on process' environment: feature switches, FW version etc.
		// 3. We want as few penalty for dispatching as it is possible;

		#region PerfTest helpers
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static int Implementation1(int a) => a * a + 1;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static int Implementation2(int a) => a + 1;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static int Implementation3(int a) => a / 2 + 1;

		private enum ImplementationToUse
		{
			Implementation1,
			Implementation2,
			Implementation3
		}

		#region Dispatching implementations
		[MethodImpl(MethodImplOptions.NoInlining)]
		private static int DirectCall(int a) => Implementation2(a);

		// ReSharper disable once ConvertToConstant.Local
		private static readonly ImplementationToUse _implementationToUse1 = ImplementationToUse.Implementation2;

		[MethodImpl(MethodImplOptions.NoInlining)]
		private static int SwitchOverReadonlyField(int a)
		{
			switch (_implementationToUse1)
			{
				case ImplementationToUse.Implementation1:
					return Implementation1(a);
				case ImplementationToUse.Implementation2:
					return Implementation2(a);
				case ImplementationToUse.Implementation3:
					return Implementation3(a);
				default:
					throw new ArgumentOutOfRangeException();
			}
		}

		private static volatile ImplementationToUse _implementationToUse2 = ImplementationToUse.Implementation2;

		[MethodImpl(MethodImplOptions.NoInlining)]
		private static int SwitchOverMutableField(int a)
		{
			switch (_implementationToUse2)
			{
				case ImplementationToUse.Implementation1:
					return Implementation1(a);
				case ImplementationToUse.Implementation2:
					return Implementation2(a);
				case ImplementationToUse.Implementation3:
					return Implementation3(a);
				default:
					throw new ArgumentOutOfRangeException();
			}
		}
		#endregion

		#endregion

		#region Assertion to proof the idea works at all
		[Test]
		public void AssertJitOptimizedDispatch()
		{
			const int someNum = 1024;
			var impl2 = Implementation2(someNum);
			var impl3 = Implementation3(someNum);

			// 1. Triggers JIT of the methods. Impl2 should be used.
			Assert.AreEqual(DirectCall(someNum), impl2);
			Assert.AreEqual(SwitchOverReadonlyField(someNum), impl2);
			Assert.AreEqual(SwitchOverMutableField(someNum), impl2);

			// 2. Update the field values:

			// 2.1. Updating the readonly field. The change should be ignored.
			// ReSharper disable once PossibleNullReferenceException
			typeof(JitOptimizedDispatchPerfTests)
				.GetField(nameof(_implementationToUse1), BindingFlags.Static | BindingFlags.NonPublic)
				.SetValue(null, ImplementationToUse.Implementation3);

			//2.2. Updating the field.
			// Should NOT be ignored;
			_implementationToUse2 = ImplementationToUse.Implementation3;

			// 3. Now, the assertions:
			// Nothing changed
			Assert.AreEqual(DirectCall(someNum), impl2);
			// Same as previous call (switch thrown away by JIT)
			Assert.AreEqual(SwitchOverReadonlyField(someNum), impl2);
			// Uses implementation 3
			Assert.AreEqual(SwitchOverMutableField(someNum), impl3);
		}
		#endregion

		private const int Count = 1000 * 1000;

		[Test]
		[Explicit(PerfTestsConstants.ExplicitExcludeReason)]
		public void RunJitOptimizedDispatchPerfTests() => CompetitionBenchmarkRunner.Run(this, RunConfig);

		[CompetitionBaseline]
		public int Test00Baseline()
		{
			var a = 0;
			for (var i = 0; i < Count; i++)
				a = DirectCall(a);
			return a;
		}

		[CompetitionBenchmark(0.95, 1.06)]
		public int Test01SwitchOverReadonlyField()
		{
			var a = 0;
			for (var i = 0; i < Count; i++)
				a = SwitchOverReadonlyField(a);
			return a;
		}

		[CompetitionBenchmark(1.42, 1.75)]
		public int Test02SwitchOverMutableField()
		{
			var a = 0;
			for (var i = 0; i < Count; i++)
				a = SwitchOverMutableField(a);
			return a;
		}
	}
}