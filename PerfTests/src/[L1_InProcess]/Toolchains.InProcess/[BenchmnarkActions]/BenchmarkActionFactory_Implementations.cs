using System;
using System.Reflection;
using System.Threading.Tasks;

// ReSharper disable once CheckNamespace

namespace BenchmarkDotNet.Toolchains.InProcess
{
	/*
        Design goals of the whole stuff: check the comments for BenchmarkActionBase.
     */

	// DONTTOUCH: Be VERY CAREFUL when changing the code.
	// Please, ensure that the implementation is in sync with content of BenchmarkProgram.txt

	/// <summary>Helper class that creates <see cref="BenchmarkAction"/> instances. </summary>
	public static partial class BenchmarkActionFactory
	{
		internal class BenchmarkActionVoid : BenchmarkActionBase
		{
			private readonly Action _callback;
			private readonly Action _unrolledCallback;

			public BenchmarkActionVoid(
				object instance, MethodInfo method, BenchmarkActionCodegen codegenMode, int unrollFactor)
			{
				_callback = CreateMainOrIdle<Action>(instance, method, IdleStatic, IdleInstance);
				InvokeSingle = _callback;

				if (UseFallbackCode(codegenMode, unrollFactor))
				{
					_unrolledCallback = Unroll(_callback, unrollFactor);
					InvokeMultiple = InvokeMultipleHardcoded;
				}
				else
				{
					InvokeMultiple = EmitInvokeMultiple(this, nameof(_callback), null, unrollFactor);
				}
			}

			private static void IdleStatic() { }
			private void IdleInstance() { }

			private void InvokeMultipleHardcoded(long repeatCount)
			{
				for (long i = 0; i < repeatCount; i++)
					_unrolledCallback();
			}
		}

		internal class BenchmarkAction<T> : BenchmarkActionBase
		{
			private readonly Func<T> _callback;
			private readonly Func<T> _unrolledCallback;
			private T _result;

			public BenchmarkAction(
				object instance, MethodInfo method, BenchmarkActionCodegen codegenMode, int unrollFactor)
			{
				_callback = CreateMainOrIdle<Func<T>>(instance, method, IdleStatic, IdleInstance);
				InvokeSingle = InvokeSingleHardcoded;

				if (UseFallbackCode(codegenMode, unrollFactor))
				{
					_unrolledCallback = Unroll(_callback, unrollFactor);
					InvokeMultiple = InvokeMultipleHardcoded;
				}
				else
				{
					InvokeMultiple = EmitInvokeMultiple(this, nameof(_callback), nameof(_result), unrollFactor);
				}
			}

			private static T IdleStatic() => default(T);
			private T IdleInstance() => default(T);

			private void InvokeSingleHardcoded() => _result = _callback();

			private void InvokeMultipleHardcoded(long repeatCount)
			{
				for (long i = 0; i < repeatCount; i++)
					_result = _unrolledCallback();
			}

			public override object LastRunResult => _result;
		}

		internal class BenchmarkActionTask : BenchmarkActionBase
		{
			private readonly Func<Task> _startTaskCallback;
			private readonly Action _callback;
			private readonly Action _unrolledCallback;

			public BenchmarkActionTask(
				object instance, MethodInfo method, BenchmarkActionCodegen codegenMode, int unrollFactor)
			{
				_startTaskCallback = CreateMainOrIdle<Func<Task>>(instance, method, IdleStatic, IdleInstance);
				_callback = ExecuteBlocking;
				InvokeSingle = _callback;

				if (UseFallbackCode(codegenMode, unrollFactor))
				{
					_unrolledCallback = Unroll(_callback, unrollFactor);
					InvokeMultiple = InvokeMultipleHardcoded;
				}
				else
				{
					InvokeMultiple = EmitInvokeMultiple(this, nameof(_callback), null, unrollFactor);
				}
			}

			// can't use Task.CompletedTask here because it's new in .NET 4.6 (we target 4.5)
			private static readonly Task _completed = Task.FromResult((object)null);

			private static Task IdleStatic() => _completed;
			private Task IdleInstance() => _completed;

			private void ExecuteBlocking() => _startTaskCallback.Invoke().GetAwaiter().GetResult();

			private void InvokeMultipleHardcoded(long repeatCount)
			{
				for (long i = 0; i < repeatCount; i++)
					_unrolledCallback();
			}
		}

		internal class BenchmarkActionTask<T> : BenchmarkActionBase
		{
			private readonly Func<Task<T>> _startTaskCallback;
			private readonly Func<T> _callback;
			private readonly Func<T> _unrolledCallback;
			private T _result;

			public BenchmarkActionTask(
				object instance, MethodInfo method, BenchmarkActionCodegen codegenMode, int unrollFactor)
			{
				_startTaskCallback = CreateMainOrIdle<Func<Task<T>>>(instance, method, IdleStatic, IdleInstance);
				_callback = ExecuteBlocking;
				InvokeSingle = InvokeSingleHardcoded;

				if (UseFallbackCode(codegenMode, unrollFactor))
				{
					_unrolledCallback = Unroll(_callback, unrollFactor);
					InvokeMultiple = InvokeMultipleHardcoded;
				}
				else
				{
					InvokeMultiple = EmitInvokeMultiple(this, nameof(_callback), nameof(_result), unrollFactor);
				}
			}

			private static readonly Task<T> _completed = Task.FromResult(default(T));
			private static Task<T> IdleStatic() => _completed;
			private Task<T> IdleInstance() => _completed;

			private T ExecuteBlocking() => _startTaskCallback().GetAwaiter().GetResult();

			private void InvokeSingleHardcoded() => _result = _callback();

			private void InvokeMultipleHardcoded(long repeatCount)
			{
				for (long i = 0; i < repeatCount; i++)
					_result = _unrolledCallback();
			}

			public override object LastRunResult => _result;
		}

		internal class BenchmarkActionValueTask<T> : BenchmarkActionBase
		{
			private readonly Func<ValueTask<T>> _startTaskCallback;
			private readonly Func<T> _callback;
			private readonly Func<T> _unrolledCallback;
			private T _result;

			public BenchmarkActionValueTask(
				object instance, MethodInfo method, BenchmarkActionCodegen codegenMode, int unrollFactor)
			{
				_startTaskCallback = CreateMainOrIdle<Func<ValueTask<T>>>(instance, method, IdleStatic, IdleInstance);
				_callback = ExecuteBlocking;
				InvokeSingle = InvokeSingleHardcoded;

				if (UseFallbackCode(codegenMode, unrollFactor))
				{
					_unrolledCallback = Unroll(_callback, unrollFactor);
					InvokeMultiple = InvokeMultipleHardcoded;
				}
				else
				{
					InvokeMultiple = EmitInvokeMultiple(this, nameof(_callback), nameof(_result), unrollFactor);
				}
			}

			private static ValueTask<T> IdleStatic() => new ValueTask<T>(default(T));
			private ValueTask<T> IdleInstance() => new ValueTask<T>(default(T));

			private T ExecuteBlocking() => _startTaskCallback().GetAwaiter().GetResult();

			private void InvokeSingleHardcoded() => _result = _callback();

			private void InvokeMultipleHardcoded(long repeatCount)
			{
				for (long i = 0; i < repeatCount; i++)
					_result = _unrolledCallback();
			}

			public override object LastRunResult => _result;
		}
	}
}