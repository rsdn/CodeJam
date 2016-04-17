using System;
using System.Configuration;
using System.Linq.Expressions;

using BenchmarkDotNet.Configs;
using BenchmarkDotNet.NUnit;

using CodeJam.Reflection;

using JetBrains.Annotations;

namespace CodeJam
{
	/// <summary>
	/// Use this to run fast but inaccurate measures
	/// OPTIONAL: Updates source files with actual min..max ratio for [CompetitionBenchmark]
	/// </summary>
	internal class AssemblyWideConfig : ManualConfig
	{
		/// <summary>
		/// OPTIONAL: Set AssemblyWideConfig.AnnotateOnRun=true in app.config Set this to true
		/// to enable auto-annotation of benchmark methods
		/// </summary>
		[UsedImplicitly]
		public static readonly bool AnnotateOnRun = GetAssemblySwitch(() => AnnotateOnRun);

		private static bool GetAssemblySwitch(Expression<Func<bool>> appSwitchGetter)
		{
			var memberExp = (MemberExpression)appSwitchGetter.Body;
			Code.AssertArgument(
				memberExp.Expression == null,
				nameof(appSwitchGetter),
				"The expression should be simple field (or property) accessor");

			var memberName = memberExp.Member.Name;
			var type = memberExp.Member.DeclaringType;

			// ReSharper disable once PossibleNullReferenceException
			var codeBase = type.Assembly.GetAssemblyPath();
			var config = ConfigurationManager.OpenExeConfiguration(codeBase);

			var path = type.Name + "." + memberName;
			var value = config.AppSettings.Settings[path]?.Value;
			bool result;
			bool.TryParse(value, out result);
			return result;
		}

		/// <summary>
		/// Instance of the config
		/// </summary>
		public static IConfig RunConfig => new AssemblyWideConfig(true);

		/// <summary> 
		/// Constructor
		/// </summary>
		[UsedImplicitly]
		public AssemblyWideConfig() : this(false) { }

		/// <summary> 
		/// Constructor
		/// </summary>
		public AssemblyWideConfig(bool asRunConfig)
		{
			if (asRunConfig)
				Add(DefaultConfig.Instance);

			Add(FastRunConfig.Instance);

			if (AnnotateOnRun)
				Add(
					new AnnotateSourceAnalyser
					{
						RerunIfModified = true
					});
		}
	}
}