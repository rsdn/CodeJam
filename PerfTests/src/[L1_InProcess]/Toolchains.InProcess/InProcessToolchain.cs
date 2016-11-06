using System;

using BenchmarkDotNet.Characteristics;
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
		public static readonly IToolchain Instance = new InProcessToolchain(true);

		/// <summary>The default toolchain instance.</summary>
		public static readonly IToolchain DontLogOutput = new InProcessToolchain(false);

		/// <summary>Initializes a new instance of the <see cref="InProcessToolchain"/> class.</summary>
		/// <param name="logOutput"><c>true</c> if the output should be logged.</param>
		private InProcessToolchain(bool logOutput):this(TimeSpan.FromMinutes(5), logOutput)
		{
		}

		/// <summary>Initializes a new instance of the <see cref="InProcessToolchain"/> class.</summary>
		/// <param name="timeout">Timeout for the run.</param>
		/// <param name="logOutput"><c>true</c> if the output should be logged.</param>
		private InProcessToolchain(TimeSpan timeout, bool logOutput)
		{
			Generator = new InProcessGenerator();
			Builder = new InProcessBuilder();
			Executor = new InProcessExecutor(timeout, logOutput);
		}

		/// <summary>Determines whether the specified benchmark is supported.</summary>
		/// <param name="benchmark">The benchmark.</param>
		/// <param name="logger">The logger.</param>
		/// <param name="resolver">The resolver.</param>
		/// <returns><c>true</c> if the benchmark can be run with the toolchain.</returns>
		public bool IsSupported(Benchmark benchmark, ILogger logger, IResolver resolver) => true;

		/// <summary>Name of the toolchain.</summary>
		/// <value>The name of the toolchain.</value>
		public string Name => nameof(InProcessToolchain);

		/// <summary>The generator.</summary>
		/// <value>The generator.</value>
		public IGenerator Generator { get; }

		/// <summary>The builder.</summary>
		/// <value>The builder.</value>
		public IBuilder Builder { get; }

		/// <summary>The executor.</summary>
		/// <value>The executor.</value>
		public IExecutor Executor { get; }

		#region Overrides of Object
		/// <summary>Returns a <see cref="String" /> that represents this instance.</summary>
		/// <returns>A <see cref="String" /> that represents this instance.</returns>
		public override string ToString() => GetType().Name;
		#endregion
	}
}