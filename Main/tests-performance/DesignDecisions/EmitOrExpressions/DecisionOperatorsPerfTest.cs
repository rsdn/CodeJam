using System;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;

using BenchmarkDotNet.Attributes;

using CodeJam.Arithmetic;
using CodeJam.PerfTests;

using NUnit.Framework;

namespace CodeJam.DesignDecisions.EmitOrExpressions
{
	using ComparisonCallback = Func<int, int, bool>;

	/// <summary>
	/// Proofs that Expression.Compile() does provide same level of performance 
	/// as instance-associated delegates provided by Reflection.Emit for simple expressions.
	/// 
	/// IMPORTANT: Compiled expressions have overhead if your code perform member access.
	/// RECOMMENDED: Triple check ALL of the following before using Reflection.Emit instead of compiled expressions:
	/// * The body of emitted method is a simple statement that performs member access only.
	/// * The emitted method is on performance-critical hotpath and there's a perftest that proofs that total boost from 
	///   swtching to il-emit is at least 10%.
	/// * You're not targeting .net Native (.net Native does not include JIT, either way emitted code will be interpreted)
	/// </summary>
	/// <seealso cref="DecisionMappingPerfTest"/>
	[TestFixture(Category = CompetitionHelpers.PerfTestCategory + ": Design decisions")]
	public class DecisionOperatorsPerfTest
	{
		#region Benchmark helpers
		private static readonly ComparisonCallback _lessThanDelegate;
		private static readonly ComparisonCallback _lessThanInstanceDelegate;
		private static readonly ComparisonCallback _lessThanOperators;
		private static readonly ComparisonCallback _lessThanExpression;
		private static readonly ComparisonCallback _lessThanDynamicMethod;
		private static readonly ComparisonCallback _lessThanDynamicMethodAssociated;
		private static readonly ComparisonCallback _lessThanDynamicMethodAssociatedInstance;
		private static readonly ComparisonCallback _lessThanTypeBuilder;
		private static readonly ComparisonCallback _lessThanTypeBuilderInstance;
		private static readonly ComparisonCallback _lessThanTbExpression;

		private static void EmitLessThan(ILGenerator g, bool instnace)
		{
			if (instnace)
			{
				g.Emit(OpCodes.Ldarg_1);
				g.Emit(OpCodes.Ldarg_2);
			}
			else
			{
				g.Emit(OpCodes.Ldarg_0);
				g.Emit(OpCodes.Ldarg_1);
			}
			g.Emit(OpCodes.Clt);
			g.Emit(OpCodes.Ret);
		}

		[MethodImpl(MethodImplOptions.NoInlining)]
		private static bool LessThan(int a, int b) => a < b;


		[MethodImpl(MethodImplOptions.NoInlining)]
		private bool LessThanInstance(int a, int b) => a < b;

		static DecisionOperatorsPerfTest()
		{
			_lessThanDelegate = LessThan;
			_lessThanInstanceDelegate = new DecisionOperatorsPerfTest().LessThanInstance;
			_lessThanOperators = Operators<int>.LessThan;

			Expression<ComparisonCallback> expr = (a, b) => a < b;
			_lessThanExpression = expr.Compile();

			var delegateType = typeof(ComparisonCallback);
			var argType = typeof(int);
			var resultType = typeof(bool);
			var associatedWithType = typeof(DecisionOperatorsPerfTest);

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
				(ComparisonCallback)dynMethod.CreateDelegate(delegateType, new DecisionOperatorsPerfTest());
			#endregion

			#region Type builder
			var ab = AssemblyBuilder.DefineDynamicAssembly(new AssemblyName("Ops"), AssemblyBuilderAccess.RunAndCollect);
			var mod = ab.DefineDynamicModule("Ops");
			var tb = mod.DefineType("Ops" + nameof(Int32));

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
			Assert.True(_lessThanDelegate(1, 2));
			Assert.True(_lessThanInstanceDelegate(1, 2));
			Assert.True(_lessThanOperators(1, 2));
			Assert.True(_lessThanExpression(1, 2));
			Assert.True(_lessThanDynamicMethod(1, 2));
			Assert.True(_lessThanDynamicMethodAssociated(1, 2));
			Assert.True(_lessThanDynamicMethodAssociatedInstance(1, 2));
			Assert.True(_lessThanTypeBuilder(1, 2));
			Assert.True(_lessThanTypeBuilderInstance(1, 2));
			Assert.True(_lessThanTbExpression(1, 2));
		}

		private const int Count = CompetitionRunHelpers.SmallLoopCount;
		private int[] _a, _b, _c;

		[GlobalSetup]
		public void Setup()
		{
			var rnd = new Random(0);
			_a = new int[Count];
			_b = new int[Count];
			_c = new int[Count];
			for (int i = 0; i < Count; i++)
			{
				_a[i] = rnd.Next();
				_b[i] = rnd.Next();
			}
		}
		#endregion

		[Test]
		public void RunDecisionOperatorsPerfTest() => Competition.Run(this);

		[CompetitionBaseline, GcAllocations(0)]
		public void MinDelegate()
		{
			for (int i = 0; i < Count; i++)
			{
				int x = _a[i], y = _b[i];
				_c[i] = _lessThanDelegate(x, y) ? x : y;
			}
		}

		[CompetitionBenchmark(0.33, 0.92)]
		[GcAllocations(0)]
		public void MinInstanceDelegate()
		{
			for (int i = 0; i < Count; i++)
			{
				int x = _a[i], y = _b[i];
				_c[i] = _lessThanInstanceDelegate(x, y) ? x : y;
			}
		}

		[CompetitionBenchmark(0.42, 1.07), GcAllocations(0)]
		public void MinMethod()
		{
			for (int i = 0; i < Count; i++)
			{
				int x = _a[i], y = _b[i];
				_c[i] = LessThan(x, y) ? x : y;
			}
		}

		[CompetitionBenchmark(0.167, 0.650), GcAllocations(0)]
		public void MinHardcoded()
		{
			for (int i = 0; i < Count; i++)
			{
				int x = _a[i], y = _b[i];
				_c[i] = x < y ? x : y;
			}
		}

		[CompetitionBenchmark(0.42, 1.10), GcAllocations(0)]
		public void MinOperators()
		{
			for (int i = 0; i < Count; i++)
			{
				int x = _a[i], y = _b[i];
				_c[i] = _lessThanOperators(x, y) ? x : y;
			}
		}

		[CompetitionBenchmark(0.46, 1.04), GcAllocations(0)]
		public void MinExpression()
		{
			for (int i = 0; i < Count; i++)
			{
				int x = _a[i], y = _b[i];
				_c[i] = _lessThanExpression(x, y) ? x : y;
			}
		}

		[CompetitionBenchmark(0.69, 1.35), GcAllocations(0)]
		public void MinDynamicMethod()
		{
			for (int i = 0; i < Count; i++)
			{
				int x = _a[i], y = _b[i];
				_c[i] = _lessThanDynamicMethod(x, y) ? x : y;
			}
		}

		[CompetitionBenchmark(0.62, 1.35), GcAllocations(0)]
		public void MinDynamicMethodAssociated()
		{
			for (int i = 0; i < Count; i++)
			{
				int x = _a[i], y = _b[i];
				_c[i] = _lessThanDynamicMethodAssociated(x, y) ? x : y;
			}
		}


		[CompetitionBenchmark(0.32, 0.94), GcAllocations(0)]
		public void MinDynamicMethodAssociatedInstance()
		{
			for (int i = 0; i < Count; i++)
			{
				int x = _a[i], y = _b[i];
				_c[i] = _lessThanDynamicMethodAssociatedInstance(x, y) ? x : y;
			}
		}

		[CompetitionBenchmark(0.62, 1.25), GcAllocations(0)]
		public void MinTypeBuilder()
		{
			for (int i = 0; i < Count; i++)
			{
				int x = _a[i], y = _b[i];
				_c[i] = _lessThanTypeBuilder(x, y) ? x : y;
			}
		}

		[CompetitionBenchmark(0.32, 0.93), GcAllocations(0)]
		public void MinTypeBuilderInstance()
		{
			for (int i = 0; i < Count; i++)
			{
				int x = _a[i], y = _b[i];
				_c[i] = _lessThanTypeBuilderInstance(x, y) ? x : y;
			}
		}

		[CompetitionBenchmark(0.62, 1.28), GcAllocations(0)]
		public void MinTbExpression()
		{
			for (int i = 0; i < Count; i++)
			{
				int x = _a[i], y = _b[i];
				_c[i] = _lessThanTbExpression(x, y) ? x : y;
			}
		}
	}
}