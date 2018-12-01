using System;

namespace CodeJam.PerfTests.Metrics.Etw
{
	/// <summary>
	/// Trace event that stores information about begin/end of trace scope
	/// </summary>
	internal class DiagnoserTraceScopeEvent
	{
		/// <summary>Initializes a new instance of the <see cref="DiagnoserTraceScopeEvent"/> class.</summary>
		/// <param name="scopeId">The scope identifier.</param>
		/// <param name="processId">The process identifier.</param>
		public DiagnoserTraceScopeEvent(Guid scopeId, int processId)
		{
			ScopeId = scopeId;
			ProcessId = processId;
		}

		/// <summary>Gets unique scope identifier.</summary>
		/// <value>The scope identifier.</value>
		public Guid ScopeId { get; }

		/// <summary>Gets the process identifier the event was traced for.</summary>
		/// <value>The process identifier the event was traced for.</value>
		public int ProcessId { get; }

		/// <summary>Gets or sets time when the trace scope was started.</summary>
		/// <value>The time when the trace scope was started.</value>
		public TimeSpan? Started { get; set; }

		/// <summary>Gets or sets time when the trace scope was stopped.</summary>
		/// <value>The time when the trace scope was stopped.</value>
		public TimeSpan? Stopped { get; set; }
	}
}