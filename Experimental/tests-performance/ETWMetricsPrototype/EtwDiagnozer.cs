using System;
using System.Collections.Concurrent;
using BenchmarkDotNet.Parameters;
using BenchmarkDotNet.Running;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using BenchmarkDotNet.Diagnosers;

using CodeJam;
using CodeJam.Reflection;

using Microsoft.Diagnostics.Tracing;
using Microsoft.Diagnostics.Tracing.Parsers;
using Microsoft.Diagnostics.Tracing.Session;

namespace BenchmarkDotNet.Diagnostics.Windows
{
	public abstract class EtwDiagnoser<TStats> where TStats : new()
	{
		// TODO: to a stateless object with all state stored in the run slot
		protected readonly LogCapture Logger = new LogCapture();
		protected readonly Dictionary<Benchmark, int> BenchmarkToProcess = new Dictionary<Benchmark, int>();
		protected readonly ConcurrentDictionary<int, TStats> StatsPerProcess = new ConcurrentDictionary<int, TStats>();

		protected TraceEventSession Session { get; private set; }

		protected abstract ulong EventType { get; }

		protected abstract string SessionNamePrefix { get; }

		protected void Start(DiagnoserActionParameters parameters)
		{
			Clear();

			BenchmarkToProcess.Add(parameters.Benchmark, parameters.Process.Id);
			StatsPerProcess.TryAdd(parameters.Process.Id, GetInitializedStats(parameters));

			WorkaroundEnsureNativeDlls();

			// TOD: HACK: copy native files into assembly location
			Session = CreateSession(parameters.Benchmark);

			EnableProvider();

			AttachToEvents(Session, parameters.Benchmark);

			// The ETW collection thread starts receiving events immediately, but we only
			// start aggregating them after ProcessStarted is called and we know which process
			// (or processes) we should be monitoring. Communication between the benchmark thread
			// and the ETW collection thread is through the statsPerProcess concurrent dictionary
			// and through the TraceEventSession class, which is thread-safe.
			var task = Task.Factory.StartNew((Action)(() => Session.Source.Process()), TaskCreationOptions.LongRunning);

			// wait until the processing has started, block by then so we don't loose any 
			// information (very important for jit-related things)
			WaitUntilStarted(task);
		}

		//HACK: Fixes https://github.com/Microsoft/perfview/issues/292
		private void WorkaroundEnsureNativeDlls()
		{
			var etwAssembly = typeof(ETWTraceEventSource).Assembly;
			var location = Path.GetDirectoryName(etwAssembly.Location);
			var codebase = etwAssembly.GetAssemblyDirectory();
			if (!location.Equals(codebase, StringComparison.InvariantCultureIgnoreCase))
			{
				DebugCode.BugIf(
					Path.GetFullPath(location).ToUpperInvariant() == Path.GetFullPath(codebase).ToUpperInvariant(),
					"Path.GetFullPath(location).ToUpperInvariant() == Path.GetFullPath(codebase).ToUpperInvariant()");

				CopyDirectoryIfExists(
					Path.Combine(codebase, "amd64"),
					Path.Combine(location, "amd64"));


				CopyDirectoryIfExists(
					Path.Combine(codebase, "x86"),
					Path.Combine(location, "x86"));
			}
		}

		private void CopyDirectoryIfExists(string source, string target, bool overwrite = false)
		{
			source = Path.GetFullPath(source);
			target = Path.GetFullPath(target);

			if (!source.EndsWith("\\") && !source.EndsWith("/") && !source.EndsWith(":"))
				source += "\\";

			if (!target.EndsWith("\\") && !target.EndsWith("/") && !target.EndsWith(":"))
				target += "\\";

			if (!Directory.Exists(source))
				return;


			if (!Directory.Exists(target))
				Directory.CreateDirectory(target);

			foreach (var sourceDirectory in Directory.GetDirectories(source, "*", SearchOption.AllDirectories))
			{
				var targetDirectory = Path.Combine(target, sourceDirectory.Substring(source.Length));
				Directory.CreateDirectory(targetDirectory);
			}


			foreach (var sourceFile in Directory.GetFiles(source, "*", SearchOption.AllDirectories))
			{
				var targetFile = Path.Combine(target, sourceFile.Substring(source.Length));
				if (overwrite || !File.Exists(targetFile))
				{
					File.Copy(sourceFile, targetFile, overwrite);
				}
			}
		}

		protected virtual TStats GetInitializedStats(DiagnoserActionParameters parameters) => new TStats();

		/// <summary>Creates the session.</summary>
		/// <param name="benchmark">The benchmark.</param>
		/// <returns></returns>
		protected virtual TraceEventSession CreateSession(Benchmark benchmark)
					=> new TraceEventSession(KernelTraceEventParser.KernelSessionName);

		protected virtual void EnableProvider()
		{
			Session.EnableProvider(
				ClrTraceEventParser.ProviderGuid,
				TraceEventLevel.Verbose,
				EventType);
		}

		protected abstract void AttachToEvents(TraceEventSession traceEventSession, Benchmark benchmark);

		protected void Stop()
		{
			WaitForDelayedEvents();

			Session.Dispose();
		}

		private void Clear()
		{
			BenchmarkToProcess.Clear();
			StatsPerProcess.Clear();
		}

		private static string GetSessionName(string prefix, Benchmark benchmark, ParameterInstances parameters = null)
		{
			if (parameters != null && parameters.Items.Count > 0)
				return $"{prefix}-{benchmark.FolderInfo}-{parameters.FolderInfo}";
			return $"{prefix}-{benchmark.FolderInfo}";
		}

		private static void WaitUntilStarted(Task task)
		{
			while (task.Status == TaskStatus.Created
				|| task.Status == TaskStatus.WaitingForActivation
				|| task.Status == TaskStatus.WaitingToRun)
			{
				Thread.Sleep(10);
			}
		}

		/// <summary>
		/// ETW real-time sessions receive events with a slight delay. Typically it
		/// shouldn't be more than a few seconds. This increases the likelihood that
		/// all relevant events are processed by the collection thread by the time we
		/// are done with the benchmark.
		/// </summary>
		private static void WaitForDelayedEvents()
		{
			Thread.Sleep(TimeSpan.FromSeconds(3));
		}
	}
}