using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

using BenchmarkDotNet.UnitTesting;

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
	// WAITINGFOR: https://github.com/PerfDotNet/BenchmarkDotNet/issues/126.
	[Explicit(PerfTestsConstants.ExplicitExcludeReason)]
	[PublicAPI]
	public class CallCostPerfTests
	{
		#region PerfTest helpers
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

		[Test]
		public void RunCallCostPerfTests() => 
			CompetitionBenchmarkRunner.Run(this, RunConfig);

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

		[CompetitionBenchmark(0.88, 1.12)]
		public int Test01Call()
		{
			int a = 0;
			for (int i = 0; i < Count; i++)
			{
				a = CompareCalls.Call(a);
			}
			return Count;
		}

		[CompetitionBenchmark(0.89, 1.28)]
		public int Test02GenericCall()
		{
			int a = 0;
			for (int i = 0; i < Count; i++)
			{
				a = CompareCalls.Call<object>(a);
			}
			return Count;
		}

		[CompetitionBenchmark(0.90, 1.05)]
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

		[CompetitionBenchmark(0.90, 1.09)]
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

		[CompetitionBenchmark(7.13, 9.04)]
		public int Test05CallNoInline()
		{
			int a = 0;
			for (int i = 0; i < Count; i++)
			{
				a = CompareCalls.CallNoInline(a);
			}
			return Count;
		}

		[CompetitionBenchmark(7.45, 9.39)]
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

		[CompetitionBenchmark(5.93, 7.01)]
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

		[CompetitionBenchmark(5.88, 7.51)]
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

		[CompetitionBenchmark(7.64, 9.19)]
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

		[CompetitionBenchmark(7.67, 9.13)]
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

		[CompetitionBenchmark(7.57, 8.98)]
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

		[CompetitionBenchmark(7.63, 9.25)]
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

		[CompetitionBenchmark(36.35, 43.71)]
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

		[CompetitionBenchmark(37.26, 46.17)]
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

		[CompetitionBenchmark(6.72, 8.93)]
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

		[CompetitionBenchmark(9.33, 10.92)]
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

		[CompetitionBenchmark(9.77, 10.86)]
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

		[CompetitionBenchmark(51.18, 82.72)]
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

		[CompetitionBenchmark(9.70, 12.19)]
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

		[CompetitionBenchmark(7.64, 9.10)]
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

		[CompetitionBenchmark(37.31, 43.81)]
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