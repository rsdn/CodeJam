using System;
using System.Diagnostics;
using BenchmarkDotNet.Loggers;

namespace BenchmarkDotNet.Extensions
{
    // we need it public to reuse it in the auto-generated dll
    // but we hide it from intellisense with following attribute
    [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
    public static class ProcessExtensions
    {
		public static void SetPriority(
			this Process process, ProcessPriorityClass priority, ILogger logger)
		{
			try
			{
				process.PriorityClass = priority;
			}
			catch (Exception ex)
			{
				logger.WriteLineError(string.Format("Failed to set up priority {1}. Make sure you have the right permissions. Message: {0}", (object)ex.Message, priority));
			}
		}
    }
}