using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading;

using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Characteristics;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Loggers;
using BenchmarkDotNet.Order;
using BenchmarkDotNet.Reports;
using BenchmarkDotNet.Running;

using CodeJam;
using CodeJam.Reflection;

using JetBrains.Annotations;

// ReSharper disable once CheckNamespace

namespace BenchmarkDotNet.Helpers
{
	// TODO: move to different classes
	/// <summary>
	/// Helper methods for benchmark infrastructure.
	/// </summary>
	[PublicAPI]
	[SuppressMessage("ReSharper", "ArrangeBraces_using")]
	public static class BenchmarkHelpers
	{
		#region Benchmark-related
		/// <summary>Returns benchmark types from the assembly.</summary>
		/// <param name="assembly">The assembly to get benchmarks from.</param>
		/// <returns>Benchmark types from the assembly</returns>
		public static Type[] GetBenchmarkTypes([NotNull] Assembly assembly) =>
			// Use reflection for a more maintainable way of creating the benchmark switcher,
			// Benchmarks are listed in namespace order first (e.g. BenchmarkDotNet.Samples.CPU,
			// BenchmarkDotNet.Samples.IL, etc) then by name, so the output is easy to understand.
			assembly
				.GetTypes()
				.Where(
					t => t.GetMethods(BindingFlags.Instance | BindingFlags.Public)
						.Any(m => m.GetCustomAttributes(true).OfType<BenchmarkAttribute>().Any()))
				.Where(t => !t.GetTypeInfo().IsGenericType)
				.OrderBy(t => t.Namespace)
				.ThenBy(t => t.Name)
				.ToArray();

		/// <summary>Creates read-only wrapper for the config.</summary>
		/// <param name="config">The config to wrap.</param>
		/// <returns>Read-only wrapper for the config.</returns>
		public static IConfig AsReadOnly(this IConfig config) => new ReadOnlyConfig(config);

		#region Selects
		/// <summary>Returns jobs from the benchmarks.</summary>
		/// <param name="benchmarks">The benchmarks to select jobs from.</param>
		/// <returns>Jobs from the benchmarks.</returns>
		// ReSharper disable once ReturnTypeCanBeEnumerable.Global
		public static IOrderedEnumerable<Job> GetJobs([NotNull] this IEnumerable<Benchmark> benchmarks) =>
			benchmarks
				.Select(d => d.Job)
				.Distinct()
				.OrderBy(d => d.DisplayInfo);

		/// <summary>Returns benchmarks for the summary sorted by execution order.</summary>
		/// <param name="summary">Summary for the run.</param>
		/// <returns>Benchmarks for the summary.</returns>
		public static IEnumerable<Benchmark> GetExecutionOrderBenchmarks([NotNull] this Summary summary)
		{
			var orderProvider = summary.Config.GetOrderProvider() ?? DefaultOrderProvider.Instance;
			return orderProvider.GetExecutionOrder(summary.Benchmarks);
		}

		/// <summary>Returns benchmarks for the summary sorted by display order.</summary>
		/// <param name="summary">Summary for the run.</param>
		/// <returns>Benchmarks for the summary.</returns>
		public static IEnumerable<Benchmark> GetSummaryOrderBenchmarks([NotNull] this Summary summary)
		{
			var orderProvider = summary.Config.GetOrderProvider() ?? DefaultOrderProvider.Instance;
			return orderProvider.GetSummaryOrder(summary.Benchmarks, summary);
		}

		/// <summary>Returns the baseline for the benchmark.</summary>
		/// <param name="summary">Summary for the run.</param>
		/// <param name="benchmark">The benchmark.</param>
		/// <returns>Baseline for the benchmark</returns>
		public static Benchmark TryGetBaseline(
			[NotNull] this Summary summary,
			[NotNull] Benchmark benchmark) =>
				summary.Benchmarks
					.Where(b => b.Job.DisplayInfo == benchmark.Job.DisplayInfo)
					.Where(b => b.Parameters.DisplayInfo == benchmark.Parameters.DisplayInfo)
					.FirstOrDefault(b => b.Target.Baseline);

		/// <summary>Returns the report for the benchmark or <c>null</c> if there's no report.</summary>
		/// <param name="summary">Summary for the run.</param>
		/// <param name="benchmark">The benchmark.</param>
		/// <returns>The report for the benchmark or <c>null</c> if there's no report.</returns>
		public static BenchmarkReport TryGetBenchmarkReport(
			[NotNull] this Summary summary,
			[NotNull] Benchmark benchmark) =>
				summary[benchmark];
		#endregion

		/// <summary>
		/// Determines whether the characteristic has influence on job execution.
		/// </summary>
		/// <param name="characteristic">The characteristic.</param>
		/// <param name="includeIgnoreOnApply">Include ignorable.</param>
		/// <returns>
		/// <c>true</c> if the characteristic has influence on job execution.
		/// </returns>
		public static bool DeterminesBehavior(this Characteristic characteristic, bool includeIgnoreOnApply = false) =>
			!characteristic.HasChildCharacteristics && (includeIgnoreOnApply || !characteristic.IgnoreOnApply);

		/// <summary>Gets the value of the current TimeSpan structure expressed in nanoseconds.</summary>
		/// <param name="timeSpan">The timespan.</param>
		/// <returns>The total number of nanoseconds represented by this instance.</returns>
		public static double TotalNanoseconds(this TimeSpan timeSpan) => timeSpan.Ticks * (1.0e9 / TimeSpan.TicksPerSecond);
		#endregion

		#region Loggers
		private const int SeparatorLength = 42;

		/// <summary>Helper method that writes separator log line.</summary>
		/// <param name="logger">The logger.</param>
		public static void WriteSeparatorLine([NotNull] this ILogger logger) =>
			WriteSeparatorLine(logger, null, false);

		/// <summary>Helper method that writes separator log line.</summary>
		/// <param name="logger">The logger.</param>
		/// <param name="header">The separator line text.</param>
		public static void WriteSeparatorLine([NotNull] this ILogger logger, [CanBeNull] string header) =>
			WriteSeparatorLine(logger, header, false);

		/// <summary>Helper method that writes separator log line.</summary>
		/// <param name="logger">The logger.</param>
		/// <param name="header">The separator line text.</param>
		/// <param name="topHeader">Write top-level header.</param>
		public static void WriteSeparatorLine(
			[NotNull] this ILogger logger, [CanBeNull] string header, bool topHeader)
		{
			var separatorChar = topHeader ? '=' : '-';
			var logKind = topHeader ? LogKind.Header : LogKind.Help;

			var result = new StringBuilder(SeparatorLength);

			if (!string.IsNullOrEmpty(header))
			{
				var prefixLength = (SeparatorLength - header.Length - 2) / 2;
				if (prefixLength > 0)
				{
					result.Append(separatorChar, prefixLength);
				}
				result.Append(' ').Append(header).Append(' ');
			}

			var suffixLength = SeparatorLength - result.Length;
			if (suffixLength > 0)
			{
				result.Append(separatorChar, suffixLength);
			}

			logger.WriteLine();
			logger.WriteLine(logKind, result.ToString());
		}
		#endregion

		#region Reflection
		/// <summary>Checks that the assembly is build in debug mode.</summary>
		/// <param name="assembly">The assembly.</param>
		/// <returns><c>true</c> if the assembly was build with optimizations disabled.</returns>
		public static bool IsDebugAssembly([NotNull] this Assembly assembly)
		{
			if (assembly == null)
				throw new ArgumentNullException(nameof(assembly));

			var optAtt = (DebuggableAttribute)Attribute.GetCustomAttribute(assembly, typeof(DebuggableAttribute));
			return optAtt != null && optAtt.IsJITOptimizerDisabled;
		}

		/// <summary>Gets the short form of assembly qualified type name.</summary>
		/// <param name="type">The type to get the name for.</param>
		/// <returns>The short form of assembly qualified type name.</returns>
		public static string GetShortAssemblyQualifiedName([NotNull] this Type type) =>
			type.FullName + ", " + type.Assembly.GetName().Name;

		/// <summary>
		/// Performs search for metadata attribute in the following order:
		/// * member attributes (if the <paramref name="attributeProvider"/> is member of the type)
		/// * type attributes (if the <paramref name="attributeProvider"/> is type or member of the type)
		/// * container type attributes (if the type is nested type)
		/// * assembly attributes.
		/// </summary>
		/// <typeparam name="TAttribute">Type of the attribute.</typeparam>
		/// <param name="attributeProvider">Metadata attribute source.</param>
		/// <returns>Metadata attributes.</returns>
		public static TAttribute TryGetMetadataAttribute<TAttribute>(
			[NotNull] this ICustomAttributeProvider attributeProvider)
			where TAttribute : class =>
				GetMetadataAttributes<TAttribute>(attributeProvider).FirstOrDefault();

		/// <summary>
		/// Returns metadata attributes in the following order:
		/// * member attributes (if the <paramref name="attributeProvider"/> is member of the type)
		/// * type attributes (if the <paramref name="attributeProvider"/> is type or member of the type)
		/// * container type attributes (if the type is nested type)
		/// * assembly attributes.
		/// </summary>
		/// <typeparam name="TAttribute">Type of the attribute.</typeparam>
		/// <param name="attributeProvider">Metadata attribute source.</param>
		/// <returns>Metadata attributes.</returns>
		public static IEnumerable<TAttribute> GetMetadataAttributes<TAttribute>(
			[NotNull] this ICustomAttributeProvider attributeProvider)
			where TAttribute : class
		{
			if (attributeProvider == null)
				throw new ArgumentNullException(nameof(attributeProvider));

			return GetMetadataAttributesDispatch<TAttribute>(attributeProvider);
		}

		private static IEnumerable<TAttribute> GetMetadataAttributesDispatch<TAttribute>(
			ICustomAttributeProvider attributeProvider)
			where TAttribute : class
		{
			var type = attributeProvider as Type;
			if (type != null)
				return type.GetMetadataAttributesForType<TAttribute>();

			var member = attributeProvider as MemberInfo;
			if (member != null)
				return member.GetMetadataAttributesForMember<TAttribute>();

			return attributeProvider.GetMetadataAttributesCore<TAttribute>();
		}

		private static IEnumerable<TAttribute> GetMetadataAttributesCore<TAttribute>(
			this ICustomAttributeProvider attributeProvider)
			where TAttribute : class
			=>
				attributeProvider.GetCustomAttributes(typeof(TAttribute), true)
					.Cast<TAttribute>()
					.OrderBy(t => t.GetType().Name)
					.ThenBy(t => t.GetType().AssemblyQualifiedName);

		private static IEnumerable<TAttribute> GetMetadataAttributesForType<TAttribute>(
			this Type type)
			where TAttribute : class
		{
			var tempType = type;
			while (tempType != null)
			{
				var typeAttributes = tempType.GetMetadataAttributesCore<TAttribute>();
				foreach (var attribute in typeAttributes)
				{
					yield return attribute;
				}

				tempType = tempType.DeclaringType;
			}

			foreach (var attribute in type.Assembly.GetMetadataAttributesCore<TAttribute>())
			{
				yield return attribute;
			}
		}

		private static IEnumerable<TAttribute> GetMetadataAttributesForMember<TAttribute>(
			this MemberInfo member)
			where TAttribute : class
		{
			foreach (var attribute in member.GetMetadataAttributesCore<TAttribute>())
			{
				yield return attribute;
			}

			foreach (var attribute in member.DeclaringType.GetMetadataAttributesForType<TAttribute>())
			{
				yield return attribute;
			}
		}
		#endregion

		#region Priority & affinity
		/// <summary>Changes the priority of the thread.</summary>
		/// <param name="thread">The target thread.</param>
		/// <param name="priority">The priority.</param>
		/// <param name="logger">The logger.</param>
		public static bool SetPriority(
			[NotNull] this Thread thread,
			ThreadPriority priority,
			[NotNull] ILogger logger)
		{
			if (thread == null)
				throw new ArgumentNullException(nameof(thread));
			if (logger == null)
				throw new ArgumentNullException(nameof(logger));

			try
			{
				thread.Priority = priority;
				return true;
			}
			catch (Exception ex)
			{
				logger.WriteLineError(
					$"// ! Failed to set up priority {priority} for thread {thread}. Make sure you have the right permissions. Message: {ex.Message}");
			}
			return false;
		}

		/// <summary>Changes the priority of the process.</summary>
		/// <param name="process">The target process.</param>
		/// <param name="priority">The priority.</param>
		/// <param name="logger">The logger.</param>
		public static bool SetPriority(
			[NotNull] this Process process,
			ProcessPriorityClass priority,
			[NotNull] ILogger logger)
		{
			if (process == null)
				throw new ArgumentNullException(nameof(process));
			if (logger == null)
				throw new ArgumentNullException(nameof(logger));

			try
			{
				process.PriorityClass = priority;
				return true;
			}
			catch (Exception ex)
			{
				logger.WriteLineError(
					$"// ! Failed to set up priority {priority} for process {process}. Make sure you have the right permissions. Message: {ex.Message}");
			}
			return false;
		}

		/// <summary>Changes the cpu affinity of the process.</summary>
		/// <param name="process">The target process.</param>
		/// <param name="processorAffinity">The processor affinity.</param>
		/// <param name="logger">The logger.</param>
		public static bool SetAffinity(
			[NotNull] this Process process,
			IntPtr processorAffinity,
			[NotNull] ILogger logger)
		{
			if (process == null)
				throw new ArgumentNullException(nameof(process));
			if (logger == null)
				throw new ArgumentNullException(nameof(logger));

			try
			{
				var cpuMask = (1 << Environment.ProcessorCount) - 1;
				process.ProcessorAffinity = new IntPtr(processorAffinity.ToInt32() & cpuMask);
				return true;
			}
			catch (Exception ex)
			{
				logger.WriteLineError(
					$"// ! Failed to set up processor affinity 0x{(long)processorAffinity:X} for process {process}. Make sure you have the right permissions. Message: {ex.Message}");
			}
			return false;
		}

		/// <summary>Sets highest possible priority and Changes the cpu affinity of the process.</summary>
		/// <param name="processorAffinity">The processor affinity.</param>
		/// <param name="logger">The logger.</param>
		public static IDisposable SetupHighestPriorityScope(
			[CanBeNull] IntPtr? processorAffinity,
			[NotNull] ILogger logger)
		{
			var process = Process.GetCurrentProcess();
			var thread = Thread.CurrentThread;

			var oldThreadPriority = thread.Priority;
			var oldProcdessPriority = process.PriorityClass;
			var oldAffinity = process.ProcessorAffinity;

			process.SetPriority(ProcessPriorityClass.RealTime, logger);
			if (processorAffinity.HasValue)
			{
				process.SetAffinity(processorAffinity.Value, logger);
			}
			thread.SetPriority(ThreadPriority.Highest, logger);

			return Disposable.Create(
				() =>
				{
					thread.SetPriority(oldThreadPriority, logger);
					if (processorAffinity.HasValue)
					{
						process.SetAffinity(oldAffinity, logger);
					}
					process.SetPriority(oldProcdessPriority, logger);
				});
		}
		#endregion

		#region IO
		/// <summary>Writes file content without empty line at the end.</summary>
		/// <param name="path">The path.</param>
		/// <param name="lines">The lines to write.</param>
		// THANKSTO: http://stackoverflow.com/a/11689630
		public static void WriteFileContent(string path, string[] lines)
		{
			if (path == null)
				throw new ArgumentNullException(nameof(path));
			if (lines == null)
				throw new ArgumentNullException(nameof(lines));

			using (var writer = File.CreateText(path))
			{
				if (lines.Length > 0)
				{
					for (var i = 0; i < lines.Length - 1; i++)
					{
						writer.WriteLine(lines[i]);
					}
					writer.Write(lines[lines.Length - 1]);
				}
			}
		}

		/// <summary>Tries to obtain text from the given URI.</summary>
		/// <param name="uri">The URI to geth the text from.</param>
		/// <returns>The text reader or <c>null</c> if none.</returns>
		public static TextReader TryGetTextFromUri(string uri) =>
			TryGetTextFromUri(uri, null);

		/// <summary>Tries to obtain text from the given URI.</summary>
		/// <param name="uri">The URI to geth the text from.</param>
		/// <param name="timeOut">The timeout.</param>
		/// <returns>The text reader or <c>null</c> if none.</returns>
		public static TextReader TryGetTextFromUri(string uri, TimeSpan? timeOut)
		{
			if (uri == null)
				throw new ArgumentNullException(nameof(uri));

			var uriInst = new Uri(uri, UriKind.RelativeOrAbsolute);
			if (uriInst.IsAbsoluteUri && !uriInst.IsFile)
			{
				try
				{
					var webRequest = WebRequest.Create(uriInst);
					if (timeOut != null)
					{
						webRequest.Timeout = (int)timeOut.Value.TotalMilliseconds;
					}
					using (var response = webRequest.GetResponse())
					using (var content = response.GetResponseStream())
					{
						if (content == null)
							return null;

						using (var reader = new StreamReader(content))
						{
							return new StringReader(reader.ReadToEnd());
						}
					}
				}
				catch (WebException)
				{
					return null;
				}
			}

			var path = uriInst.IsAbsoluteUri ? uriInst.LocalPath : uri;

			return File.Exists(path) ? File.OpenText(path) : null;
		}
		#endregion

		#region Console
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
				var holder = new PreviousOutputHolder(Console.Out);
				Console.SetOut(output);
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

					CodeJam.Code.BugIf(output != _outputStack.Peek(), "Capture output stack disbalanced.");
					Console.SetOut(_outputStack.Pop().Output);
				}
			}
		}

		/// <summary>Reports that work is completed and asks user to press any key to continue.</summary>
		public static void ConsoleDoneWaitForConfirmation()
		{
			Console.WriteLine();
			Console.Write("Done. Press any key to continue...");

			Console.ReadKey(true);
			Console.WriteLine();
		}
		#endregion

		#region Configs
		/// <summary>
		/// Retuns configuration section from app.config or (if none)
		/// from first of the <paramref name="fallbackAssemblies"/> that have the section in its config.
		/// </summary>
		/// <typeparam name="TSection">Type of the section.</typeparam>
		/// <param name="sectionName">Name of the section.</param>
		/// <param name="fallbackAssemblies">
		/// The assemblies to check for the config section if the app.config does not contain the section.
		/// </param>
		/// <returns>Configuration section with the name specified in <paramref name="sectionName"/>.</returns>
		public static TSection ParseConfigurationSection<TSection>(
			[NotNull] string sectionName,
			params Assembly[] fallbackAssemblies)
			where TSection : ConfigurationSection =>
				ParseConfigurationSection<TSection>(sectionName, fallbackAssemblies.AsEnumerable());

		private static readonly Func<Assembly, string, object> _sectionsCache = Algorithms.Memoize(
			(Assembly a, string sectionName) => ConfigurationManager
				.OpenExeConfiguration(a.GetAssemblyPath())
				.GetSection(sectionName),
			true);

		private static readonly Func<string, object> _configSectionsCache = Algorithms.Memoize(
			(string sectionName) => ConfigurationManager.GetSection(sectionName),
			true);

		/// <summary>
		/// Retuns configuration section from app.config or (if none)
		/// from first of the <paramref name="fallbackAssemblies"/> that have the section in its config.
		/// </summary>
		/// <typeparam name="TSection">Type of the section.</typeparam>
		/// <param name="sectionName">Name of the section.</param>
		/// <param name="fallbackAssemblies">
		/// The assemblies to check for the config section if the app.config does not contain the section.
		/// </param>
		/// <returns>Configuration section with the name specified in <paramref name="sectionName"/>.</returns>
		public static TSection ParseConfigurationSection<TSection>(
			[NotNull] string sectionName,
			IEnumerable<Assembly> fallbackAssemblies)
			where TSection : ConfigurationSection
		{
			if (string.IsNullOrEmpty(sectionName))
				throw new ArgumentNullException(nameof(sectionName));

			// TODO: path to failed in exception
			try
			{
				var result = (TSection)_configSectionsCache(sectionName);
				if (result == null)
				{
					// DONTTOUCH: .Distinct preserves order of fallbackAssemblies.
					foreach (var assembly in fallbackAssemblies.Distinct())
					{
						result = (TSection)_sectionsCache(assembly, sectionName);

						if (result != null)
							break;
					}
				}
				return result;
			}
			catch (ConfigurationErrorsException ex)
			{
				throw new InvalidOperationException("Could not read appconfig file.", ex);
			}
		}
		#endregion

		#region Environment
		/// <summary>
		/// Determines whether any environment variable is set.
		/// </summary>
		/// <param name="variables">The variables to check. Case is ignored.</param>
		/// <returns>
		///   <c>true</c> if any environment variable from <paramref name="variables"/> is set.
		/// </returns>
		public static bool HasAnyEnvironmentVariable(params string[] variables) =>
			HasAnyEnvironmentVariable((IEnumerable<string>)variables);

		/// <summary>
		/// Determines whether any environment variable is set.
		/// </summary>
		/// <param name="variables">The variables to check. Case is ignored.</param>
		/// <returns>
		///   <c>true</c> if any environment variable from <paramref name="variables"/> is set.
		/// </returns>
		public static bool HasAnyEnvironmentVariable(IEnumerable<string> variables)
		{
			if (variables == null)
				return false;

			var envVariables = Environment.GetEnvironmentVariables().Keys.Cast<string>();
			var set = new HashSet<string>(envVariables, StringComparer.OrdinalIgnoreCase);

			return set.Overlaps(variables);
		}
		#endregion

		#region Formatting
		private const double ScaleCoefficient = 1.89; // Empirical value.

		private static int GetAutoscaleDigits(double value)
		{
			// Same logic for positive & negative values.
			value = Math.Abs(value);

			// Corner cases
			if (value <= 0)
				return 0;
			if (value >= 100)
				return 1;
			if (value >= 1)
				return 2;

			// Make value smaller to get additional decimal places for values with normalized mantissa less than 1.9;
			value /= ScaleCoefficient;

			// Get normalization scale. As the value expected to be <<1 result should be negated.
			var normalizationPow = -Math.Log10(value);

			// Add two extra places to normalized values.
			var decimalPlaces = (int)Math.Floor(normalizationPow) + 2;

			// If there's no decimal places - use zero.
			return Math.Max(0, decimalPlaces);
		}

		/// <summary>Gets the autoscaled format for the value.</summary>
		/// <param name="value">The value.</param>
		/// <returns>Autoscaled format for the value</returns>
		public static string GetAutoscaledFormat(double value) =>
			"F" + GetAutoscaleDigits(value);
		#endregion
	}
}