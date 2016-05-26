using System;
using System.Linq;

using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Reports;

namespace CodeJam.PerfTests.Running.Core
{
	/// <summary>
	/// The typed slot for the running state.
	/// There can be only one value of each type stored in the run state
	/// so the slot serves as per-run singleton.
	/// </summary>
	/// <typeparam name="T">The type of the running state instance.</typeparam>
	public class RunState<T> where T : new()
	{
		/// <summary>Returns the running state for the current run.</summary>
		/// <value>The instance that stores the state.</value>
		/// <param name="summary">The summary for the current run.</param>
		/// <returns>Running state for the current run.</returns>
		public T this[Summary summary]
		{
			get
			{
				Code.NotNull(summary, nameof(summary));
				return this[summary.Config];
			}
		}

		/// <summary>Returns the running state for the current run.</summary>
		/// <value>The instance that stores the state.</value>
		/// <param name="config">The config for the current run.</param>
		/// <returns>Running state for the current run.</returns>
		public T this[IConfig config]
		{
			get
			{
				Code.NotNull(config, nameof(config));

				var slots = config.GetValidators()
					.OfType<RunStateSlots>()
					.Single();
				return slots.GetSlot<T>();
			}
		}
	}
}