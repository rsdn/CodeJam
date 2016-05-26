using System;

using BenchmarkDotNet.Loggers;
using BenchmarkDotNet.Running;
using BenchmarkDotNet.Toolchains.Results;

// ReSharper disable once CheckNamespace

namespace BenchmarkDotNet.Toolchains.InProcess
{
	/// <summary>
	/// Implementation of <seealso cref="IGenerator"/> for in-process benchmarks.
	/// </summary>
	public class InProcessGenerator : IGenerator
	{
		/// <summary>Generates the project for benchmark.</summary>
		/// <param name="benchmark">The benchmark.</param>
		/// <param name="logger">The logger.</param>
		/// <param name="rootArtifactsFolderPath">The root artifacts folder path.</param>
		/// <returns>Generation result.</returns>
		public GenerateResult GenerateProject(Benchmark benchmark, ILogger logger, string rootArtifactsFolderPath) =>
			new GenerateResult(null, true, null);
	}
}