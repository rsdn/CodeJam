using System;

namespace CodeJam.PerfTests.Running.Core
{
	/// <summary>Behavior for concurrent competition runs.</summary>
	public enum ConcurrentRunBehavior
	{
		/// <summary>Use global (cross-process) lock to prevent concurrent runs.</summary>
		Lock,
		/// <summary>Competition will fail with error.</summary>
		Fail,
		/// <summary>Same as <see cref="Lock"/>: use global (cross-process) lock to prevent concurrent runs.</summary>
		Default = Lock
	}
}