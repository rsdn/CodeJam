using System;

using BenchmarkDotNet.Loggers;
using BenchmarkDotNet.Running;
using BenchmarkDotNet.Toolchains.Results;

// ReSharper disable once CheckNamespace

namespace BenchmarkDotNet.Toolchains.InProcess
{
	public class InProcessGenerator : IGenerator
	{
		public GenerateResult GenerateProject(Benchmark benchmark, ILogger logger, string rootArtifactsFolderPath) =>
			new GenerateResult(null, true, null);
	}
}