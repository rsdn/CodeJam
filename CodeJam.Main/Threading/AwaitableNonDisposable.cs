// BASEDON: https://github.com/StephenCleary/AsyncEx AwaitableDisposable class.

using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

using JetBrains.Annotations;

using static CodeJam.Targeting.MethodImplOptionsEx;

namespace CodeJam.Threading
{
	/// <summary>
	/// An awaitable wrapper around a task whose result is <see cref="IDisposable"/>.
	/// The wrapper itself is not <see cref="IDisposable"/>.!--
	/// This prevents usage errors like <code>using (lock.AcquireAsync())</code> when the appropriate usage should be <code>using (await lock.AcquireAsync())</code>.
	/// </summary>
	/// <typeparam name="T">The type of the result of the underlying task.</typeparam>
	[PublicAPI]
	public struct AwaitableNonDisposable<T> where T : IDisposable
	{
		/// <summary>
		/// Performs an implicit conversion from <see cref="Task{T}"/> to <see cref="AwaitableNonDisposable{T}"/>.
		/// </summary>
		public static implicit operator AwaitableNonDisposable<T>(Task<T> task) => new AwaitableNonDisposable<T>(task);

		/// <summary>
		/// The underlying task.
		/// </summary>
		[NotNull] private readonly Task<T> _task;

		/// <summary>
		/// Initializes a new awaitable wrapper around the specified task.
		/// </summary>
		/// <param name="task">The underlying task to wrap. This may not be <c>null</c>.</param>
		[MethodImpl(AggressiveInlining)]
		public AwaitableNonDisposable([NotNull] Task<T> task)
		{
			Code.NotNull(task, nameof(task));

			_task = task;
		}

		/// <summary>
		/// Returns the underlying task.
		/// </summary>
		/// <returns>Underlying task.</returns>
		[NotNull]
		[MethodImpl(AggressiveInlining)]
		public Task<T> AsTask() => _task;

		/// <summary>
		/// Implicit conversion to the underlying task.
		/// </summary>
		/// <param name="source">The awaitable wrapper.</param>
		/// <returns>Underlying task</returns>
		[NotNull]
		[MethodImpl(AggressiveInlining)]
		public static implicit operator Task<T>(AwaitableNonDisposable<T> source) => source.AsTask();

		/// <summary>
		/// Infrastructure. Returns the task awaiter for the underlying task.
		/// </summary>
		/// <returns>Task awaiter for the underlying task.</returns>
		[MethodImpl(AggressiveInlining)]
		public TaskAwaiter<T> GetAwaiter() => _task.GetAwaiter();

		/// <summary>
		/// Infrastructure. Returns a configured task awaiter for the underlying task.
		/// </summary>
		/// <param name="continueOnCapturedContext">Whether to attempt to marshal the continuation back to the captured context.</param>
		/// <returns>A configured task awaiter for the underlying task.</returns>
		[MethodImpl(AggressiveInlining)]
		public ConfiguredTaskAwaitable<T> ConfigureAwait(bool continueOnCapturedContext) =>
			_task.ConfigureAwait(continueOnCapturedContext);
	}
}