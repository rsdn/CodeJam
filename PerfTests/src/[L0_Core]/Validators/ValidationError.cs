using System;

using BenchmarkDotNet.Running;

// ReSharper disable once CheckNamespace

namespace BenchmarkDotNet.Validators
{
	// TODO: reuse the one from Bench.Net
	public class ValidationError : IValidationError
	{
		public ValidationError(bool isCritical, string message, Benchmark benchmark = null)
		{
			IsCritical = isCritical;
			Message = message;
			Benchmark = benchmark;
		}

		public bool IsCritical { get; }

		public string Message { get; }

		public Benchmark Benchmark { get; }

		public override string ToString() => Message;
	}
}