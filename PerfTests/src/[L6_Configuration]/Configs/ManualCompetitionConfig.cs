using System;
using System.Collections.Generic;
using System.Linq;

using BenchmarkDotNet.Analysers;
using BenchmarkDotNet.Characteristics;
using BenchmarkDotNet.Columns;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Exporters;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Loggers;
using BenchmarkDotNet.Order;
using BenchmarkDotNet.Validators;

using JetBrains.Annotations;

namespace CodeJam.PerfTests.Configs
{
	/// <summary>Class to ease competition config creation</summary>
	[PublicAPI]
	public sealed class ManualCompetitionConfig : ICompetitionConfig
	{
		#region Fields & .ctor
		private CompetitionOptions _competitionOptions;

		/// <summary>Initializes a new instance of the <see cref="ManualCompetitionConfig"/> class.</summary>
		public ManualCompetitionConfig() { }

		/// <summary>Initializes a new instance of the <see cref="ManualCompetitionConfig"/> class.</summary>
		/// <param name="config">The config to init from.</param>
		public ManualCompetitionConfig([CanBeNull] IConfig config)
		{
			Add(config);
		}

		/// <summary>Initializes a new instance of the <see cref="ManualCompetitionConfig"/> class.</summary>
		/// <param name="config">The config to init from.</param>
		public ManualCompetitionConfig([CanBeNull] ICompetitionConfig config)
		{
			Add(config);
		}
		#endregion

		#region Properties
		/// <summary>Gets the column providers.</summary>
		/// <value>The column providers.</value>
		public List<IColumnProvider> ColumnProviders { get; } = new List<IColumnProvider>();
		/// <summary>Gets the exporters.</summary>
		/// <value>The exporters.</value>
		public List<IExporter> Exporters { get; } = new List<IExporter>();
		/// <summary>Gets the loggers.</summary>
		/// <value>The loggers.</value>
		public List<ILogger> Loggers { get; } = new List<ILogger>();
		/// <summary>Gets the diagnosers.</summary>
		/// <value>The diagnosers.</value>
		public List<IDiagnoser> Diagnosers { get; } = new List<IDiagnoser>();
		/// <summary>Gets the analysers.</summary>
		/// <value>The analysers.</value>
		public List<IAnalyser> Analysers { get; } = new List<IAnalyser>();
		/// <summary>Gets the validators.</summary>
		/// <value>The validators.</value>
		public List<IValidator> Validators { get; } = new List<IValidator>();
		/// <summary>Gets the jobs.</summary>
		/// <value>The jobs.</value>
		public List<Job> Jobs { get; } = new List<Job>();
		/// <summary>Gets or sets the order provider.</summary>
		/// <value>The order provider.</value>
		public IOrderProvider OrderProvider { get; set; }
		/// <summary>
		/// determines if all auto-generated files should be kept or removed after running benchmarks
		/// </summary>
		public bool KeepBenchmarkFiles { get; set; }

		/// <summary>Competition options.</summary>
		/// <value>Competition options.</value>
		public CompetitionOptions Options
		{
			get
			{
				return _competitionOptions ?? CompetitionOptions.Default;
			}
			set
			{
				_competitionOptions = value?.Freeze();
			}
		}
		#endregion

		#region Add methods
		/// <summary>Adds the specified new columns.</summary>
		/// <param name="newColumns">The new columns.</param>
		public void Add(params IColumn[] newColumns) => ColumnProviders.AddRange(newColumns.Select(c => c.ToProvider()));
		/// <summary>Adds the specified new column providers.</summary>
		/// <param name="newColumnProviders">The new column providers.</param>
		public void Add(params IColumnProvider[] newColumnProviders) => ColumnProviders.AddRange(newColumnProviders);
		/// <summary>Adds the specified new exporters.</summary>
		/// <param name="newExporters">The new exporters.</param>
		public void Add(params IExporter[] newExporters) => Exporters.AddRange(newExporters);
		/// <summary>Adds the specified new loggers.</summary>
		/// <param name="newLoggers">The new loggers.</param>
		public void Add(params ILogger[] newLoggers) => Loggers.AddRange(newLoggers);
		/// <summary>Adds the specified new diagnosers.</summary>
		/// <param name="newDiagnosers">The new diagnosers.</param>
		public void Add(params IDiagnoser[] newDiagnosers) => Diagnosers.AddRange(newDiagnosers);
		/// <summary>Adds the specified new analysers.</summary>
		/// <param name="newAnalysers">The new analysers.</param>
		public void Add(params IAnalyser[] newAnalysers) => Analysers.AddRange(newAnalysers);
		/// <summary>Adds the specified new validators.</summary>
		/// <param name="newValidators">The new validators.</param>
		public void Add(params IValidator[] newValidators) => Validators.AddRange(newValidators);
		/// <summary>Adds the specified new jobs.</summary>
		/// <param name="newJobs">The new jobs.</param>
		public void Add(params Job[] newJobs) => Jobs.AddRange(newJobs.Select(j => j.Freeze())); // DONTTOUCH: please DO NOT remove .Freeze() call.
																								 /// <summary>Sets the specified provider.</summary>
																								 /// <param name="provider">The provider.</param>
		public void Set(IOrderProvider provider) => OrderProvider = provider ?? OrderProvider;
		/// <summary>Sets the specified competition options.</summary>
		/// <param name="competitionOptions">Competition options.</param>
		public void Set(CompetitionOptions competitionOptions) => Options = competitionOptions ?? Options;

		/// <summary>Fills properties from the specified config.</summary>
		/// <param name="config">The config to init from.</param>
		public void Add(IConfig config)
		{
			if (config == null)
				return;

			Add(config.GetColumnProviders().ToArray());
			Add(config.GetExporters().ToArray());
			Add(config.GetLoggers().ToArray());
			Add(config.GetDiagnosers().ToArray());
			Add(config.GetAnalysers().ToArray());
			Add(config.GetJobs().ToArray());
			Add(config.GetValidators().ToArray());
			Set(config.GetOrderProvider());
			KeepBenchmarkFiles |= config.KeepBenchmarkFiles;

			var competitionConfig = config as ICompetitionConfig;
			if (competitionConfig != null)
			{
				Set(competitionConfig.Options);
			}
		}
		#endregion

		/// <summary>Applies the competition options.</summary>
		/// <param name="competitionOptions">Competition options.</param>
		public void ApplyCompetitionOptions(CompetitionOptions competitionOptions) =>
			Options = competitionOptions == null
				? null
				: new CompetitionOptions(Options, competitionOptions);

		/// <summary>Applies modifier to jobs.</summary>
		/// <param name="jobModifier">Job modifier to apply.</param>
		/// <param name="preserveId">if set to <c>true</c> job ids are preserved.</param>
		public void ApplyToJobs(Job jobModifier, bool preserveId = false)
		{
			var jobs = Jobs;
			for (int i = 0; i < jobs.Count; i++)
			{
				var job = jobs[i];

				string id = null;
				if (preserveId)
				{
					if (job.HasValue(JobMode.IdCharacteristic))
						id = job.Id;
					if (jobModifier.HasValue(JobMode.IdCharacteristic))
						id += jobModifier.Id;
				}

				jobs[i] = new Job(id, job, jobModifier).Freeze();
			}
		}

		#region Explicit ICompetitionConfig
		/// <summary>Gets the column providers.</summary>
		/// <returns>The column providers.</returns>
		IEnumerable<IColumnProvider> IConfig.GetColumnProviders() => ColumnProviders;
		/// <summary>Gets the exporters.</summary>
		/// <returns>The exporters.</returns>
		IEnumerable<IExporter> IConfig.GetExporters() => Exporters;
		/// <summary>Gets the loggers.</summary>
		/// <returns>The loggers.</returns>
		IEnumerable<ILogger> IConfig.GetLoggers() => Loggers;
		/// <summary>Gets the diagnosers.</summary>
		/// <returns>The diagnosers.</returns>
		IEnumerable<IDiagnoser> IConfig.GetDiagnosers() => Diagnosers;
		/// <summary>Gets the analysers.</summary>
		/// <returns>The analysers.</returns>
		IEnumerable<IAnalyser> IConfig.GetAnalysers() => Analysers;
		/// <summary>Gets the validators.</summary>
		/// <returns>The validators.</returns>
		IEnumerable<IValidator> IConfig.GetValidators() => Validators;
		/// <summary>Gets the jobs.</summary>
		/// <returns>The jobs.</returns>
		IEnumerable<Job> IConfig.GetJobs() => Jobs;
		/// <summary>Gets the order provider.</summary>
		/// <returns>The order provider.</returns>
		IOrderProvider IConfig.GetOrderProvider() => OrderProvider;
		/// <summary>Gets the union rule.</summary>
		/// <value>The union rule.</value>
		ConfigUnionRule IConfig.UnionRule => ConfigUnionRule.Union;
		#endregion

		/// <summary>Returns read-only wrapper for the config.</summary>
		/// <returns>Read-only wrapper for the config</returns>
		public ICompetitionConfig AsReadOnly() => new ReadOnlyCompetitionConfig(this);
	}
}