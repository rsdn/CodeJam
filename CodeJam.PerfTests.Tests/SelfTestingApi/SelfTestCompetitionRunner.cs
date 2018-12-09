using System;
using System.Reflection;

using JetBrains.Annotations;

using NUnit.Framework;

// ReSharper disable once CheckNamespace

namespace CodeJam.PerfTests.Running.Core
{
	[PublicAPI]
	internal class SelfTestCompetitionRunner : ConsoleCompetitionRunner
	{
		#region Override test running behavior
		/// <summary>Returns output directory that should be used for running the test.</summary>
		/// <param name="targetAssembly">The descriptor assembly tests will be run for.</param>
		/// <returns>
		/// Output directory that should be used for running the test or <c>null</c> if the current directory should be used.
		/// </returns>
		protected override string GetOutputDirectory(Assembly targetAssembly)
		{
			if (TestContext.CurrentContext.WorkDirectory != null)
			{
				return TestContext.CurrentContext.TestDirectory;
			}
			return base.GetOutputDirectory(targetAssembly);
		}
		#endregion
	}
}