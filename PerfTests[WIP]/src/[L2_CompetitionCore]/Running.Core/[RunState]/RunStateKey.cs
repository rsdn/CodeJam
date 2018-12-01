using System;
using System.Linq;

using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Reports;

using JetBrains.Annotations;

namespace CodeJam.PerfTests.Running.Core
{
	/// <summary>
	/// Base class for state slot
	/// </summary>
	public abstract class RunStateKey
	{
		/// <summary>Initializes a new instance of the <see cref="RunStateKey"/> class.</summary>
		/// <param name="clearBeforeEachRun">if set to <c>true</c> the value of the slot is cleaned on each run.</param>
		protected RunStateKey(bool clearBeforeEachRun) => ClearBeforeEachRun = clearBeforeEachRun;

		/// <summary>Gets a value indicating whether  the value of the slot is cleaned on each run.</summary>
		/// <value><c>true</c> if  the value of the slot is cleaned on each run; otherwise, <c>false</c>.</value>
		public bool ClearBeforeEachRun { get; }

		/// <summary>Returns run state object for the current run.</summary>
		/// <value>Run state object for the current run.</value>
		/// <param name="summary">The summary for the current run.</param>
		/// <returns>Run state object for the current run..</returns>
		[NotNull]
		protected object this[[NotNull] Summary summary]
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
		protected object this[[NotNull] IConfig config]
		{
			get
			{
				Code.NotNull(config, nameof(config));

				var slots = config.GetValidators()
					.OfType<RunStateSlots>()
					.FirstOrDefault();

				Code.AssertState(slots != null, "The config is broken. Please do not clean default config validators.");

				return slots.GetSlot(
					this,
					() =>
					{
						var state = CreateState(config);
						Code.BugIf(state == null, "The value factory should not return null values.");
						return state;
					});
			}
		}

		/// <summary>Creates state value for the config.</summary>
		/// <param name="config">The config.</param>
		/// <returns>A new state value for the config.</returns>
		[NotNull]
		protected abstract object CreateState(IConfig config);
	}
}