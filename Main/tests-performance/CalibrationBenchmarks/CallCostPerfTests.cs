using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

using CodeJam.PerfTests;

using NUnit.Framework;

namespace CodeJam
{
	/// <summary>
	/// Estimates average cost of calls
	/// </summary>
	[TestFixture(Category = CompetitionHelpers.PerfTestCategory + ": Self-testing")]
	[SuppressMessage("ReSharper", "AccessToModifiedClosure")]
	[SuppressMessage("ReSharper", "ClassCanBeSealed.Local")]
	[SuppressMessage("ReSharper", "ConvertMethodToExpressionBody")]
	[SuppressMessage("ReSharper", "ConvertToConstant.Local")]
	[SuppressMessage("ReSharper", "MemberCanBeMadeStatic.Local")]
	[SuppressMessage("ReSharper", "SuggestVarOrType_BuiltInTypes")]
	[SuppressMessage("ReSharper", "SuggestVarOrType_SimpleTypes")]
	[SuppressMessage("ReSharper", "UnusedTypeParameter")]
	// WAITINGFOR: https://github.com/PerfDotNet/BenchmarkDotNet/issues/126.
	[Explicit(CompetitionHelpers.TemporarilyExcludedReason)]
	[CompetitionBurstMode]
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
		public void RunCallCostPerfTests() => Competition.Run(this);

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

		[CompetitionBenchmark(0.97, 1.11)]
		public int Test01Call()
		{
			int a = 0;
			for (int i = 0; i < Count; i++)
			{
				a = CompareCalls.Call(a);
			}
			return Count;
		}

		[CompetitionBenchmark(0.95, 1.17)]
		public int Test02GenericCall()
		{
			int a = 0;
			for (int i = 0; i < Count; i++)
			{
				a = CompareCalls.Call<object>(a);
			}
			return Count;
		}

		[CompetitionBenchmark(0.97, 1.09)]
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

		[CompetitionBenchmark(0.99, 1.11)]
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

		[CompetitionBenchmark(7.11, 9.17)]
		public int Test05CallNoInline()
		{
			int a = 0;
			for (int i = 0; i < Count; i++)
			{
				a = CompareCalls.CallNoInline(a);
			}
			return Count;
		}

		[CompetitionBenchmark(8.03, 9.01)]
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

		[CompetitionBenchmark(6.42, 7.22)]
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

		[CompetitionBenchmark(6.49, 7.20)]
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

		[CompetitionBenchmark(7.99, 8.98)]
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

		[CompetitionBenchmark(8.31, 9.37)]
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

		[CompetitionBenchmark(8.16, 9.60)]
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

		[CompetitionBenchmark(8.33, 9.22)]
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

		[CompetitionBenchmark(39.97, 44.41)]
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

		[CompetitionBenchmark(40.55, 45.36)]
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

		[CompetitionBenchmark(7.11, 8.99)]
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

		[CompetitionBenchmark(9.92, 11.02)]
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

		[CompetitionBenchmark(9.91, 11.21)]
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

		[CompetitionBenchmark(55.16, 67.44)]
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

		[CompetitionBenchmark(9.91, 11.87)]
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

		[CompetitionBenchmark(8.33, 9.40)]
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

		[CompetitionBenchmark(37.43, 45.80)]
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