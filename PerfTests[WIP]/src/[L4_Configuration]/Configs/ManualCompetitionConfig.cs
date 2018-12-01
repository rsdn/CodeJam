using System;
using System.Collections.Generic;
using System.Linq;

using BenchmarkDotNet.Analysers;
using BenchmarkDotNet.Characteristics;
using BenchmarkDotNet.Columns;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Exporters;
using BenchmarkDotNet.Filters;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Loggers;
using BenchmarkDotNet.Order;
using BenchmarkDotNet.Reports;
using BenchmarkDotNet.Validators;

using CodeJam.PerfTests.Metrics;

using JetBrains.Annotations;

namespace CodeJam.PerfTests.Configs
{
	/// <summary>Class to ease competition config creation</summary>
	[PublicAPI]
	public sealed class ManualCompetitionConfig : ICompetitionConfig
	{
		#region Fields & .ctor
		private CompetitionOptions _options;

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
		/// <summary>Gets hardware counters.</summary>
		/// <returns>The hardware counters</returns>
		public List<HardwareCounter> HardwareCounters { get; } = new List<HardwareCounter>();
		/// <summary>Gets the filters.</summary>
		/// <returns>Filters</returns>
		public List<IFilter> Filters { get; } = new List<IFilter>();
		/// <summary>Gets the jobs.</summary>
		/// <value>The jobs.</value>
		public List<Job> Jobs { get; } = new List<Job>();
		/// <summary>Gets or sets the order provider.</summary>
		/// <value>The order provider.</value>
		public IOrderProvider OrderProvider { get; set; }
		/// <summary>Gets summary style.</summary>
		/// <returns>The summary style</returns>
		public ISummaryStyle SummaryStyle { get; set; }

		/// <summary>
		/// determines if all auto-generated files should be kept or removed after running benchmarks
		/// </summary>
		public bool KeepBenchmarkFiles { get; set; }

		/// <summary>Competition options.</summary>
		/// <value>Competition options.</value>
		public CompetitionOptions Options
		{
			[NotNull]
			get => _options ?? CompetitionOptions.Default;
			// DONTTOUCH: please DO NOT remove .Freeze() call.
			set => _options = value?.Freeze();
		}

		/// <summary>Gets the jobs.</summary>
		/// <value>The jobs.</value>
		public List<MetricInfo> Metrics { get; } = new List<MetricInfo>();
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
		public void Add(params Job[] newJobs) =>
			// DONTTOUCH: please DO NOT remove .Freeze() call.
			Jobs.AddRange(newJobs.Select(j => j.Freeze()));

		/// <summary>Adds the specified new competition metrics.</summary>
		/// <param name="newMetrics">The new competition metrics.</param>
		public void Add(params MetricInfo[] newMetrics) => Metrics.AddRange(newMetrics);


		/// <summary>Adds the specified new hardware counters.</summary>
		/// <param name="counters">The new hardware counters.</param>
		public void Add(params HardwareCounter[] counters) => HardwareCounters.AddRange(counters);

		/// <summary>Sets the specified provider.</summary>
		/// <param name="provider">The provider.</param>
		public void Set(IOrderProvider provider) => OrderProvider = provider ?? OrderProvider;

		/// <summary>Sets the specified summary style.</summary>
		/// <param name="summaryStyle">The summary style.</param>
		public void Set(ISummaryStyle summaryStyle) => SummaryStyle = summaryStyle ?? SummaryStyle;

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
			Add(config.GetHardwareCounters().ToArray());
			Set(config.GetOrderProvider());
			Set(config.GetSummaryStyle());
			KeepBenchmarkFiles |= config.KeepBenchmarkFiles;

			if (config is ICompetitionConfig competitionConfig)
			{
				Add(competitionConfig.GetMetrics().ToArray());
				Set(competitionConfig.Options);
			}
		}
		#endregion

		/// <summary>Applies modifier to competition options.</summary>
		/// <param name="optionsModifier">Competition options to apply.</param>
		public void ApplyModifier([NotNull] CompetitionOptions optionsModifier)
		{
			Code.NotNull(optionsModifier, nameof(optionsModifier));

			var options = _options;

			string id = null;
			if (options.HasValue(CharacteristicObject.IdCharacteristic))
				id = options.Id;
			if (optionsModifier.HasValue(CharacteristicObject.IdCharacteristic))
				id += optionsModifier.Id;

			// DONTTOUCH: please DO NOT remove .Freeze() call.
			_options = new CompetitionOptions(id, options, optionsModifier).Freeze();
		}

		/// <summary>Applies modifier to jobs.</summary>
		/// <param name="jobModifier">Job modifier to apply.</param>
		public void ApplyModifier([NotNull] Job jobModifier)
		{
			Code.NotNull(jobModifier, nameof(jobModifier));

			var jobs = Jobs;
			for (var i = 0; i < jobs.Count; i++)
			{
				var job = jobs[i];

				string id = null;
				if (job.HasValue(CharacteristicObject.IdCharacteristic))
					id = job.Id;
				if (jobModifier.HasValue(CharacteristicObject.IdCharacteristic))
					id += jobModifier.Id;

				// DONTTOUCH: please DO NOT remove .Freeze() call.
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

		/// <summary>Gets hardware counters.</summary>
		/// <returns>Hardware counters</returns>
		IEnumerable<HardwareCounter> IConfig.GetHardwareCounters() => HardwareCounters;

		/// <summary>Gets the filters.</summary>
		/// <returns>Filters</returns>
		public IEnumerable<IFilter> GetFilters() => Filters;

		/// <summary>Gets the jobs.</summary>
		/// <returns>The jobs.</returns>
		IEnumerable<Job> IConfig.GetJobs() => Jobs;

		/// <summary>Gets the order provider.</summary>
		/// <returns>The order provider.</returns>
		IOrderProvider IConfig.GetOrderProvider() => OrderProvider;

		/// <summary>Gets summary style.</summary>
		/// <returns>The summary style</returns>
		ISummaryStyle IConfig.GetSummaryStyle() => SummaryStyle;

		/// <summary>Gets the union rule.</summary>
		/// <value>The union rule.</value>
		ConfigUnionRule IConfig.UnionRule => ConfigUnionRule.Union;

		/// <summary>Gets competition metrics.</summary>
		/// <returns>The competition metrics.</returns>
		public IEnumerable<MetricInfo> GetMetrics() => Metrics;
		#endregion

		/// <summary>Returns read-only wrapper for the config.</summary>
		/// <returns>The read-only wrapper for the config</returns>
		public ICompetitionConfig AsReadOnly() => new ReadOnlyCompetitionConfig(this);
	}
}