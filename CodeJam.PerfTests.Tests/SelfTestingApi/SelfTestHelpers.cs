using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

using BenchmarkDotNet.Environments;
using BenchmarkDotNet.Reports;
using BenchmarkDotNet.Validators;

using CodeJam.Collections;
using CodeJam.PerfTests.Configs;
using CodeJam.PerfTests.Running.Core;

using JetBrains.Annotations;

using NUnit.Framework;

namespace CodeJam.PerfTests
{
	[PublicAPI]
	internal static class SelfTestHelpers
	{
		// Jitting = 1, WarmupCount = 2, TargetCount = 2
		public const int ExpectedSelfTestRunCount = 5;

		public static int GetExpectedCount(ICompetitionConfig config, int methodsCount)
		{
			var runMode = config.GetJobs().Single().Run;
			var unrollFactor = runMode.UnrollFactor;
			return GetExpectedCountCore(config, methodsCount, unrollFactor);
		}

		public static int GetExpectedCountIgnoreUnroll(ICompetitionConfig config, int methodsCount)
		{
			return GetExpectedCountCore(config, methodsCount, 1);
		}

		private static int GetExpectedCountCore(ICompetitionConfig config, int methodsCount, int unrollFactor)
		{
			const int JittingCount = 1;
			var runMode = config.GetJobs().Single().Run;

			var singleLaunchCount = JittingCount * unrollFactor +
				(runMode.WarmupCount + runMode.IterationCount) * runMode.InvocationCount;

			return singleLaunchCount * runMode.LaunchCount * methodsCount;
		}

		[MethodImpl(MethodImplOptions.NoInlining)]
		public static void IgnoreIfDebug()
		{
			var caller = Assembly.GetCallingAssembly();
			if ((caller.GetCustomAttribute<DebuggableAttribute>()?.IsJITOptimizerDisabled ?? false) &&
				!HostEnvironmentInfo.GetCurrent().HasAttachedDebugger)
			{
				Assert.Ignore("Please run as a release build");
			}
		}

		private static readonly string[] _mandatoryValidationErrors =
			"which defines benchmarks references non-optimized;which defines benchmarks is non-optimized".Split(';');

		public static ValidationError[] GetNonMandatoryValidationErrors([CanBeNull] this Summary summary)
		{
			return (summary?.ValidationErrors)
				.EmptyIfNull()
				.Where(e => !_mandatoryValidationErrors.Any(text => e.Message.Contains(text)))
				.ToArray();
		}

		public static IMessage[] GetNonMandatoryMessages([NotNull] this CompetitionState competitionState)
		{
			int runMessageNumber = 0;
			int runNumber = 0;
			var result = new List<IMessage>();
			foreach (var message in competitionState.GetMessages())
			{
				if (runNumber != message.RunNumber)
				{
					runNumber = message.RunNumber;
					runMessageNumber = message.RunMessageNumber;
				}

				if (_mandatoryValidationErrors.Any(text => message.MessageText.Contains(text)))
					continue;

				if (runMessageNumber == message.RunMessageNumber)
				{
					result.Add(message);
				}
				else
				{
					result.Add(
						new Message(
							message.RunNumber,
							runMessageNumber,
							message.Elapsed,
							message.MessageSource,
							message.MessageSeverity,
							message.MessageText,
							message.HintText));
				}
				runMessageNumber++;
			}

			return result.ToArray();
		}
	}
}