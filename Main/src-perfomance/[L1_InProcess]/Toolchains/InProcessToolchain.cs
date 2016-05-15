using System;

using BenchmarkDotNet.Loggers;
using BenchmarkDotNet.Running;

using JetBrains.Annotations;

namespace BenchmarkDotNet.Toolchains
{
	[PublicAPI]
	public sealed class InProcessToolchain : IToolchain
	{
		public static readonly IToolchain Default = new InProcessToolchain();

		public bool IsSupported(Benchmark benchmark, ILogger logger) => true;

		public string Name => nameof(InProcessToolchain);
		public IGenerator Generator { get; } = new InProcessGenerator();
		public IBuilder Builder { get; } = new InProcessBuilder();
		public IExecutor Executor { get; } = new InProcessExecutor();
	}
}