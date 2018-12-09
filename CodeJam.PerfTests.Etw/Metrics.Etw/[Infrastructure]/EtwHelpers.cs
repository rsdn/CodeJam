using System;
using System.IO;

using CodeJam.IO;
using CodeJam.Reflection;

using Microsoft.Diagnostics.Tracing;

namespace CodeJam.PerfTests.Metrics.Etw
{
	/// <summary>
	/// Helper methods for ETW tracing
	/// </summary>
	internal static class EtwHelpers
	{
		/// <summary>Workaround that enables usage of Microsoft.Diagnostics.Tracing.TraceEvent together with shadow copy assemblies.</summary>
		/// <remarks>SEE https://github.com/Microsoft/perfview/issues/292</remarks>
		public static void WorkaroundEnsureNativeDlls()
		{
			var etwAssembly = typeof(ETWTraceEventSource).Assembly;
			var location = Path.GetDirectoryName(etwAssembly.Location);
			var codebase = etwAssembly.GetAssemblyDirectory();

			if (location != null && codebase != null && !location.Equals(codebase, StringComparison.InvariantCultureIgnoreCase))
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

		private static void CopyDirectoryIfExists(string source, string descriptor, bool overwrite = false)
		{
			DebugIoCode.IsWellFormedPath(source, nameof(source));
			DebugIoCode.IsWellFormedPath(descriptor, nameof(descriptor));

			source = PathHelpers.EnsureContainerPath(Path.GetFullPath(source));
			descriptor = PathHelpers.EnsureContainerPath(Path.GetFullPath(descriptor));

			if (!Directory.Exists(source))
				return;

			if (!Directory.Exists(descriptor))
				Directory.CreateDirectory(descriptor);

			foreach (var sourceDirectory in Directory.GetDirectories(source, "*", SearchOption.AllDirectories))
			{
				DebugCode.BugIf(
					!sourceDirectory.Substring(0, source.Length).Equals(source, StringComparison.InvariantCultureIgnoreCase),
					$"GetDirectories() return invalid path. {sourceDirectory} is not a child of {source}");

				var targetDirectory = Path.Combine(descriptor, sourceDirectory.Substring(source.Length));
				Directory.CreateDirectory(targetDirectory);
			}


			foreach (var sourceFile in Directory.GetFiles(source, "*", SearchOption.AllDirectories))
			{
				DebugCode.BugIf(
					!sourceFile.Substring(0, source.Length).Equals(source, StringComparison.InvariantCultureIgnoreCase),
					$"GetFiles() return invalid path. {sourceFile} is not a child of {source}");

				var targetFile = Path.Combine(descriptor, sourceFile.Substring(source.Length));
				if (overwrite || !File.Exists(targetFile))
				{
					File.Copy(sourceFile, targetFile, overwrite);
				}
			}
		}
	}
}