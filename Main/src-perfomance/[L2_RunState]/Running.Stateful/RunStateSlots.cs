using System;
using System.Collections.Generic;
using System.Linq;

using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Reports;
using BenchmarkDotNet.Validators;

namespace BenchmarkDotNet.Running.Stateful
{
	internal class RunStateSlots : IValidator
	{
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

	public class RunState<T> where T : new()
	{
		public T this[Summary summary] => this[summary.Config];

		public T this[IConfig config]
		{
			get
			{
				var slots = config.GetValidators()
					.OfType<RunStateSlots>()
					.Single();
				return slots.GetSlot<T>();
			}
		}
	}
}