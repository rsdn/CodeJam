using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

using BenchmarkDotNet.Characteristics;
using BenchmarkDotNet.Environments;
using BenchmarkDotNet.Helpers;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Running;
using BenchmarkDotNet.Toolchains.InProcess;

using JetBrains.Annotations;

// ReSharper disable once CheckNamespace

namespace BenchmarkDotNet.Validators
{
	/// <summary>
	/// Validator to be used together with <see cref="InProcessToolchain"/>
	/// to proof that the config matches the environment.
	/// </summary>
	/// <seealso cref="IValidator"/>
	[PublicAPI]
	[SuppressMessage("ReSharper", "SuggestVarOrType_BuiltInTypes")]
	public class InProcessValidator : IValidator
	{
		#region Validation rules
		// ReSharper disable HeapView.DelegateAllocation
		private static readonly IReadOnlyDictionary<Characteristic, Func<Job, Characteristic, string>> _validationRules =
			new Dictionary<Characteristic, Func<Job, Characteristic, string>>
			{
				// TODO: recheck all characteristics
				{ EnvMode.AffinityCharacteristic, DontValidate },
				{ EnvMode.JitCharacteristic, ValidateEnvironment },
				{ EnvMode.PlatformCharacteristic, ValidatePlatform },
				{ EnvMode.RuntimeCharacteristic, ValidateEnvironment },
				{ GcMode.ServerCharacteristic, ValidateEnvironment },
				{ GcMode.ConcurrentCharacteristic, ValidateEnvironment },
				{ GcMode.CpuGroupsCharacteristic, ValidateEnvironment },
				{ GcMode.ForceCharacteristic, DontValidate },
				{ GcMode.AllowVeryLargeObjectsCharacteristic, ValidateEnvironment },
				{ RunMode.LaunchCountCharacteristic, DontValidate },
				{ RunMode.RunStrategyCharacteristic, DontValidate },
				{ RunMode.WarmupCountCharacteristic, DontValidate },
				{ RunMode.TargetCountCharacteristic, DontValidate },
				{ RunMode.IterationTimeCharacteristic, DontValidate },
				{ RunMode.InvocationCountCharacteristic, DontValidate },
				{ RunMode.UnrollFactorCharacteristic, DontValidate },
				{ AccuracyMode.AnalyzeLaunchVarianceCharacteristic, DontValidate },
				{ AccuracyMode.EvaluateOverheadCharacteristic, DontValidate },
				{ AccuracyMode.MaxStdErrRelativeCharacteristic, DontValidate },
				{ AccuracyMode.MinInvokeCountCharacteristic, DontValidate },
				{ AccuracyMode.MinIterationTimeCharacteristic, DontValidate },
				{ AccuracyMode.RemoveOutliersCharacteristic, DontValidate },
				{ InfrastructureMode.ClockCharacteristic, DontValidate },
				{ InfrastructureMode.EngineFactoryCharacteristic, DontValidate },
				{ InfrastructureMode.ToolchainCharacteristic, ValidateToolchain }
			};

		// ReSharper restore HeapView.DelegateAllocation

		private static string DontValidate(Job job, Characteristic characteristic) => null;

		private static string ValidateEnvironment(Job job, Characteristic characteristic)
		{
			var resolver = EnvResolver.Instance;
			var actual = resolver.Resolve(Job.Default, characteristic);
			var expected = resolver.Resolve(job, characteristic);
			return Equals(actual, expected)
				? null
				: $"value ({expected}) does not match environment ({actual}).";
		}

		private static string ValidatePlatform(Job job, Characteristic characteristic)
		{
			if (job.Env.Platform == Platform.AnyCpu)
				return null;

			return ValidateEnvironment(job, characteristic);
		}

		private static string ValidateToolchain(Job job, Characteristic characteristic) =>
			job.Infrastructure.Toolchain is InProcessToolchain
				? null
				: $"Should be instance of {nameof(InProcessToolchain)}.";
		#endregion

		/// <summary>The instance of validator that does NOT fail on error.</summary>
		public static readonly IValidator DontFailOnError = new InProcessValidator(false);

		/// <summary>The instance of validator that DOES fail on error.</summary>
		public static readonly IValidator FailOnError = new InProcessValidator(true);

		private InProcessValidator(bool failOnErrors)
		{
			TreatsWarningsAsErrors = failOnErrors;
		}

		/// <summary>Gets a value indicating whether warnings are treated as errors.</summary>
		/// <value>
		/// <c>true</c> if treats warnings as errors; otherwise, <c>false</c>.
		/// </value>
		public bool TreatsWarningsAsErrors { get; }

		// TODO: check that all diagnosers can be run in-process
		/// <summary>Proofs that benchmarks' jobs match the environment.</summary>
		/// <param name="benchmarks">The benchmarks to validate.</param>
		/// <returns>Enumerable of validation errors.</returns>
		public IEnumerable<ValidationError> Validate(IList<Benchmark> benchmarks)
		{
			var result = new List<ValidationError>();

			foreach (var job in benchmarks.GetJobs())
			{
				foreach (var characteristic in job.GetCharacteristicsWithValues())
				{
					Func<Job, Characteristic, string> validationRule;
					if (_validationRules.TryGetValue(characteristic, out validationRule))
					{
						var message = validationRule(job, characteristic);
						if (!string.IsNullOrEmpty(message))
						{
							result.Add(
								new ValidationError(
									TreatsWarningsAsErrors,
									$"Job {job}, property {characteristic.FullId}: {message}"));
						}
					}
					else if (characteristic.DeterminesBehavior())
					{
						result.Add(
							new ValidationError(
								false,
								$"Job {job}, property {characteristic.FullId}: no validation rule specified."));
					}
				}
			}

			return result.ToArray();
		}
	}
}