using System;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;

using BenchmarkDotNet.Attributes;

using CodeJam.PerfTests;
using CodeJam.PerfTests.Configs;

using NUnit.Framework;

namespace CodeJam.DesignDecisions.EmitOrExpressions
{
	/// <summary>
	/// Proofs that Expression.Compile() is inefficient for member access operations.
	/// See http://stackoverflow.com/questions/4211418/why-is-func-created-from-expressionfunc-slower-than-func-declared-direct
	/// See http://stackoverflow.com/questions/5053032/performance-of-compiled-to-delegate-expression
	/// IMPORTANT: Compiled expressions have overhead if your code perrform member access.
	/// RECOMMENDED: Triple check ALL of the following before using Reflection.Emit instead of compiled expressions:
	/// * The body of emitted method is a simple statement that performs member access only.
	/// * The emitted method is on hotpath and there's a perftest that proofs that total boost from 
	///   swtching to il-emit is at least 10%.
	/// * You're not targeting .net Native (.net Native does not include JIT, either way emitted code will be interpreted)
	/// </summary>
	/// <seealso cref="DecisionOperatorsPerfTest"/>
	[TestFixture(Category = CompetitionHelpers.PerfTestCategory + ": Design decisions")]
	[CompetitionBurstMode]
	[CompetitionMeasureAllocations]
	public class DecisionMappingPerfTest
	{
		#region Benchmark helpers
		public class Holder<T>
		{
			public T Value { get; set; }
		}

		private static readonly Func<Holder<int>, Holder<int>, bool> _lessThanDelegate;
		private static readonly Func<Holder<int>, Holder<int>, bool> _lessThanExpression;
		private static readonly Func<Holder<int>, Holder<int>, bool> _lessThanDynamicMethod;
		private static readonly Func<Holder<int>, Holder<int>, bool> _lessThanDynamicMethodAssociated;
		private static readonly Func<Holder<int>, Holder<int>, bool> _lessThanTypeBuilder;
		private static readonly Func<Holder<int>, Holder<int>, bool> _lessThanTbExpression;

		private static void EmitLessThan(ILGenerator g)
		{
			g.Emit(OpCodes.Ldarg_0);
			g.Emit(OpCodes.Callvirt, typeof(Holder<int>).GetProperty(nameof(Holder<int>.Value)).GetMethod);
			g.Emit(OpCodes.Ldarg_1);
			g.Emit(OpCodes.Callvirt, typeof(Holder<int>).GetProperty(nameof(Holder<int>.Value)).GetMethod);
			g.Emit(OpCodes.Clt);
			g.Emit(OpCodes.Ret);
		}

		[MethodImpl(MethodImplOptions.NoInlining)]
		private static bool LessThan(Holder<int> a, Holder<int> b) => a.Value < b.Value;

		static DecisionMappingPerfTest()
		{
			_lessThanDelegate = LessThan;

			Expression<Func<Holder<int>, Holder<int>, bool>> expr = (a, b) => a.Value < b.Value;
			_lessThanExpression = expr.Compile();

			var dynMethod = new DynamicMethod("LessThan", typeof(bool), new[] { typeof(Holder<int>), typeof(Holder<int>) });
			EmitLessThan(dynMethod.GetILGenerator());
			_lessThanDynamicMethod = (Func<Holder<int>, Holder<int>, bool>)dynMethod.CreateDelegate(typeof(Func<Holder<int>, Holder<int>, bool>));

			dynMethod = new DynamicMethod("LessThanAssoc", typeof(bool), new[] { typeof(Holder<int>), typeof(Holder<int>) }, typeof(Holder<int>), true);
			EmitLessThan(dynMethod.GetILGenerator());
			_lessThanDynamicMethodAssociated = (Func<Holder<int>, Holder<int>, bool>)dynMethod.CreateDelegate(typeof(Func<Holder<int>, Holder<int>, bool>));

			var ab = AssemblyBuilder.DefineDynamicAssembly(new AssemblyName("OpsHolder"), AssemblyBuilderAccess.RunAndCollect);
			var mod = ab.DefineDynamicModule("OpsHolder");
			var tb = mod.DefineType("OpsHolder" + nameof(Int32));

			var mLessThan = tb.DefineMethod(
				"LessThan",
				MethodAttributes.Public | MethodAttributes.Static,
				typeof(bool), new[] { typeof(Holder<int>), typeof(Holder<int>) });
			EmitLessThan(mLessThan.GetILGenerator());

			var mLessThanExpr = tb.DefineMethod(
				"LessThanExpr",
				MethodAttributes.Public | MethodAttributes.Static,
				typeof(bool), new[] { typeof(Holder<int>), typeof(Holder<int>) });
			expr.CompileToMethod(mLessThanExpr);

			var t2 = tb.CreateType();
			_lessThanTypeBuilder =
				(Func<Holder<int>, Holder<int>, bool>)t2.GetMethod("LessThan").CreateDelegate(typeof(Func<Holder<int>, Holder<int>, bool>));
			_lessThanTbExpression =
				(Func<Holder<int>, Holder<int>, bool>)t2.GetMethod("LessThanExpr").CreateDelegate(typeof(Func<Holder<int>, Holder<int>, bool>));
		}

		private const int N = 1001;
		private Holder<int>[] _a, _b, _c;

		[Setup]
		public void Setup()
		{
			var rnd = new Random(0);
			_a = new Holder<int>[N];
			_b = new Holder<int>[N];
			_c = new Holder<int>[N];
			for (int i = 0; i < N; i++)
			{
				_a[i] = new Holder<int> { Value = rnd.Next() };
				_b[i] = new Holder<int> { Value = rnd.Next() };
			}
		}
		#endregion

		[Test]
		public void RunDecisionMappingPerfTest() => Competition.Run(this);

		[CompetitionBaseline, GcAllocations(0)]
		public void MinDelegate()
		{
			for (int i = 0; i < N; i++)
			{
				Holder<int> x = _a[i], y = _b[i];
				_c[i] = _lessThanDelegate(x, y) ? x : y;
			}
		}

		[CompetitionBenchmark(0.53, 1.07), GcAllocations(0)]
		public void MinMethod()
		{
			for (int i = 0; i < N; i++)
			{
				Holder<int> x = _a[i], y = _b[i];
				_c[i] = LessThan(x, y) ? x : y;
			}
		}

		[CompetitionBenchmark(0.45, 0.95), GcAllocations(0)]
		public void MinHardcoded()
		{
			for (int i = 0; i < N; i++)
			{
				Holder<int> x = _a[i], y = _b[i];
				_c[i] = x.Value < y.Value ? x : y;
			}
		}

		[CompetitionBenchmark(1.75, 3.73), GcAllocations(0)]
		public void MinExpression()
		{
			for (int i = 0; i < N; i++)
			{
				Holder<int> x = _a[i], y = _b[i];
				_c[i] = _lessThanExpression(x, y) ? x : y;
			}
		}

		[CompetitionBenchmark(2.07, 4.14), GcAllocations(0)]
		public void MinDynamicMethod()
		{
			for (int i = 0; i < N; i++)
			{
				Holder<int> x = _a[i], y = _b[i];
				_c[i] = _lessThanDynamicMethod(x, y) ? x : y;
			}
		}

		[CompetitionBenchmark(0.60, 1.31), GcAllocations(0)]
		public void MinDynamicMethodAssociated()
		{
			for (int i = 0; i < N; i++)
			{
				Holder<int> x = _a[i], y = _b[i];
				_c[i] = _lessThanDynamicMethodAssociated(x, y) ? x : y;
			}
		}

		[CompetitionBenchmark(0.69, 1.26), GcAllocations(0)]
		public void MinTypeBuilder()
		{
			for (int i = 0; i < N; i++)
			{
				Holder<int> x = _a[i], y = _b[i];
				_c[i] = _lessThanTypeBuilder(x, y) ? x : y;
			}
		}

		[CompetitionBenchmark(0.61, 1.27), GcAllocations(0)]
		public void MinTbExpression()
		{
			for (int i = 0; i < N; i++)
			{
				Holder<int> x = _a[i], y = _b[i];
				_c[i] = _lessThanTbExpression(x, y) ? x : y;
			}
		}
	}
}