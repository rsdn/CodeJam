using System;

using BenchmarkDotNet.Loggers;
using BenchmarkDotNet.Running;
using BenchmarkDotNet.Toolchains.Results;

namespace BenchmarkDotNet.Toolchains.InProcess
{
	public class InProcessBuilder : IBuilder
	{
		public BuildResult Build(GenerateResult generateResult, ILogger logger, Benchmark benchmark) =>
			new BuildResult(generateResult, true, null, null);
	}
}