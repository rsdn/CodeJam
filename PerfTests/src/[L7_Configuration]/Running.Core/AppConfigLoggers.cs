using System;

using JetBrains.Annotations;

namespace CodeJam.PerfTests.Running.Core
{
	/// <summary>Loggers for the competition.</summary>
	[Flags]
	[PublicAPI]
	// DONTTOUCH: check AppConfigHelpers log-related constants before renaming the enum values.
	public enum AppConfigLoggers
	{
		/// <summary>Do not log anything.</summary>
		None = 0x0,

		/// <summary>Add important-level loggers.</summary>
		ImportantOnly = 0x1,

		/// <summary>Add detailed logger.</summary>
		Detailed = 0x2,

		/// <summary>Use both important-level and full loggers.</summary>
		Both = ImportantOnly | Detailed
	}
}