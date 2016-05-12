using System;

using BenchmarkDotNet.Loggers;
using BenchmarkDotNet.Running;

using JetBrains.Annotations;

// ReSharper disable CheckNamespace

namespace BenchmarkDotNet.Toolchains
{
	[PublicAPI]
	public sealed class InProcessToolchain : IToolchain
	{
		public static readonly IToolchain Default = new InProcessToolchain();

		// TODO: check that analyzers can run in-process
		// TODO: check that job matches the environment
		// TODO: check that the target is not static class
		public bool IsSupported(Benchmark benchmark, ILogger logger) => true;

		public string Name => nameof(InProcessToolchain);
		public IGenerator Generator { get; } = new InProcessGenerator();
		public IBuilder Builder { get; } = new InProcessBuilder();
		public IExecutor Executor { get; } = new InProcessExecutor();
	}
}