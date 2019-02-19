// BASEDON: https://github.com/StephenCleary/AsyncEx AwaitableDisposable class.

using System;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;

using JetBrains.Annotations;

namespace CodeJam.Threading
{
    /// <summary>
    /// An awaitable wrapper around a task whose result is <see cref="IDisposable"/>.
	/// The wrapper itself is not <see cref="IDisposable"/>.!--
	/// This prevents usage errors like <code>using (lock.AcquireAsync())</code> when the appropriate usage should be <code>using (await lock.AcquireAsync())</code>.
    /// </summary>
    /// <typeparam name="T">The type of the result of the underlying task.</typeparam>
    public struct AwaitableNonDisposable<T> where T : IDisposable
    {
        /// <summary>
        /// The underlying task.
        /// </summary>
        [NotNull] private readonly Task<T> _task;

        /// <summary>
        /// Initializes a new awaitable wrapper around the specified task.
        /// </summary>
        /// <param name="task">The underlying task to wrap. This may not be <c>null</c>.</param>
        public AwaitableNonDisposable([NotNull] Task<T> task)
        {
			Code.NotNull(task, nameof(task));

            _task = task;
        }

        /// <summary>
        /// Returns the underlying task.
        /// </summary>
        public Task<T> AsTask()
        {
            return _task;
        }

        /// <summary>
        /// Implicit conversion to the underlying task.
        /// </summary>
        /// <param name="source">The awaitable wrapper.</param>
        public static implicit operator Task<T>([NotNull] AwaitableNonDisposable<T> source)
        {
            return source.AsTask();
        }

        /// <summary>
        /// Infrastructure. Returns the task awaiter for the underlying task.
        /// </summary>
        public TaskAwaiter<T> GetAwaiter()
        {
            return _task.GetAwaiter();
        }

        /// <summary>
        /// Infrastructure. Returns a configured task awaiter for the underlying task.
        /// </summary>
        /// <param name="continueOnCapturedContext">Whether to attempt to marshal the continuation back to the captured context.</param>
        public ConfiguredTaskAwaitable<T> ConfigureAwait(bool continueOnCapturedContext)
        {
            return _task.ConfigureAwait(continueOnCapturedContext);
        }
    }
}