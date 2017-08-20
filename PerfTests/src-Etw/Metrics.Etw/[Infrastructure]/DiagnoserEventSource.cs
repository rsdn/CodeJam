using System;
using System.Diagnostics.Tracing;

namespace CodeJam.PerfTests.Metrics.Etw
{
	/// <summary>
	/// An ETW trace source used to mark trace with diagnoser events
	/// </summary>
	/// <seealso cref="EventSource" />
	[EventSource(Name = SourceName, Guid = SourceGuidValue)]
	internal class DiagnoserEventSource : EventSource
	{
		#region Trace constants
		/// <summary>The etw source name</summary>
		public const string SourceName = "CodeJam-PerfTests-DiagnoserEventSource";
		/// <summary>The etw source guid</summary>
		public const string SourceGuidValue = "C86C9494-5309-4B52-B8F6-EE1C5B2A84E7";
		/// <summary>The etw source guid</summary>
		public static readonly Guid SourceGuid = new Guid(SourceGuidValue);

		/// <summary>The name of RunId payload</summary>
		public const string RunIdPayload = "RunId";
		/// <summary>The name of IterationId payload</summary>
		public const string IterationIdPayload = "IterationId";
		#endregion

		/// <summary>The default instance of the trace source</summary>
		public static DiagnoserEventSource Instance = new DiagnoserEventSource();

		// DONTTOUCH: Trace methods should go first, order of methods is important.

		#region Trace methods
		// ReSharper disable InconsistentNaming
		/// <summary>Writes begin trace scope event.</summary>
		/// <param name="RunId">The run identifier.</param>
		/// <param name="IterationId">The iteration identifier.</param>
		public void TraceStarted(Guid RunId, Guid IterationId)
		{
			if (IsEnabled())
				WriteEvent(1, RunId, IterationId);
		}

		/// <summary>Writes end trace scope event.</summary>
		/// <param name="RunId">The run identifier.</param>
		/// <param name="IterationId">The iteration identifier.</param>
		public void TraceStopped(Guid RunId, Guid IterationId)
		{
			if (IsEnabled())
				WriteEvent(2, RunId, IterationId);
		}
		// ReSharper restore InconsistentNaming
		#endregion

	}
}