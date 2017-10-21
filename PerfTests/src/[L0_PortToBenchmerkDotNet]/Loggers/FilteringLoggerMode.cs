using System;

using JetBrains.Annotations;

// ReSharper disable once CheckNamespace

namespace BenchmarkDotNet.Loggers
{
	/// <summary>Log filtering mode.</summary>
	[PublicAPI]
	public enum FilteringLoggerMode
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