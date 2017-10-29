using System;

using JetBrains.Annotations;

namespace CodeJam.PerfTests.Running.Core
{
	/// <summary>Source of the message.</summary>
	[PublicAPI]
	public enum MessageSource
	{
		/// <summary>The origin of the message is unknown.</summary>
		Unknown,

		/// <summary>The message is reported by benchmark runner.</summary>
		Runner,

		/// <summary>The message is reported by validator.</summary>
		Validator,

		/// <summary>The message is reported by analyser.</summary>
		Analyser,

		/// <summary>The message is reported by diagnoser.</summary>
		Diagnoser,

		/// <summary>The message is reported by exporter.</summary>
		Exporter
	}
}