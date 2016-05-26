using System;
using System.IO;

using BenchmarkDotNet.Running;

// ReSharper disable once CheckNamespace

namespace BenchmarkDotNet.Toolchains
{
	public interface IRunnableBenchmark
	{
		// TODO: API that can be used with out-of-process benchmarks. 
		// Use Job + Parameters + Target instead of benchmark?
		// TODO: Init() with string[] args?
		void Init(Benchmark benchmarkToRun, TextWriter output);
		void Run();
	}
}