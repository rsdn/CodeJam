using System;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;

using BenchmarkDotNet.Attributes;

using CodeJam.PerfTests;

using JetBrains.Annotations;

using NUnit.Framework;

namespace CodeJam.DesignDecisions.EmitOrExpressions
{
	using HInt32 = DecisionMappingPerfTest.Holder<int>;
	using ComparisonCallback = Func<DecisionMappingPerfTest.Holder<int>, DecisionMappingPerfTest.Holder<int>, bool>;

	/// <summary>
	/// Proofs that Expression.Compile() is inefficient for member access operations.
	/// See http://stackoverflow.com/questions/4211418/why-is-func-created-from-expressionfunc-slower-than-func-declared-direct
	/// See http://stackoverflow.com/questions/5053032/performance-of-compiled-to-delegate-expression
	/// 
	/// IMPORTANT: Compiled expressions have overhead if your code perform member access.
	/// RECOMMENDED: Triple check ALL of the following before using Reflection.Emit instead of compiled expressions:
	/// * The body of emitted method is a simple statement that performs member access only.
	/// * The emitted method is on performance-critical hotpath and there's a perftest that proofs that total boost from 
	///   swtching to il-emit is at least 10%.
	/// * You're not targeting .net Native (.net Native does not include JIT, either way emitted code will be interpreted)
	/// </summary>
	/// <seealso cref="DecisionOperatorsPerfTest"/>
	[TestFixture(Category = CompetitionHelpers.PerfTestCategory + ": Design decisions")]
	public class DecisionMappingPerfTest
	{
		#region Benchmark helpers
		public class Holder<T>
		{
			public Holder(T value)
			{
				Value = value;
			}
			[UsedImplicitly]
			public T Value { get; set; }
		}

		private static readonly ComparisonCallback _lessThanDelegate;
		private static readonly ComparisonCallback _lessThanInstanceDelegate;
		private static readonly ComparisonCallback _lessThanExpression;
		private static readonly ComparisonCallback _lessThanDynamicMethod;
		private static readonly ComparisonCallback _lessThanDynamicMethodAssociated;
		private static readonly ComparisonCallback _lessThanDynamicMethodAssociatedInstance;
		private static readonly ComparisonCallback _lessThanTypeBuilder;
		private static readonly ComparisonCallback _lessThanTypeBuilderInstance;
		private static readonly ComparisonCallback _lessThanTbExpression;

		private static void EmitLessThan(ILGenerator g, bool instnace)
		{
			var getter = typeof(HInt32).GetProperty(nameof(HInt32.Value)).GetMethod;
			if (instnace)
			{
				g.Emit(OpCodes.Ldarg_1);
				g.Emit(OpCodes.Callvirt, getter);
				g.Emit(OpCodes.Ldarg_2);
				g.Emit(OpCodes.Callvirt, getter);
			}
			else
			{
				g.Emit(OpCodes.Ldarg_0);
				g.Emit(OpCodes.Callvirt, getter);
				g.Emit(OpCodes.Ldarg_1);
				g.Emit(OpCodes.Callvirt, getter);
			}
			g.Emit(OpCodes.Clt);
			g.Emit(OpCodes.Ret);
		}

		[MethodImpl(MethodImplOptions.NoInlining)]
		private static bool LessThan(HInt32 a, HInt32 b) => a.Value < b.Value;


		[MethodImpl(MethodImplOptions.NoInlining)]
		private bool LessThanInstance(HInt32 a, HInt32 b) => a.Value < b.Value;

		static DecisionMappingPerfTest()
		{
			_lessThanDelegate = LessThan;
			_lessThanInstanceDelegate = new DecisionMappingPerfTest().LessThanInstance;

			Expression<ComparisonCallback> expr = (a, b) => a.Value < b.Value;
			_lessThanExpression = expr.Compile();

			var delegateType = typeof(ComparisonCallback);
			var argType = typeof(Holder<int>);
			var resultType = typeof(bool);
			var associatedWithType = typeof(DecisionMappingPerfTest);

			#region Dynamic method
			var dynMethod = new DynamicMethod("LessThan", resultType, new[] { argType, argType });
			EmitLessThan(dynMethod.GetILGenerator(), false);
			_lessThanDynamicMethod = (ComparisonCallback)dynMethod.CreateDelegate(delegateType);

			dynMethod = new DynamicMethod("LessThanAssoc", resultType, new[] { argType, argType }, associatedWithType);
			EmitLessThan(dynMethod.GetILGenerator(), false);
			_lessThanDynamicMethodAssociated = (ComparisonCallback)dynMethod.CreateDelegate(delegateType);

			dynMethod = new DynamicMethod(
				"LessThanAssocInstance", resultType,
				new[] { associatedWithType, argType, argType },
				associatedWithType);
			EmitLessThan(dynMethod.GetILGenerator(), true);
			_lessThanDynamicMethodAssociatedInstance =
				(ComparisonCallback)dynMethod.CreateDelegate(delegateType, new DecisionMappingPerfTest());
			#endregion

			#region Type builder
			var ab = AssemblyBuilder.DefineDynamicAssembly(new AssemblyName("OpsHolder"), AssemblyBuilderAccess.RunAndCollect);
			var mod = ab.DefineDynamicModule("OpsHolder");
			var tb = mod.DefineType("OpsHolder" + nameof(Int32));

			var mLessThan = tb.DefineMethod(
				"LessThan",
				MethodAttributes.Public | MethodAttributes.Static,
				resultType, new[] { argType, argType });
			EmitLessThan(mLessThan.GetILGenerator(), false);

			var mLessThanInstance = tb.DefineMethod(
				"LessThanInstance",
				MethodAttributes.Public,
				resultType, new[] { argType, argType });
			EmitLessThan(mLessThanInstance.GetILGenerator(), true);

			var mLessThanExpr = tb.DefineMethod(
				"LessThanExpr",
				MethodAttributes.Public | MethodAttributes.Static,
				resultType, new[] { argType, argType });
			expr.CompileToMethod(mLessThanExpr);

			var t2 = tb.CreateType();
			var inst = Activator.CreateInstance(t2);

			_lessThanTypeBuilder = (ComparisonCallback)t2.GetMethod(mLessThan.Name).CreateDelegate(delegateType);
			_lessThanTypeBuilderInstance = (ComparisonCallback)t2.GetMethod(mLessThanInstance.Name).CreateDelegate(delegateType, inst);
			_lessThanTbExpression =
				(ComparisonCallback)t2.GetMethod(mLessThanExpr.Name).CreateDelegate(delegateType);
			#endregion
		}


		[Test]
		public void TestDelegates()
		{
			var a1 = new HInt32(1);
			var a2 = new HInt32(2);
			Assert.True(_lessThanDelegate(a1, a2));
			Assert.True(_lessThanInstanceDelegate(a1, a2));
			Assert.True(_lessThanExpression(a1, a2));
			Assert.True(_lessThanDynamicMethod(a1, a2));
			Assert.True(_lessThanDynamicMethodAssociated(a1, a2));
			Assert.True(_lessThanDynamicMethodAssociatedInstance(a1, a2));
			Assert.True(_lessThanTypeBuilder(a1, a2));
			Assert.True(_lessThanTypeBuilderInstance(a1, a2));
			Assert.True(_lessThanTbExpression(a1, a2));
		}

		private const int Count = CompetitionHelpers.SmallLoopCount;
		private HInt32[] _a, _b, _c;

		[Setup]
		public void Setup()
		{
			var rnd = new Random(0);
			_a = new HInt32[Count];
			_b = new HInt32[Count];
			_c = new HInt32[Count];
			for (int i = 0; i < Count; i++)
			{
				_a[i] = new HInt32(rnd.Next());
				_b[i] = new HInt32(rnd.Next());
			}
		}
		#endregion

		[Test]
		public void RunDecisionMappingPerfTest() => Competition.Run(this);

		[CompetitionBaseline, GcAllocations(0)]
		public void MinDelegate()
		{
			for (int i = 0; i < Count; i++)
			{
				HInt32 x = _a[i], y = _b[i];
				_c[i] = _lessThanDelegate(x, y) ? x : y;
			}
		}

		[CompetitionBenchmark(0.98, 1.39)]
		[GcAllocations(0)]
		public void MinInstanceDelegate()
		{
			for (int i = 0; i < Count; i++)
			{
				HInt32 x = _a[i], y = _b[i];
				_c[i] = _lessThanInstanceDelegate(x, y) ? x : y;
			}
		}

		[CompetitionBenchmark(0.50, 0.93), GcAllocations(0)]
		public void MinMethod()
		{
			for (int i = 0; i < Count; i++)
			{
				HInt32 x = _a[i], y = _b[i];
				_c[i] = LessThan(x, y) ? x : y;
			}
		}

		[CompetitionBenchmark(0.42, 0.70), GcAllocations(0)]
		public void MinHardcoded()
		{
			for (int i = 0; i < Count; i++)
			{
				HInt32 x = _a[i], y = _b[i];
				_c[i] = x.Value < y.Value ? x : y;
			}
		}

		[CompetitionBenchmark(2.45, 3.91), GcAllocations(0)]
		public void MinExpression()
		{
			for (int i = 0; i < Count; i++)
			{
				HInt32 x = _a[i], y = _b[i];
				_c[i] = _lessThanExpression(x, y) ? x : y;
			}
		}

		[CompetitionBenchmark(2.32, 4.86), GcAllocations(0)]
		public void MinDynamicMethod()
		{
			for (int i = 0; i < Count; i++)
			{
				HInt32 x = _a[i], y = _b[i];
				_c[i] = _lessThanDynamicMethod(x, y) ? x : y;
			}
		}

		[CompetitionBenchmark(0.66, 1.22), GcAllocations(0)]
		public void MinDynamicMethodAssociated()
		{
			for (int i = 0; i < Count; i++)
			{
				HInt32 x = _a[i], y = _b[i];
				_c[i] = _lessThanDynamicMethodAssociated(x, y) ? x : y;
			}
		}


		[CompetitionBenchmark(0.97, 1.60), GcAllocations(0)]
		public void MinDynamicMethodAssociatedInstance()
		{
			for (int i = 0; i < Count; i++)
			{
				HInt32 x = _a[i], y = _b[i];
				_c[i] = _lessThanDynamicMethodAssociatedInstance(x, y) ? x : y;
			}
		}

		[CompetitionBenchmark(0.66, 1.46), GcAllocations(0)]
		public void MinTypeBuilder()
		{
			for (int i = 0; i < Count; i++)
			{
				HInt32 x = _a[i], y = _b[i];
				_c[i] = _lessThanTypeBuilder(x, y) ? x : y;
			}
		}

		[CompetitionBenchmark(0.92, 1.75), GcAllocations(0)]
		public void MinTypeBuilderInstance()
		{
			for (int i = 0; i < Count; i++)
			{
				HInt32 x = _a[i], y = _b[i];
				_c[i] = _lessThanTypeBuilderInstance(x, y) ? x : y;
			}
		}

		[CompetitionBenchmark(0.62, 1.42), GcAllocations(0)]
		public void MinTbExpression()
		{
			for (int i = 0; i < Count; i++)
			{
				HInt32 x = _a[i], y = _b[i];
				_c[i] = _lessThanTbExpression(x, y) ? x : y;
			}
		}
	}
}