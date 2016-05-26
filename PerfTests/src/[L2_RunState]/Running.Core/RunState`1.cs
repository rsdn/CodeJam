using System;
using System.Linq;

using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Reports;

namespace CodeJam.PerfTests.Running.Core
{
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