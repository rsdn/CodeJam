using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

using BenchmarkDotNet.NUnit;

using JetBrains.Annotations;

using NUnit.Framework;

using static CodeJam.AssemblyWideConfig;

namespace CodeJam
{
	/// <summary>
	/// Estimates average cost of calls
	/// </summary>
	[TestFixture(Category = PerfTestsConstants.PerfTestCategory + ": Self-testing")]
	[SuppressMessage("ReSharper", "AccessToModifiedClosure")]
	[SuppressMessage("ReSharper", "ClassCanBeSealed.Local")]
	[SuppressMessage("ReSharper", "ConvertMethodToExpressionBody")]
	[SuppressMessage("ReSharper", "ConvertToConstant.Local")]
	[SuppressMessage("ReSharper", "MemberCanBeMadeStatic.Local")]
	[SuppressMessage("ReSharper", "SuggestVarOrType_BuiltInTypes")]
	[SuppressMessage("ReSharper", "SuggestVarOrType_SimpleTypes")]
	[SuppressMessage("ReSharper", "UnusedTypeParameter")]
	[PublicAPI]
	public class CallCostPerfTests
	{
		[Test]
		// WAITINGFOR: https://github.com/PerfDotNet/BenchmarkDotNet/issues/126.
		[Explicit(PerfTestsConstants.ExplicitExcludeReason)]
		public void RunCallCostPerfTests() =>
			CompetitionBenchmarkRunner.Run(this, RunConfig);

		#region CompetitionMethods
		private interface ICompareCalls
		{
			int CallInterface(int a);
			int CallInterface<T>(int a);
		}

		private interface ICompareCalls<T>
		{
			T CallInterface(T a);
		}

		private class CompareCalls : ICompareCalls<int>, ICompareCalls
		{
			public static int Call(int a)
			{
				return a + 1;
			}

			[MethodImpl(MethodImplOptions.NoInlining)]
			public static int CallNoInline(int a)
			{
				return a + 1;
			}

			public static int Call<T>(int a)
			{
				return a + 1;
			}

			public int CallInst(int a)
			{
				return a + 1;
			}

			[MethodImpl(MethodImplOptions.NoInlining)]
			public int CallInstNoInline(int a)
			{
				return a + 1;
			}

			public int CallInst<T>(int a)
			{
				return a + 1;
			}

			public virtual int CallVirtual(int a)
			{
				return a + 1;
			}

			public virtual int CallInterface(int a)
			{
				return a + 1;
			}

			public virtual int CallInterface<T>(int a)
			{
				return a + 1;
			}
		}

		private class CompareCallsDerived : CompareCalls
		{
			public override int CallVirtual(int a)
			{
				return a + 1;
			}

			public override int CallInterface(int a)
			{
				return a + 1;
			}

			public override int CallInterface<T>(int a)
			{
				return a + 1;
			}
		}
		#endregion

		private const int Count = 100 * 1000;

		[CompetitionBaseline]
		public int Test00Raw()
		{
			int a = 0;
			for (int i = 0; i < Count; i++)
			{
				a = a + 1;
			}

			return Count;
		}

		[CompetitionBenchmark(0.84, 1.16)]
		public int Test01Call()
		{
			int a = 0;
			for (int i = 0; i < Count; i++)
			{
				a = CompareCalls.Call(a);
			}

			return Count;
		}

		[CompetitionBenchmark(0.81, 1.17)]
		public int Test02GenericCall()
		{
			int a = 0;
			for (int i = 0; i < Count; i++)
			{
				a = CompareCalls.Call<object>(a);
			}

			return Count;
		}

		[CompetitionBenchmark(0.79, 1.19)]
		public int Test03InstanceCall()
		{
			int a = 0;
			CompareCalls p = new CompareCalls();
			for (int i = 0; i < Count; i++)
			{
				a = p.CallInst(a);
			}

			return Count;
		}

		[CompetitionBenchmark(0.79, 1.28)]
		public int Test04InstanceGenericCall()
		{
			int a = 0;
			CompareCalls p = new CompareCalls();
			for (int i = 0; i < Count; i++)
			{
				a = p.CallInst<object>(a);
			}

			return Count;
		}

		[CompetitionBenchmark(4.65, 6.37)]
		public int Test05CallNoInline()
		{
			int a = 0;
			for (int i = 0; i < Count; i++)
			{
				a = CompareCalls.CallNoInline(a);
			}

			return Count;
		}

		[CompetitionBenchmark(5.02, 6.46)]
		public int Test06InstanceCallNoInline()
		{
			int a = 0;
			CompareCalls p = new CompareCalls();
			for (int i = 0; i < Count; i++)
			{
				a = p.CallInstNoInline(a);
			}

			return Count;
		}

		[CompetitionBenchmark(5.37, 7.96)]
		public int Test07InstanceVirtualCall()
		{
			int a = 0;
			CompareCalls p = new CompareCalls();
			for (int i = 0; i < Count; i++)
			{
				a = p.CallVirtual(a);
			}

			return Count;
		}

		[CompetitionBenchmark(5.40, 7.36)]
		public int Test08DerivedVirtualCall()
		{
			int a = 0;
			CompareCallsDerived p = new CompareCallsDerived();
			for (int i = 0; i < Count; i++)
			{
				a = p.CallVirtual(a);
			}

			return Count;
		}

		[CompetitionBenchmark(7.09, 9.35)]
		public int Test09InterfaceCall()
		{
			int a = 0;
			ICompareCalls p = new CompareCalls();
			for (int i = 0; i < Count; i++)
			{
				a = p.CallInterface(a);
			}

			return Count;
		}

		[CompetitionBenchmark(7.27, 9.43)]
		public int Test10DerivedInterfaceCall()
		{
			int a = 0;
			ICompareCalls p = new CompareCallsDerived();
			for (int i = 0; i < Count; i++)
			{
				a = p.CallInterface(a);
			}

			return Count;
		}

		[CompetitionBenchmark(6.95, 9.55)]
		public int Test11GenericInterfaceCall()
		{
			int a = 0;
			ICompareCalls<int> p = new CompareCalls();
			for (int i = 0; i < Count; i++)
			{
				a = p.CallInterface(a);
			}

			return Count;
		}

		[CompetitionBenchmark(7.21, 9.74)]
		public int Test12DerivedGenericInterfaceCall()
		{
			int a = 0;
			ICompareCalls<int> p = new CompareCallsDerived();
			for (int i = 0; i < Count; i++)
			{
				a = p.CallInterface(a);
			}

			return Count;
		}

		[CompetitionBenchmark(30.32, 49.85)]
		public int Test13InterfaceGenericCall()
		{
			int a = 0;
			ICompareCalls p = new CompareCalls();
			for (int i = 0; i < Count; i++)
			{
				a = p.CallInterface<object>(a);
			}

			return Count;
		}

		[CompetitionBenchmark(31.74, 40.88)]
		public int Test14DerivedInterfaceGenericCall()
		{
			int a = 0;
			ICompareCalls p = new CompareCallsDerived();
			for (int i = 0; i < Count; i++)
			{
				a = p.CallInterface<object>(a);
			}

			return Count;
		}

		[CompetitionBenchmark(7.03, 9.84)]
		public int Test15LambdaCached()
		{
			int a1 = 0;
			Func<int, int> x = a => a + 1;
			for (int i = 0; i < Count; i++)
			{
				a1 = x(a1);
			}

			return Count;
		}

		[CompetitionBenchmark(8.57, 11.56)]
		public int Test16LambdaNew()
		{
			int a1 = 0;
			for (int i = 0; i < Count; i++)
			{
				Func<int, int> x = a => a + 1;
				a1 = x(a1);
			}

			return Count;
		}

		[CompetitionBenchmark(8.33, 11.57)]
		public int Test17LambdaClosure()
		{
			int a1 = 0;
			int t;
			for (int i = 0; i < Count; i++)
			{
				t = 1;
				Func<int, int> x = a => a + t;
				a1 = x(a1);
			}

			return Count;
		}

		[CompetitionBenchmark(27.81, 44.25)]
		public int Test18LambdaClosureLocal()
		{
			int a1 = 0;
			for (int i = 0; i < Count; i++)
			{
				int t = 1;
				Func<int, int> x = a => a + t;
				a1 = x(a1);
			}

			return Count;
		}

		[CompetitionBenchmark(8.51, 11.74)]
		public int Test19FuncCached()
		{
			int a = 0;
			Func<int, int> x = CompareCalls.Call;
			for (int i = 0; i < Count; i++)
			{
				a = x(a);
			}

			return Count;
		}

		[CompetitionBenchmark(6.38, 9.05)]
		public int Test20FuncCachedInstance()
		{
			int a = 0;
			Func<int, int> x = new CompareCalls().CallInst;
			for (int i = 0; i < Count; i++)
			{
				a = x(a);
			}

			return Count;
		}

		[CompetitionBenchmark(22.79, 32.65)]
		public int Test21FuncNew()
		{
			int a = 0;
			for (int i = 0; i < Count; i++)
			{
				Func<int, int> x = CompareCalls.Call;
				a = x(a);
			}

			return Count;
		}
	}
}