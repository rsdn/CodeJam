using System;
using System.IO;

using BenchmarkDotNet.Running;

// ReSharper disable once CheckNamespace

namespace BenchmarkDotNet.Toolchains
{
	/// <summary>
	/// Base interface to manage the benchmark runs.
	/// </summary>
	public interface IRunnableBenchmark
	{
		// TODO: API that can be used with out-of-process benchmarks.
		// Use Job + Parameters + Target instead of benchmark?
		// TODO: Init() with string[] args?
		/// <summary>Initializes the specified benchmark before <see cref="IRunnableBenchmark.Run"/> call.</summary>
		/// <param name="benchmarkToRun">The benchmark that will be run.</param>
		/// <param name="engineFactoryType">Type of the engine factory.</param>
		/// <param name="output">The writer to redirect the output.</param>
		/// <param name="isDiagnoserAttached"><c>true</c> if there is diagnoser attached.</param>
		void Init(Benchmark benchmarkToRun, Type engineFactoryType, TextWriter output, bool isDiagnoserAttached);

		/// <summary>Runs the benchmark. The <see cref="Init"/> method should be called before calling this.</summary>
		void Run();
	}
}