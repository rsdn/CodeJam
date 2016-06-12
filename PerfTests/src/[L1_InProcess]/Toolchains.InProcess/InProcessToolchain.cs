using System;

using BenchmarkDotNet.Loggers;
using BenchmarkDotNet.Running;

using JetBrains.Annotations;

// ReSharper disable once CheckNamespace

namespace BenchmarkDotNet.Toolchains.InProcess
{
	/// <summary>
	/// A toolchain to run the benchmarks in-process.
	/// </summary>
	/// <seealso cref="IToolchain"/>
	[PublicAPI]
	public sealed class InProcessToolchain : IToolchain
	{
		/// <summary>The default toolchain instance.</summary>
		public static readonly IToolchain Instance = new InProcessToolchain();

		/// <summary>Determines whether the specified benchmark is supported.</summary>
		/// <param name="benchmark">The benchmark.</param>
		/// <param name="logger">The logger.</param>
		/// <returns><c>true</c> if the benchmark can be run with the toolchain.</returns>
		public bool IsSupported(Benchmark benchmark, ILogger logger) => true;

		/// <summary>Name of the toolchain.</summary>
		/// <value>The name of the toolchain.</value>
		public string Name => nameof(InProcessToolchain);
		/// <summary>The generator.</summary>
		/// <value>The generator.</value>
		public IGenerator Generator { get; } = new InProcessGenerator();
		/// <summary>The builder.</summary>
		/// <value>The builder.</value>
		public IBuilder Builder { get; } = new InProcessBuilder();
		/// <summary>The executor.</summary>
		/// <value>The executor.</value>
		public IExecutor Executor { get; } = new InProcessExecutor();
	}
}