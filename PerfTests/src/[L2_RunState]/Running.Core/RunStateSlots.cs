using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

using BenchmarkDotNet.Running;
using BenchmarkDotNet.Validators;

using JetBrains.Annotations;

namespace CodeJam.PerfTests.Running.Core
{
	/// <summary>
	/// Helper type to store the values for the <see cref="RunState{T}"/>.
	/// </summary>
	internal class RunStateSlots : IValidator
	{
		private readonly ConcurrentDictionary<Type, object> _stateSlots = new ConcurrentDictionary<Type, object>();

		/// <summary>
		/// Value for the <see cref="RunState{T}"/>.
		/// There can be only one value of each type stored as a run state so typed slot works as an singleton storage.
		/// </summary>
		/// <typeparam name="T">The type of the running state instance.</typeparam>
		/// <returns>The value for the <see cref="RunState{T}"/>.</returns>
		[NotNull]
		public T GetSlot<T>() where T : class, new() =>
			(T)_stateSlots.GetOrAdd(typeof(T), t => new T());

		#region IValidator stub implementation
		/// <summary>Gets a value indicating whether warnings are treated as errors.</summary>
		/// <value>
		/// <c>true</c> if treats warnings as errors; otherwise, <c>false</c>.
		/// </value>
		bool IValidator.TreatsWarningsAsErrors => false;

		/// <summary>Validates the specified benchmarks (stub implementation, does nothing).</summary>
		/// <param name="validationParameters">The validation parameters.</param>
		/// <returns>Enumerable of validation errors.</returns>
		IEnumerable<ValidationError> IValidator.Validate(ValidationParameters validationParameters) =>
			Enumerable.Empty<ValidationError>();
		#endregion
	}
}