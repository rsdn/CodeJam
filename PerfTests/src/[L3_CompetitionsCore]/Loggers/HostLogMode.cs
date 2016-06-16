using System;

using JetBrains.Annotations;

namespace CodeJam.PerfTests.Loggers
{
	/// <summary>Host logging mode.</summary>
	[PublicAPI]
	public enum HostLogMode
	{
		/// <summary>Log all messages.</summary>
		AllMessages,

		/// <summary>Log error messages and messages with HostLogger.LogXyz prefixes.</summary>
		PrefixedAndErrors,

		/// <summary>Log messages with HostLogger.LogXyz prefixes only.</summary>
		PrefixedOnly
	}
}