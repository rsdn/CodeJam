using System;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Loggers;
using BenchmarkDotNet.Running;
using BenchmarkDotNet.Toolchains.Results;

// ReSharper disable CheckNamespace

namespace BenchmarkDotNet.Toolchains
{
	public class InProcessExecutor : IExecutor
	{
		public ExecuteResult Execute(
			BuildResult buildResult,
			Benchmark benchmark,
			ILogger logger,
			IDiagnoser diagnoser = null)
		{
			var program = ProgramFactory.CreateInProcessProgram(benchmark);
			program.Init(benchmark);

			// Here be magic.
			throw new NotImplementedException();
		}
	}
}