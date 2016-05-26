using System;

using BenchmarkDotNet.Running;

// ReSharper disable once CheckNamespace

namespace BenchmarkDotNet.Validators
{
	// TODO: reuse the one from Bench.Net
	/// <summary>
	/// The validation error message.
	/// </summary>
	public class ValidationError : IValidationError
	{
		/// <summary>Initializes a new instance of the <see cref="ValidationError"/> class.</summary>
		/// <param name="isCritical">if set to <c>true</c> is critical.</param>
		/// <param name="message">The message.</param>
		/// <param name="benchmark">The benchmark.</param>
		public ValidationError(bool isCritical, string message, Benchmark benchmark = null)
		{
			IsCritical = isCritical;
			Message = message;
			Benchmark = benchmark;
		}

		/// <summary>Gets a value indicating whether this instance is critical.</summary>
		/// <value>
		/// <c>true</c> if this instance is critical; otherwise, <c>false</c>.
		/// </value>
		public bool IsCritical { get; }

		/// <summary>Returns the validation message.</summary>
		/// <value>The validation message.</value>
		public string Message { get; }

		/// <summary>Returns the benchmark validation failed on.</summary>
		/// <value>The benchmark.</value>
		public Benchmark Benchmark { get; }

		/// <summary>Returns a <see cref="string"/> that represents this instance.</summary>
		/// <returns>A <see cref="string"/> that represents this instance.</returns>
		public override string ToString() => Message;
	}
}