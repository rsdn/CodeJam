using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;


// ReSharper disable once CheckNamespace

namespace BenchmarkDotNet.Toolchains
{
	/*
		Design goals of the whole stuff:
		0. Reusable API to call Setup/Clean/Idle/Run action with arbitrary return value and store the result.
			Supported ones are: void, T, Task, Task<T>, ValueTask<T>.
		1. Idle signature should match to the benchmark method signature.
			OBSOLETE: Idle methods should be static to provide accurate results.
			NEW: Idle instance methods are fine if the action is created immediately before the Run.
		2. Should work under .Net native. There's option to skip Expression.Compile calls.
			TODO: normal switch for it.
		3. High data locality and no additional allocations / JIT if possible.
			This means NO closures are allowed at all, all state should be stored explicitly as BenchmarkAction's fields.
			There can be multiple benchmark actions per single target instance,
			so target instantiation is not responsibility of the benchmark action.
		4. No codegen/compilation. We target in-process benchmark too.
	 */
	// BASEDON: https://github.com/dotnet/BenchmarkDotNet/blob/2edb9adf2e1b0aa7545097bb92bde55830ec4dde/src/BenchmarkDotNet.Core/Running/AsyncMethodInvoker.cs
	/// <summary>Helper class that creates <see cref="BenchmarkAction"/> instances. </summary>
	public static partial class BenchmarkActionFactory
	{
		internal class BenchmarkActionBase : BenchmarkAction
		{
			protected static TDelegate Unroll<TDelegate>(TDelegate callback, int unrollFactor)
			{
				if (callback == null)
					throw new ArgumentNullException(nameof(callback));
				if (unrollFactor <= 1)
					return callback;

				// TODO: valid conditional variable instead of NETNATIVE
				// OR: appswitch for Delegate.Combine?
#if NETNATIVE
				return (TDelegate)(object)Delegate.Combine(
					Enumerable.Repeat((Delegate)(object)callback, unrollFactor).ToArray());
#else
				return (TDelegate)(object)Expression.Lambda(
					typeof(TDelegate),
					Expression.Block(
						Enumerable.Repeat(callback, unrollFactor).Select(
							m =>
								Expression.Call(Expression.Constant(m), m.GetType().GetMethod(nameof(Action.Invoke)))) // Delegate call to prevent inlining.
						)).Compile();
#endif
			}

			protected static TDelegate CreateOrIdle<TDelegate>(
				object instance, MethodInfo method, TDelegate idleStaticCallback, TDelegate idleInstanceCallback)
			{
				TDelegate callback;
				if (method == null)
				{
					callback = instance == null ? idleStaticCallback : idleInstanceCallback;
				}
				else if (method.IsStatic)
				{
					callback = (TDelegate)(object)Delegate.CreateDelegate(typeof(TDelegate), method);
				}
				else
				{
					callback = (TDelegate)(object)Delegate.CreateDelegate(typeof(TDelegate), instance, method);
				}
				return callback;
			}
		}

		internal class BenchmarkActionVoid : BenchmarkActionBase
		{
			private readonly Action _callback;

			public BenchmarkActionVoid(object instance, MethodInfo method, int unrollFactor)
			{
				var callback = CreateOrIdle<Action>(instance, method, IdleStatic, IdleInstance);
				callback = Unroll(callback, unrollFactor);

				_callback = callback;
				InvokeSingle = _callback;
				InvokeMultiple = Invoke;
			}

			private void Invoke(long repeatCount)
			{
				var callback = _callback;
				for (long i = 0; i < repeatCount; i++)
				{
					callback();
				}
			}

			private static void IdleStatic() { }
			private void IdleInstance() { }
		}

		internal class BenchmarkAction<T> : BenchmarkActionBase
		{
			private readonly Func<T> _callback;
			private T _result;

			public BenchmarkAction(object instance, MethodInfo method, int unrollFactor)
			{
				var callback = CreateOrIdle<Func<T>>(instance, method, IdleStatic, IdleInstance);
				callback = Unroll(callback, unrollFactor);

				_callback = callback;
				InvokeSingle = Invoke;
				InvokeMultiple = Invoke;
			}

			private void Invoke() => _result = _callback();

			private void Invoke(long repeatCount)
			{
				var callback = _callback;
				var result = default(T);
				for (long i = 0; i < repeatCount; i++)
				{
					result = callback();
				}
				_result = result;
			}

			private static T IdleStatic() => default(T);
			private T IdleInstance() => default(T);

			public override object LastRunResult => _result;
		}

		internal class BenchmarkActionTask : BenchmarkActionBase
		{
			private readonly Func<Task> _startTaskCallback;
			private readonly Action _callback;

			public BenchmarkActionTask(object instance, MethodInfo method, int unrollFactor)
			{
				_startTaskCallback = CreateOrIdle<Func<Task>>(instance, method, IdleStatic, IdleInstance);
				_callback = Unroll<Action>(InvokeStartWait, unrollFactor);

				InvokeSingle = _callback;
				InvokeMultiple = Invoke;
			}

			private void InvokeStartWait() => _startTaskCallback().GetAwaiter().GetResult();

			private void Invoke(long repeatCount)
			{
				var callback = _callback;
				for (long i = 0; i < repeatCount; i++)
				{
					callback();
				}
			}

			private static readonly Task _completed = Task.FromResult((object)null);
			private static Task IdleStatic() => _completed;
			private Task IdleInstance() => _completed;
		}

		internal class BenchmarkActionTask<T> : BenchmarkActionBase
		{
			private readonly Func<Task<T>> _startTaskCallback;
			private readonly Func<T> _callback;
			private T _result;

			public BenchmarkActionTask(object instance, MethodInfo method, int unrollFactor)
			{
				_startTaskCallback = CreateOrIdle<Func<Task<T>>>(instance, method, IdleStatic, IdleInstance);
				_callback = Unroll<Func<T>>(InvokeStartWait, unrollFactor);

				InvokeSingle = Invoke;
				InvokeMultiple = Invoke;
			}

			private T InvokeStartWait() => _startTaskCallback().GetAwaiter().GetResult();

			private void Invoke() => _result = _callback();

			private void Invoke(long repeatCount)
			{
				var callback = _callback;
				var result = default(T);
				for (long i = 0; i < repeatCount; i++)
				{
					result = callback();
				}
				_result = result;
			}

			private static readonly Task<T> _completed = Task.FromResult(default(T));
			private static Task<T> IdleStatic() => _completed;
			private Task<T> IdleInstance() => _completed;

			public override object LastRunResult => _result;
		}

		internal class BenchmarkActionValueTask<T> : BenchmarkActionBase
		{
			private readonly Func<ValueTask<T>> _startTaskCallback;
			private readonly Func<T> _callback;
			private T _result;

			public BenchmarkActionValueTask(object instance, MethodInfo method, int unrollFactor)
			{
				_startTaskCallback = CreateOrIdle<Func<ValueTask<T>>>(instance, method, IdleStatic, IdleInstance);
				_callback = Unroll<Func<T>>(InvokeStartWait, unrollFactor);

				InvokeSingle = Invoke;
				InvokeMultiple = Invoke;
			}

			private T InvokeStartWait() => _startTaskCallback().GetAwaiter().GetResult();

			private void Invoke() => _result = _callback();

			private void Invoke(long repeatCount)
			{
				var callback = _callback;
				var result = default(T);
				for (long i = 0; i < repeatCount; i++)
				{
					result = callback();
				}
				_result = result;
			}

			private static ValueTask<T> IdleStatic() => new ValueTask<T>(default(T));
			private static ValueTask<T> IdleInstance() => new ValueTask<T>(default(T));

			public override object LastRunResult => _result;
		}
	}
}