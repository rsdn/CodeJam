using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;

using CodeJam.Reflection;

using JetBrains.Annotations;

namespace CodeJam.PerfTests.Running.Core
{
	/// <summary>
	/// Environment helpers.
	/// </summary>
	[PublicAPI]
	public static class EnvironmentHelpers
	{
		private static readonly Func<string, object> _configSectionsCache = Algorithms.Memoize(
			(string sectionName) => ConfigurationManager.GetSection(sectionName),
			true);

		private static readonly Func<Assembly, string, object> _sectionsCache = Algorithms.Memoize(
			(Assembly a, string sectionName) => ConfigurationManager
				.OpenExeConfiguration(a.GetAssemblyPath())
				.GetSection(sectionName),
			true);

		/// <summary>Determines whether any environment variable is set.</summary>
		/// <param name="variables">The variables to check. Case is ignored.</param>
		/// <returns>
		/// <c>true</c> if any environment variable from <paramref name="variables"/> is set.
		/// </returns>
		public static bool HasAnyEnvironmentVariable(params string[] variables) =>
			HasAnyEnvironmentVariable((IEnumerable<string>)variables);

		/// <summary>Determines whether any environment variable is set.</summary>
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

			var targetFile =
				AppDomain.CurrentDomain.SetupInformation.ConfigurationFile ??
					(Process.GetCurrentProcess().MainModule.FileName + ".config");
			targetFile = Path.GetFileName(targetFile);
			try
			{
				var result = (TSection)_configSectionsCache(sectionName);
				if (result == null)
				{
					// DONTTOUCH: .Distinct preserves order of fallbackAssemblies.
					foreach (var assembly in fallbackAssemblies.Distinct())
					{
						targetFile = Path.GetFileName(assembly.GetAssemblyPath() + ".config");

						result = (TSection)_sectionsCache(assembly, sectionName);

						if (result != null)
							break;
					}
				}
				return result;
			}
			catch (ConfigurationErrorsException ex)
			{
				throw new InvalidOperationException(
					$"Could not read config section {sectionName}, file '{targetFile}'.", ex);
			}
		}
	}
}