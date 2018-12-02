using System;

using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Reports;

using JetBrains.Annotations;

namespace CodeJam.PerfTests.Running.Core
{
	/// <summary>Typed slot for run state object.</summary>
	/// <typeparam name="T">The type of run state object.</typeparam>
	public class RunStateKey<T> : RunStateKey where T : class
	{
		private readonly Func<IConfig, T> _valueFactory;

		/// <summary>Initializes a new instance of the <see cref="RunStateKey{T}" /> class.</summary>
		/// <param name="valueFactory">The value factory.</param>
		public RunStateKey([NotNull]Func<IConfig, T> valueFactory) : this(valueFactory, false) { }

		/// <summary>Initializes a new instance of the <see cref="RunStateKey{T}" /> class.</summary>
		/// <param name="valueFactory">The value factory.</param>
		/// <param name="clearBeforeEachRun">if set to <c>true</c> the value of the slot is cleaned on each run.</param>
		public RunStateKey([NotNull] Func<IConfig, T> valueFactory, bool clearBeforeEachRun) : base(clearBeforeEachRun)
		{
			Code.NotNull(valueFactory, nameof(valueFactory));
			_valueFactory = valueFactory;
		}

		/// <summary>Returns run state object for the current run.</summary>
		/// <value>Run state object for the current run.</value>
		/// <param name="summary">The summary for the current run.</param>
		/// <returns>Run state object for the current run..</returns>
		[NotNull]
		public new T this[[NotNull] Summary summary] => (T)base[summary];

		/// <summary>Returns the running state for the current run.</summary>
		/// <value>Run state object for the current run.</value>
		/// <param name="config">The config for the current run.</param>
		/// <returns>Run state object for the current run.</returns>
		[NotNull]
		public new T this[[NotNull] IConfig config] => (T)base[config];

		/// <summary>Creates state value for the config.</summary>
		/// <param name="config">The config.</param>
		/// <returns>A new state value for the config.</returns>
		protected override object CreateState(IConfig config) => _valueFactory(config);
	}
}