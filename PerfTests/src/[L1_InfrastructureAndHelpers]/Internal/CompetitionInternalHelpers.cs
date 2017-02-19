using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;

using BenchmarkDotNet.Loggers;

using CodeJam.Reflection;

using JetBrains.Annotations;

namespace CodeJam.PerfTests.Internal
{
	// TODO: move to different classes
	/// <summary>
	/// Helper methods for benchmark infrastructure.
	/// </summary>
	[PublicAPI]
	[SuppressMessage("ReSharper", "ArrangeBraces_using")]
	public static class CompetitionInternalHelpers
	{
		#region ILogger
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

		#region System.Reflection
		/// <summary>Gets the name of the attribute without 'Attribute' suffix.</summary>
		/// <param name="attributeType">Type of the attribute.</param>
		/// <returns>Name of the attribute without 'Attribute' suffix.</returns>
		public static string GetAttributeName([NotNull] this Type attributeType)
		{
			if (!typeof(Attribute).IsAssignableFrom(attributeType))
				throw CodeExceptions.Argument(
					nameof(attributeType), $"The {nameof(attributeType)} should be derived from {typeof(Attribute)}");

			var attributeName = attributeType.Name;
			if (attributeName.EndsWith(nameof(Attribute)))
				attributeName = attributeName.Substring(0, attributeName.Length - nameof(Attribute).Length);

			return attributeName;
		}
		#endregion

		#region System.IO
		/// <summary>Reads file content and fails if not able to detect encoding.</summary>
		/// <param name="path">The path.</param>
		/// <returns>File lines.</returns>
		public static string[] ReadFileContent(string path)
		{
			var lines = new List<string>();
			using (var streamReader = new StreamReader(path))
			{
				string line;
				while ((line = streamReader.ReadLine()) != null)
				{
					var fallback = streamReader.CurrentEncoding.DecoderFallback as DecoderReplacementFallback;
					if (fallback != null)
					{
						var idx = line.IndexOf(fallback.DefaultString, StringComparison.Ordinal);
						if (idx >= 0)
							throw new DecoderFallbackException(
								$"Invalid character at line {lines.Count + 1}, position {idx + 1}.");
					}
					lines.Add(line);
				}
			}
			return lines.ToArray();
		}

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

		#region Formatting
		private const double ScaleCoefficient = 1.89; // Empirical value.

		/// <summary>Returns number of fractional digits for rounding same scale values.</summary>
		/// <param name="value">The value.</param>
		/// <returns>Number of fractional digits for rounding same scale values.</returns>
		public static int GetRoundingDigits(double value)
		{
			if (double.IsNaN(value) || double.IsInfinity(value))
				return 0;

			// Same logic for positive & negative values.
			value = Math.Abs(value);

			// Corner cases
			if (value >= 100)
				return 1;
			if (value <= 0 || value >= 1)
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
			"F" + GetRoundingDigits(value);
		#endregion

		#region Environment
		/// <summary>
		/// Determines whether any environment variable is set.
		/// </summary>
		/// <param name="variables">The variables to check. Case is ignored.</param>
		/// <returns>
		/// <c>true</c> if any environment variable from <paramref name="variables"/> is set.
		/// </returns>
		public static bool HasAnyEnvironmentVariable(params string[] variables) =>
			HasAnyEnvironmentVariable((IEnumerable<string>)variables);

		/// <summary>
		/// Determines whether any environment variable is set.
		/// </summary>
		/// <param name="variables">The variables to check. Case is ignored.</param>
		/// <returns>
		/// <c>true</c> if any environment variable from <paramref name="variables"/> is set.
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

		#region System.Configuration
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

		#region System.Console
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
	}
}