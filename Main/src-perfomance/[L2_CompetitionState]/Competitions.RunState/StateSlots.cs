using System;
using System.Collections.Generic;
using System.Linq;

using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Reports;
using BenchmarkDotNet.Running;
using BenchmarkDotNet.Validators;

namespace BenchmarkDotNet.Competitions.RunState
{
	internal class StateSlots : IValidator
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

		#region IValidator stub inplementation
		IEnumerable<IValidationError> IValidator.Validate(IList<Benchmark> benchmarks) =>
			Enumerable.Empty<IValidationError>();

		bool IValidator.TreatsWarningsAsErrors => false;
		#endregion
	}

	public class StateSlot<T> where T : new()
	{
		public T this[Summary summary] => this[summary.Config];

		public T this[IConfig config]
		{
			get
			{
				var slots = config.GetValidators()
					.OfType<StateSlots>()
					.Single();
				return slots.GetSlot<T>();
			}
		}
	}
}