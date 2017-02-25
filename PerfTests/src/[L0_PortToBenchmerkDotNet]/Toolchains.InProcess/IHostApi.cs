using System;

// ReSharper disable once CheckNamespace
namespace BenchmarkDotNet.Engines
{
	/// <summary>
	/// IHost API interface
	/// </summary>
	public interface IHostApi
	{
		// TODO: report return value?
		/// <summary>Gets a value indicating whether this instance is diagnoser attached.</summary>
		/// <value>
		/// <c>true</c> if this instance is diagnoser attached; otherwise, <c>false</c>.
		/// </value>
		bool IsDiagnoserAttached { get; }

		/// <summary>Writes the specified message.</summary>
		/// <param name="message">The message.</param>
		void Write(string message);

		/// <summary>Writes the line.</summary>
		void WriteLine();

		/// <summary>Writes the line.</summary>
		/// <param name="message">The message.</param>
		void WriteLine(string message);

		/// <summary>Writes the line.</summary>
		/// <param name="messageFormat">The message format.</param>
		/// <param name="args">The arguments.</param>
		void WriteLine(string messageFormat, params object[] args);

		/// <summary>Prints the specified run results.</summary>
		/// <param name="runResults">The run results.</param>
		void Print(RunResults runResults);

		/// <summary>Befores anything else.</summary>
		void BeforeAnythingElse();

		/// <summary>Afters the setup.</summary>
		void AfterSetup();

		/// <summary>Befores the cleanup.</summary>
		void BeforeCleanup();

		/// <summary>Afters anything else.</summary>
		void AfterAnythingElse();
	}
}