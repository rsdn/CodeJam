using System;
using System.Collections.Generic;
using System.Linq;

using BenchmarkDotNet.Running;
using BenchmarkDotNet.Validators;

namespace CodeJam.PerfTests.Running.Core
{
	/// <summary>
	/// Helper type to store the values for the <seealso cref="RunState{T}"/>.
	/// </summary>
	internal class RunStateSlots : IValidator
	{
		// TODO: To ConcurrentDictionary if supported by all targets
		private readonly IDictionary<Type, object> _stateSlots = new Dictionary<Type, object>();

		/// <summary>
		/// Returns the value for the <seealso cref="RunState{T}"/>.
		/// There can be only one value of each type stored so the slot serves as per-run singleton.
		/// </summary>
		/// <typeparam name="T">The type of the running state instance.</typeparam>
		/// <returns>The value for the <seealso cref="RunState{T}"/>.</returns>
		public T GetSlot<T>()
			where T : new()
		{
			T result;
			lock (_stateSlots)
			{
				var key = typeof(T);
				object temp;
				if (_stateSlots.TryGetValue(key, out temp))
				{
					result = (T)temp;
				}
				else
				{
					result = new T();
					_stateSlots.Add(key, result);
				}
			}
			return result;
		}

		#region IValidator stub implementation
		/// <summary>Gets a value indicating whether warnings are treated as errors.</summary>
		/// <value>
		/// <c>true</c> if treats warnings as errors; otherwise, <c>false</c>.
		/// </value>
		bool IValidator.TreatsWarningsAsErrors => false;

		/// <summary>Validates the specified benchmarks (stub implementation, does nothing).</summary>
		/// <param name="benchmarks">The benchmarks to validate.</param>
		/// <returns>Enumerable of validation errors.</returns>
		IEnumerable<IValidationError> IValidator.Validate(IList<Benchmark> benchmarks) =>
			Enumerable.Empty<IValidationError>();
		#endregion
	}
}