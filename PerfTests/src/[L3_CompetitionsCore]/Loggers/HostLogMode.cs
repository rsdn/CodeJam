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

		/// <summary>
		/// Log error messages, hint messages and messages with <see cref="HostLogger"/> prefixes prefixes.
		/// </summary>
		PrefixedOrErrors,

		/// <summary>Log messages with with <see cref="HostLogger"/> prefixes prefixes only.</summary>
		PrefixedOnly
	}
}