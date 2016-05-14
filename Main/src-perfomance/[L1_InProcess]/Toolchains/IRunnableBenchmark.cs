using System;
using System.IO;

using BenchmarkDotNet.Running;

namespace BenchmarkDotNet.Toolchains
{
	public interface IRunnableBenchmark
	{
		// TODO: API that can be used with out-of-process benchmarks
		// TODO: Init() with string[] args?
		void Init(Benchmark benchmarkToRun, TextWriter output);
		void Run();
	}
}