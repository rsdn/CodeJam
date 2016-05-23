using System;
using System.Collections.Generic;
using System.Linq;

using BenchmarkDotNet.Validators;

namespace BenchmarkDotNet.Running.Competitions.Core
{
	internal class RunStateSlots : IValidator
	{
		// TODO: To ConcurrentDictionary if supported by all targets
		private readonly IDictionary<Type, object> _stateSlots = new Dictionary<Type, object>();

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
		IEnumerable<IValidationError> IValidator.Validate(IList<Benchmark> benchmarks) =>
			Enumerable.Empty<IValidationError>();

		bool IValidator.TreatsWarningsAsErrors => false;
		#endregion
	}
}