using System;

namespace CodeJam.PerfTests.Running.Core
{
	/// <summary>
	/// Base class for state slot
	/// </summary>
	public abstract class RunStateKey
	{
		/// <summary>Initializes a new instance of the <see cref="RunStateKey"/> class.</summary>
		/// <param name="clearBeforeEachRun">if set to <c>true</c> the value of the slot is cleaned on each run.</param>
		protected RunStateKey(bool clearBeforeEachRun)
		{
			ClearBeforeEachRun = clearBeforeEachRun;
		}

		/// <summary>Gets a value indicating whether  the value of the slot is cleaned on each run.</summary>
		/// <value><c>true</c> if  the value of the slot is cleaned on each run; otherwise, <c>false</c>.</value>
		public bool ClearBeforeEachRun { get; }
	}
}