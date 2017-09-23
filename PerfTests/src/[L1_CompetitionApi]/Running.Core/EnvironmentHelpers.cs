using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using CodeJam.Reflection;

using JetBrains.Annotations;

#if TARGETS_NET
using System.Configuration;
#else
using Microsoft.Extensions.Configuration;
using System.Xml;
#endif

namespace CodeJam.PerfTests.Running.Core
{
	/// <summary>
	/// Environment helpers.
	/// </summary>
	[PublicAPI]
	public static class EnvironmentHelpers
	{
		private static readonly Func<Assembly, string, Type, object> _configSectionsCache = Algorithms.Memoize(
			(Assembly a, string sectionName, Type sectionType) => GetConfigSectionToCache(a, sectionName, sectionType),
			true);

		private static object GetConfigSectionToCache(Assembly a, string sectionName, Type sectionType)
		{
#if TARGETS_NET
			if (a == null)
				return ConfigurationManager.GetSection(sectionName);

			return ConfigurationManager
				.OpenExeConfiguration(a.GetAssemblyPath())
				.GetSection(sectionName);
#else
			var configPath = a == null
				? GetDefaultConfigPath()
				: a.GetAssemblyPath() + ".config";
			System.Console.WriteLine(configPath);
			var configurationRoot = new ConfigurationBuilder().AddXmlFile(configPath, true).Build();
			return configurationRoot.GetSection(sectionName).Get(sectionType);
#endif
		}

		private static string GetDefaultConfigPath()
		{
#if TARGETS_NET
			return AppDomain.CurrentDomain.SetupInformation.ConfigurationFile ??
				((Assembly.GetEntryAssembly()?.GetAssemblyPath()??
					Process.GetCurrentProcess().MainModule.FileName) + ".config");
#else
			return (Assembly.GetEntryAssembly()?.GetAssemblyPath() ?? Process.GetCurrentProcess().MainModule.FileName) + ".config";
#endif
		}

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
#if TARGETS_NET
			where TSection : ConfigurationSection
#endif
				=> ParseConfigurationSection<TSection>(sectionName, fallbackAssemblies.AsEnumerable());

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
#if TARGETS_NET
			where TSection : ConfigurationSection
#endif
		{
			if (string.IsNullOrEmpty(sectionName))
				throw new ArgumentNullException(nameof(sectionName));

			var targetFile = GetDefaultConfigPath();
			targetFile = Path.GetFileName(targetFile);
			try
			{
				var result = (TSection)_configSectionsCache(null, sectionName, typeof(TSection));
				if (result == null)
				{
					// DONTTOUCH: .Distinct preserves order of fallbackAssemblies.
					foreach (var assembly in fallbackAssemblies.Distinct())
					{
						targetFile = Path.GetFileName(assembly.GetAssemblyPath() + ".config");

						result = (TSection)_configSectionsCache(assembly, sectionName, typeof(TSection));

						if (result != null)
							break;
					}
				}
				return result;
			}
#if TARGETS_NET
			catch (ConfigurationErrorsException ex)
#else
			catch (XmlException ex)
#endif
			{
				throw new InvalidOperationException(
					$"Could not read config section {sectionName}, file '{targetFile}'.", ex);
			}
		}
	}
}