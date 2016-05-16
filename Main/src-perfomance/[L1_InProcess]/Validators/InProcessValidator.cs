using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

using BenchmarkDotNet.Helpers;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Running;
using BenchmarkDotNet.Toolchains.InProcess;

namespace BenchmarkDotNet.Validators
{
	[SuppressMessage("ReSharper", "SuggestVarOrType_BuiltInTypes")]
	public class InProcessValidator : IValidator
	{
		// ReSharper disable HeapView.DelegateAllocation
		private static readonly IReadOnlyDictionary<string, Func<IJob, EnvironmentInfo, string>> _validationRules = new Dictionary
			<string, Func<IJob, EnvironmentInfo, string>>
		{
			{ nameof(IJob.Affinity), NoValidation },
			{ nameof(IJob.Framework), ValidateFramework },
			{ nameof(IJob.IterationTime), NoValidation },
			{ nameof(IJob.Jit), ValidateJit },
			{ nameof(IJob.LaunchCount), NoValidation },
			{ nameof(IJob.Mode), NoValidation },
			{ nameof(IJob.Platform), ValidatePlatform },
			{ nameof(IJob.Runtime), ValidateRuntime },
			{ nameof(IJob.TargetCount), NoValidation },
			{ nameof(IJob.Toolchain), ValidateToolchain },
			{ nameof(IJob.WarmupCount), NoValidation },
			// WAITINGFOR: https://github.com/PerfDotNet/BenchmarkDotNet/issues/179
			// TODO: remove as fixed
			{ "Warmup", NoValidation },
			{ "Target", NoValidation },
			{ "Process", NoValidation }
		};

		// ReSharper restore HeapView.DelegateAllocation

		private static string NoValidation(IJob job, EnvironmentInfo env) => null;

		// TODO: Detect framework
		private static string ValidateFramework(IJob job, EnvironmentInfo env)
		{
			switch (job.Framework)
			{
				case Framework.Host:
					return null;
				case Framework.V40:
				case Framework.V45:
				case Framework.V451:
				case Framework.V452:
				case Framework.V46:
				case Framework.V461:
				case Framework.V462:
					return $"Should be set to {nameof(Framework.Host)}.";
				default:
					throw new ArgumentOutOfRangeException(nameof(job.Framework), job.Framework, null);
			}
		}

		private static string ValidateJit(IJob job, EnvironmentInfo env)
		{
			bool isX64 = env.Architecture == "64-bit";
			switch (job.Jit)
			{
				case Jit.Host:
					return null;
				case Jit.LegacyJit:
					return !isX64 || !env.HasRyuJit
						? null
						: "The current setup does not support legacy jit.";
				case Jit.RyuJit:
					return env.HasRyuJit
						? null
						: "The current setup does not support RyuJit.";
				default:
					throw new ArgumentOutOfRangeException(nameof(job.Jit), job.Jit, null);
			}
		}

		private static string ValidatePlatform(IJob job, EnvironmentInfo env)
		{
			bool isX64 = env.Architecture == "64-bit";
			switch (job.Platform)
			{
				case Platform.Host:
				case Platform.AnyCpu:
					return null;
				case Platform.X86:
					return !isX64
						? null
						: "The current process is not run as x86.";
				case Platform.X64:
					return isX64
						? null
						: "The current process is not run as x64.";
				default:
					throw new ArgumentOutOfRangeException(nameof(job.Platform), job.Platform, null);
			}
		}

		// TODO: Detect runtime
		private static string ValidateRuntime(IJob job, EnvironmentInfo env)
		{
			switch (job.Runtime)
			{
				case Runtime.Host:
					return null;
				case Runtime.Clr:
				case Runtime.Mono:
				case Runtime.Dnx:
				case Runtime.Core:
					return $"Should be set to {nameof(Runtime.Host)}.";
				default:
					throw new ArgumentOutOfRangeException(nameof(job.Runtime), job.Runtime, null);
			}
		}

		private static string ValidateToolchain(IJob job, EnvironmentInfo env) =>
			job.Toolchain is InProcessToolchain
				? null
				: "The toolchain should be set to InProcess";

		// TODO: check that analysers can run in-process
		// TODO: check that the target is not static class
		public IEnumerable<IValidationError> Validate(IList<Benchmark> benchmarks)
		{
			var result = new List<IValidationError>();

			var env = EnvironmentInfo.GetCurrent();
			foreach (var job in benchmarks.GetJobs())
			{
				foreach (var jobProperty in job.AllProperties)
				{
					Func<IJob, EnvironmentInfo, string> validationRule;
					if (!_validationRules.TryGetValue(jobProperty.Name, out validationRule))
					{
						var prefix = $"Job {job.GetShortInfo()}, property {jobProperty.Name}: ";
						result.Add(new ValidationError(false, prefix + "no validation rule specified"));
					}
					else
					{
						var message = validationRule(job, env);
						if (!string.IsNullOrEmpty(message))
						{
							var prefix = $"Job {job.GetShortInfo()}, property {jobProperty.Name}: ";
							result.Add(new ValidationError(true, prefix + message));
						}
					}
				}
			}

			return result.ToArray();
		}

		public bool TreatsWarningsAsErrors => true;
	}
}