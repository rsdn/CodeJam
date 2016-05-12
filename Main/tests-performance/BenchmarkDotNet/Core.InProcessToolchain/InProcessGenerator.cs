﻿using System;

using BenchmarkDotNet.Loggers;
using BenchmarkDotNet.Running;
using BenchmarkDotNet.Toolchains.Results;

// ReSharper disable CheckNamespace

namespace BenchmarkDotNet.Toolchains
{
	public class InProcessGenerator : IGenerator
	{
		public GenerateResult GenerateProject(Benchmark benchmark, ILogger logger, string rootArtifactsFolderPath) => 
			new GenerateResult("~", true, null);
	}
}