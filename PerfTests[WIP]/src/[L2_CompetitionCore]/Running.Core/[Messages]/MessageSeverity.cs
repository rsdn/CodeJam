using System;

using JetBrains.Annotations;

namespace CodeJam.PerfTests.Running.Core
{
	// DONTTOUCH: DO NOT change the order of the enum values
	// because is used to compare the severities.
	// DO check usages before changing the enum values.
	/// <summary>Severity of the message.</summary>
	[PublicAPI]
	public enum MessageSeverity
	{
		/// <summary>Verbose message (default).</summary>
		Verbose,

		/// <summary>Informational message.</summary>
		Informational,

		/// <summary>Warning message.</summary>
		Warning,

		/// <summary>Test assertion failed message.</summary>
		TestError,

		/// <summary>Test is not set up correctly.</summary>
		SetupError,

		/// <summary>Test execution failed.</summary>
		ExecutionError
	}
}