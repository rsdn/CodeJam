using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

using BenchmarkDotNet.Validators;

using JetBrains.Annotations;

namespace CodeJam.PerfTests.Running.Core
{
	/// <summary>
	/// Helper type to store the values for the <see cref="RunState{T}"/>.
	/// </summary>
	internal class RunStateSlots : IValidator
	{
		private readonly ConcurrentDictionary<RunStateKey, object> _stateSlots =
			new ConcurrentDictionary<RunStateKey, object>();

		/// <summary>
		/// Value for the <see cref="RunState{T}"/>.
		/// There can be only one value of each type stored as a run state so typed slot works as an singleton storage.
		/// </summary>
		/// <returns>The value for the <see cref="RunState{T}"/>.</returns>
		[NotNull]
		public object GetSlot(RunStateKey key, Func<object> valueFactory) =>
			_stateSlots.GetOrAdd(key, _ => valueFactory());

		/// <summary>Initializes the slot.</summary>
		/// <param name="key">The key.</param>
		/// <param name="value">The value.</param>
		internal void InitSlot(RunStateKey key, object value)
		{
			var initialized = _stateSlots.TryAdd(key, value);
			Code.AssertState(initialized, "Attempt to overwrite filled state slot.");
		}

		#region IValidator implementation
		/// <summary>Gets a value indicating whether warnings are treated as errors.</summary>
		/// <value>
		/// <c>true</c> if treats warnings as errors; otherwise, <c>false</c>.
		/// </value>
		bool IValidator.TreatsWarningsAsErrors => false;

		/// <summary>Validates the specified benchmarks (cleans per-run slots).</summary>
		/// <param name="validationParameters">The validation parameters.</param>
		/// <returns>Enumerable of validation errors.</returns>
		IEnumerable<ValidationError> IValidator.Validate(ValidationParameters validationParameters)
		{
			foreach (var pair in _stateSlots.ToArray())
			{
				if (pair.Key.ClearBeforeEachRun)
				{
					_stateSlots.TryRemove(pair.Key, out _);
				}
			}

			return Enumerable.Empty<ValidationError>();
		}
		#endregion
	}
}