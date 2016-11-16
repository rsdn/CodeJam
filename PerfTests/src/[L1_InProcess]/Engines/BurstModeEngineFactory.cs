using System;

using BenchmarkDotNet.Jobs;

// ReSharper disable once CheckNamespace

namespace BenchmarkDotNet.Engines
{
	/// <summary>
	/// Factory for burst mode measurements engine (a lot of runs, measure each).
	/// Recommended for use if call time >> than timer resolution (recommended minimum is 1500 ns).
	/// </summary>
	/// <seealso cref="IEngineFactory"/>
	public class BurstModeEngineFactory : IEngineFactory
	{
		/// <summary>The default instance of <see cref="BurstModeEngineFactory"/></summary>
		public static readonly BurstModeEngineFactory Instance = new BurstModeEngineFactory();

		/// <summary>Creates the <see cref="BurstModeEngine"/>.</summary>
		/// <param name="engineParameters">The engine parameters.</param>
		/// <returns>Instance of <see cref="BurstModeEngine"/>.</returns>
		public IEngine Create(EngineParameters engineParameters)
		{
			if (engineParameters.MainAction == null)
				throw new ArgumentNullException(nameof(engineParameters.MainAction));
			if (engineParameters.IdleAction == null)
				throw new ArgumentNullException(nameof(engineParameters.IdleAction));
			if (engineParameters.TargetJob == null)
				throw new ArgumentNullException(nameof(engineParameters.TargetJob));

			if (engineParameters.IsDiagnoserAttached)
				throw new ArgumentException("Diagnosers are not supported yet.", nameof(engineParameters.IsDiagnoserAttached));

			var targetJob = engineParameters.TargetJob;
			if (!targetJob.HasValue(RunMode.InvocationCountCharacteristic) ||
				!targetJob.HasValue(RunMode.WarmupCountCharacteristic) ||
				!targetJob.HasValue(RunMode.TargetCountCharacteristic))
				throw new ArgumentException(
					$"Please set the {RunMode.InvocationCountCharacteristic.FullId}," +
						$"{RunMode.WarmupCountCharacteristic.FullId} and " +
						$"{RunMode.TargetCountCharacteristic.FullId} values .",
					nameof(engineParameters));

			return new BurstModeEngine(engineParameters);
		}
	}
}