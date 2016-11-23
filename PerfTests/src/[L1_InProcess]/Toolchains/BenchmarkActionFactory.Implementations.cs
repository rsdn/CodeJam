using System;
using System.Diagnostics.CodeAnalysis;
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
		1. Idle signature should match to the benchmark method signature. If the target method is static, the Idle should be static too.
		 If it is virtual - guess what?
		 TODO: Remove it. Entire idea failed, overhead values are inaccurate. static Idle should be used instead.
		2. Should work under .Net native. This means NO emit or Expression.Compile calls.
		 TODO: We can use expression.Compile for unroll for normal build, but this should be opt-in.
		3. High data locality and no additional allocations.
		 This means NO closures are allowed at all, all state should be stored explicitly as BenchmarkAction's fields.
		4. There can be multiple benchmark actions per single target instance, so
		 target instantiation is not responsibility of the benchmark action.
		5. No additional Jitting when possible. Again, this means _no_ expression trees at all.
		6. No codegen/Compilation. We target in-process benchmark too.
	 */

	// BASEDON: https://github.com/dotnet/BenchmarkDotNet/blob/2edb9adf2e1b0aa7545097bb92bde55830ec4dde/src/BenchmarkDotNet.Core/Running/AsyncMethodInvoker.cs
	/// <summary>Helper class that creates <see cref="BenchmarkAction"/> instances. </summary>
	[SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
	[SuppressMessage("ReSharper", "ClassWithVirtualMembersNeverInherited.Global")]
	[SuppressMessage("ReSharper", "MemberCanBeMadeStatic.Local")]
	public static partial class BenchmarkActionFactory
	{
		/// <summary> Idle method to use</summary>
		public enum IdleTarget
		{
			/// <summary>Use static Idle method.</summary>
			Static,
			/// <summary>Use instance Idle method.</summary>
			Instance,
			/// <summary>Use virtual Idle method.</summary>
			InstanceVirtual
		}

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

			protected static TDelegate ChooseOrCreate<TDelegate>(
				object instance, MethodInfo method, IdleTarget idleTarget,
				TDelegate idleStatic, TDelegate idleInstance, TDelegate idleVirtual)
			{
				TDelegate callback;
				if (method == null)
				{
					switch (idleTarget)
					{
						case IdleTarget.Static:
							callback = idleStatic;
							break;
						case IdleTarget.Instance:
							callback = idleInstance;
							break;
						case IdleTarget.InstanceVirtual:
							callback = idleVirtual;
							break;
						default:
							throw new ArgumentOutOfRangeException(nameof(idleTarget), idleTarget, null);
					}
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

			public BenchmarkActionVoid(object instance, MethodInfo method, IdleTarget idleTarget, int unrollFactor)
			{
				var callback = ChooseOrCreate<Action>(
					instance, method, idleTarget,
					IdleStatic, Idle, IdleVirtual);
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

			private void Idle() { }
			protected virtual void IdleVirtual() { }
			private static void IdleStatic() { }
		}

		internal class BenchmarkAction<T> : BenchmarkActionBase
		{
			private readonly Func<T> _callback;
			private T _result;

			public BenchmarkAction(object instance, MethodInfo method, IdleTarget idleTarget, int unrollFactor)
			{
				var callback = ChooseOrCreate<Func<T>>(
					instance, method, idleTarget,
					IdleStatic, Idle, IdleVirtual);
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

			private T Idle() => default(T);
			protected virtual T IdleVirtual() => default(T);
			private static T IdleStatic() => default(T);

			public override object LastRunResult => _result;
		}

		internal class BenchmarkActionTask : BenchmarkActionBase
		{
			private readonly Func<Task> _startTaskCallback;
			private readonly Action _callback;

			public BenchmarkActionTask(object instance, MethodInfo method, IdleTarget idleTarget, int unrollFactor)
			{
				_startTaskCallback = ChooseOrCreate<Func<Task>>(
					instance, method, idleTarget,
					IdleStatic, Idle, IdleVirtual);
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
			private Task Idle() => _completed;
			protected virtual Task IdleVirtual() => _completed;
			private static Task IdleStatic() => _completed;
		}

		internal class BenchmarkActionTask<T> : BenchmarkActionBase
		{
			private readonly Func<Task<T>> _startTaskCallback;
			private readonly Func<T> _callback;
			private T _result;

			public BenchmarkActionTask(object instance, MethodInfo method, IdleTarget idleTarget, int unrollFactor)
			{
				_startTaskCallback = ChooseOrCreate<Func<Task<T>>>(
					instance, method, idleTarget,
					IdleStatic, Idle, IdleVirtual);
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
			private Task<T> Idle() => _completed;
			protected virtual Task<T> IdleVirtual() => _completed;
			private static Task<T> IdleStatic() => _completed;

			public override object LastRunResult => _result;
		}

		internal class BenchmarkActionValueTask<T> : BenchmarkActionBase
		{
			private readonly Func<ValueTask<T>> _startTaskCallback;
			private readonly Func<T> _callback;
			private T _result;

			public BenchmarkActionValueTask(object instance, MethodInfo method, IdleTarget idleTarget, int unrollFactor)
			{
				_startTaskCallback = ChooseOrCreate<Func<ValueTask<T>>>(
					instance, method, idleTarget,
					IdleStatic, Idle, IdleVirtual);
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

			private ValueTask<T> Idle() => new ValueTask<T>(default(T));
			protected virtual ValueTask<T> IdleVirtual() => new ValueTask<T>(default(T));
			private static ValueTask<T> IdleStatic() => new ValueTask<T>(default(T));

			public override object LastRunResult => _result;
		}
	}
}