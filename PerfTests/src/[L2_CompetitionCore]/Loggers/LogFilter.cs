using System;

using JetBrains.Annotations;

namespace CodeJam.PerfTests.Loggers
{
	/// <summary>Log filtering mode.</summary>
	[PublicAPI]
	public enum LogFilter
	{
		/// <summary>Log all messages.</summary>
		AllMessages,

		/// <summary>
		/// Log error messages, hint messages and messages with <see cref="FilteringLogger"/> prefixes prefixes.
		/// </summary>
		PrefixedOrErrors,

		/// <summary>Log messages with with <see cref="FilteringLogger"/> prefixes prefixes only.</summary>
		PrefixedOnly
	}
}