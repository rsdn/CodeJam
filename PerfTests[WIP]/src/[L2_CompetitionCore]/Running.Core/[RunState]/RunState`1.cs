using System;

namespace CodeJam.PerfTests.Running.Core
{
	/// <summary>Typed slot for run state object.</summary>
	/// <typeparam name="T">The type of run state object.</typeparam>
	public class RunState<T> : RunStateKey<T> where T : class, new()
	{
		/// <summary>Initializes a new instance of the <see cref="RunState{T}"/> class.</summary>
		public RunState() : this(false) { }

		/// <summary>Initializes a new instance of the <see cref="RunState{T}"/> class.</summary>
		/// <param name="clearBeforeEachRun">if set to <c>true</c> the value of the slot is cleaned on each run.</param>
		public RunState(bool clearBeforeEachRun) : base(_ => new T(), clearBeforeEachRun) { }
	}
}