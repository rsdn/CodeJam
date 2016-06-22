using System;
using System.Linq;

using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Reports;

using JetBrains.Annotations;

namespace CodeJam.PerfTests.Running.Core
{
	/// <summary>Typed slot for run state object.</summary>
	/// <typeparam name="T">The type of run state object.</typeparam>
	public class RunState<T> where T : class, new()
	{
		/// <summary>Returns run state object for the current run.</summary>
		/// <value>Run state object for the current run.</value>
		/// <param name="summary">The summary for the current run.</param>
		/// <returns>Run state object for the current run..</returns>
		[NotNull]
		public T this[[NotNull] Summary summary]
		{
			get
			{
				Code.NotNull(summary, nameof(summary));

				return this[summary.Config];
			}
		}

		/// <summary>Returns the running state for the current run.</summary>
		/// <value>Run state object for the current run.</value>
		/// <param name="config">The config for the current run.</param>
		/// <returns>Run state object for the current run.</returns>
		[NotNull]
		public T this[[NotNull] IConfig config]
		{
			get
			{
				Code.NotNull(config, nameof(config));

				var slots = config.GetValidators()
					.OfType<RunStateSlots>()
					.First();
				return slots.GetSlot<T>();
			}
		}
	}
}