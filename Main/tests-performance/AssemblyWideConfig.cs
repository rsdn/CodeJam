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
	[PublicAPI]
	public class AssemblyWideConfig : ManualConfig
	{
		/// <summary>
		/// OPTIONAL: Set AssemblyWideConfig.AnnotateOnRun=true in app.config
		/// to enable auto-annotation of benchmark methods
		/// </summary>
		public static readonly bool AnnotateOnRun = GetAssemblySwitch(() => AnnotateOnRun);
		/// <summary>
		/// OPTIONAL: Set AssemblyWideConfig.IgnoreAnnotatedLimits=true in app.config
		/// to enable ignoring existing limits on auto-annotation of benchmark methods
		/// </summary>
		public static readonly bool IgnoreExistingAnnotations = GetAssemblySwitch(() => IgnoreExistingAnnotations);

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
					new CompetitionParametersAnalyser
					{
						AnnotateOnRun = true,
						RerunIfModified = true,
						IgnoreExistingAnnotations = IgnoreExistingAnnotations
					});
		}
	}
}