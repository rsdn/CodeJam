using System;
using System.IO;

using JetBrains.Annotations;

namespace CodeJam.CmdLine
{
	/// <summary>
	/// Helper methods for work with command line.
	/// </summary>
	[PublicAPI]
	public static class CommandLineHelper
	{
		/// <summary>
		/// Parse command line.
		/// </summary>
		public static CmdLineNode ParseCommandLine(string source) => CommandLineParser.ParseCommandLine(source);

		/// <summary>
		/// Check command line semantics.
		/// </summary>
		public static void Check(string commandLine, CmdLineRules rules) =>
			Check(CommandLineParser.ParseCommandLine(commandLine), rules);

		/// <summary>
		/// Check command line semantics.
		/// </summary>
		public static void Check(CmdLineNode commandLine, CmdLineRules rules) =>
			CommandLineChecker.Check(commandLine, rules);

		/// <summary>
		/// Print usage.
		/// </summary>
		public static void PrintUsage(
			this CmdLineRules rules,
			TextWriter writer,
			PrintUsageSettings settings)
		{
			Code.NotNull(rules, nameof(rules));
			Code.NotNull(writer, nameof(writer));
			Code.NotNull(settings, nameof(settings));

			UsagePrinter.PrintUsage(rules, writer, settings);
		}

		/// <summary>
		/// Print usage.
		/// </summary>
		public static string PrintUsage(
			this CmdLineRules rules,
			PrintUsageSettings settings)
		{
			var sw = new StringWriter();
			PrintUsage(rules, sw, settings);
			return sw.ToString();
		}

		/// <summary>
		/// Print usage with default settings.
		/// </summary>
		public static void PrintUsage(
			this CmdLineRules rules,
			TextWriter writer) =>
				PrintUsage(rules, writer, new PrintUsageSettings());
	}
}