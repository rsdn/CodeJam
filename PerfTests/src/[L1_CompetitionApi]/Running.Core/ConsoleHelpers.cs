using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using JetBrains.Annotations;

namespace CodeJam.PerfTests.Running.Core
{
	/// <summary>
	/// Console helpers.
	/// </summary>
	[PublicAPI]
	public static class ConsoleHelpers
	{
		private class PreviousOutputHolder
		{
			public PreviousOutputHolder(TextWriter output)
			{
				Output = output;
			}

			public TextWriter Output { get; }
		}

		private static readonly Stack<PreviousOutputHolder> _outputStack = new Stack<PreviousOutputHolder>();

		/// <summary>
		/// Sets the console output in a thread-safe manner.
		/// WARNING: DO use only this method to override the output. Job will fail otherwise.
		/// </summary>
		/// <param name="output">The new console output.</param>
		/// <returns><see cref="IDisposable"/> to restore the output</returns>
		public static IDisposable CaptureConsoleOutput(TextWriter output)
		{
			if (output == null)
				throw new ArgumentNullException(nameof(output));

			var holder = SetOutputCore(output);
			return Disposable.Create(() => RestoreOutputCore(holder));
		}

		private static PreviousOutputHolder SetOutputCore(TextWriter output)
		{
			lock (_outputStack)
			{
				var holder = new PreviousOutputHolder(System.Console.Out);
				System.Console.SetOut(output);
				_outputStack.Push(holder);

				return holder;
			}
		}

		private static void RestoreOutputCore(PreviousOutputHolder output)
		{
			lock (_outputStack)
			{
				var popCount = _outputStack.TakeWhile(t => t != output).Count();
				if (popCount < _outputStack.Count)
				{
					for (var i = 0; i < popCount; i++)
					{
						_outputStack.Pop();
					}

					Code.BugIf(output != _outputStack.Peek(), "Capture output stack disbalanced.");
					System.Console.SetOut(_outputStack.Pop().Output);
				}
			}
		}

		/// <summary>Reports that work is completed and asks user to press any key to continue.</summary>
		public static void ConsoleDoneWaitForConfirmation()
		{
			System.Console.WriteLine();
			System.Console.Write("Done. Press any key to continue...");

			System.Console.ReadKey(true);
			System.Console.WriteLine();
		}
	}
}