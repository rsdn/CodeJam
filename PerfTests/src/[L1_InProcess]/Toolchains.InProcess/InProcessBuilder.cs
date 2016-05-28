using System;

using BenchmarkDotNet.Loggers;
using BenchmarkDotNet.Running;
using BenchmarkDotNet.Toolchains.Results;

// ReSharper disable once CheckNamespace

namespace BenchmarkDotNet.Toolchains.InProcess
{
	/// <summary>
	/// Implementation of <seealso cref="IBuilder"/> for in-process benchmarks.
	/// </summary>
	public class InProcessBuilder : IBuilder
	{
		/// <summary>Builds the benchmark.</summary>
		/// <param name="generateResult">Generation result.</param>
		/// <param name="logger">The logger.</param>
		/// <param name="benchmark">The benchmark.</param>
		/// <returns>Build result.</returns>
		public BuildResult Build(GenerateResult generateResult, ILogger logger, Benchmark benchmark) =>
			new BuildResult(generateResult, true, null, null);
	}
}