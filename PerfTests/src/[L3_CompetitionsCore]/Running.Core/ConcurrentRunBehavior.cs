using System;

namespace CodeJam.PerfTests.Running.Core
{
	/// <summary>Behavior for concurrent competition runs.</summary>
	public enum ConcurrentRunBehavior
	{
		/// <summary>Use global (cross-process) lock to prevent concurrent runs.</summary>
		Lock,
		/// <summary>Just run the competition ignoring the runs.</summary>
		[Obsolete("BenchmarkDotNet does not support concurrent runs. Use at your own risk.")]
		Ignore,
		/// <summary>Competition will fail with error.</summary>
		[Obsolete("BenchmarkDotNet does not support concurrent runs. Use at your own risk.")]
		Warning,
		/// <summary>Competition will fail with error.</summary>
		Fail,
		/// <summary>Same as <see cref="Lock"/>: use global (cross-process) lock to prevent concurrent runs.</summary>
		Default = Lock
	}
}