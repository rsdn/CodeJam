using System;
using System.Collections.Generic;
using System.Linq;

using BenchmarkDotNet.Columns;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Diagnostics.Windows;
using BenchmarkDotNet.Loggers;
using BenchmarkDotNet.Reports;
using BenchmarkDotNet.Running;
using BenchmarkDotNet.Validators;

using CodeJam.PerfTests.Metrics;
using CodeJam.PerfTests.Running.Core;

using JetBrains.Annotations;

using Microsoft.Diagnostics.Tracing.Parsers;
using Microsoft.Diagnostics.Tracing.Session;
using CodeJam.Collections;
using CodeJam.PerfTests;
using CodeJam.PerfTests.Configs.Factories;
using CodeJam.PerfTests.Configs;

using Microsoft.Diagnostics.Tracing;

namespace CodeJam
{
	public class IoDiagnoser : EtwDiagnoser<IoDiagnoser.Stats>, IDiagnoser
	{
		// TODO: ensure elevated
		// TODO: ensure exception from diagnoser is logged
		#region Static members
		public class Samples : Dictionary<Benchmark, Stats> { }

		public static readonly RunState<Samples> SamplesSlot = new RunState<Samples>();

		public const string DiagnoserId = nameof(IoDiagnoser);
		#endregion

		private IConfig _config;

		#region Overrides no logic
		public IEnumerable<string> Ids => new[] { DiagnoserId };

		protected override ulong EventType => (ulong)KernelTraceEventParser.Keywords.FileIO;
		protected override string SessionNamePrefix => nameof(KernelTraceEventParser.Keywords.FileIO);

		public IColumnProvider GetColumnProvider() => new SimpleColumnProvider( /*_samplesSlot*/); // TODO:pass samples

		public void BeforeAnythingElse(DiagnoserActionParameters _) { }
		public void AfterGlobalSetup(DiagnoserActionParameters _) { }
		#endregion

		public IEnumerable<ValidationError> Validate(ValidationParameters validationParameters)
			=> Enumerable.Empty<ValidationError>();

		public void BeforeMainRun(DiagnoserActionParameters parameters)
		{
			_config = parameters.Config;
			Start(parameters);
		}

		public void BeforeGlobalCleanup() => Stop();

		public void ProcessResults(Benchmark benchmark, BenchmarkReport report)
		{
			var stats = ProcessEtwEvents(benchmark, report.AllMeasurements.Sum(m => m.Operations));
			SamplesSlot[_config].Add(benchmark, stats);
		}

		public void DisplayResults(ILogger logger) { }

		private Stats ProcessEtwEvents(Benchmark benchmark, long totalOperations)
		{
			if (BenchmarkToProcess.Count > 0)
			{
				var processToReport = BenchmarkToProcess[benchmark];
				Stats stats;
				if (StatsPerProcess.TryGetValue(processToReport, out stats))
				{
					stats.TotalOperations = totalOperations;
					return stats;
				}
			}
			return null;
		}

		protected override void EnableProvider()
		{
			Session.EnableKernelProvider(
				KernelTraceEventParser.Keywords.FileIO |
				KernelTraceEventParser.Keywords.DiskFileIO |
				KernelTraceEventParser.Keywords.FileIOInit |
				KernelTraceEventParser.Keywords.DiskIO |
				KernelTraceEventParser.Keywords.SplitIO |
					KernelTraceEventParser.Keywords.NetworkTCPIP);
		}
		protected override void AttachToEvents(TraceEventSession session, Benchmark benchmark)
		{
			session.Source.Kernel.FileIORead += readData =>
			{
				Stats stats;
				if (StatsPerProcess.TryGetValue(readData.ProcessID, out stats))
					stats.ReadBytes += readData.IoSize;
			};

			session.Source.AllEvents += readData =>
			{
				Stats stats;
				if (StatsPerProcess.TryGetValue(readData.ProcessID, out stats))
					stats.ReadBytes += 0;
			};

		}

		public class Stats
		{
			public long TotalOperations { get; set; }

			public long ReadBytes { get; set; }
		}
	}

	public class FileReadMetricValuesProvider : MetricValuesProviderBase
	{
		public FileReadMetricValuesProvider(bool resultIsRelative) : base(SingleValueMetricCalculator.Instance, resultIsRelative) { }

		protected override double[] GetValuesFromReport(BenchmarkReport benchmarkReport, Summary summary)
		{
			var status = IoDiagnoser.SamplesSlot[summary].GetValueOrDefault(benchmarkReport.Benchmark);
			if (status == null) return new double[0];

			return new[] { (double)status.ReadBytes / status.TotalOperations };
		}
		/// <summary>Gets diagnosers the metric values.</summary>
		/// <param name="metric">The metric to get diagnosers for.</param>
		/// <returns>Diagnosers for the metric values</returns>
		protected override IDiagnoser[] GetDiagnosersOverride(MetricInfo metric)
		{
			return new[] { new IoDiagnoser() };
		}
	}


	/// <summary>
	/// IO read bytes metric attribute.
	/// </summary>
	[MetricInfo(GcMetricValuesProvider.Category, MetricSingleValueMode.BothMinAndMax)]
	public class IoReadAttribute : MetricAttributeBase,
		IMetricAttribute<IoReadAttribute.ValuesProvider, BinarySizeUnit>
	{
		/// <summary>
		/// Implementation of <see cref="IMetricValuesProvider"/> for the see <see cref="IoReadAttribute"/>.
		/// that returns allocations per operation.
		/// </summary>
		internal class ValuesProvider : FileReadMetricValuesProvider
		{
			/// <summary>Initializes a new instance of the <see cref="ValuesProvider"/> class.</summary>
			public ValuesProvider() : base(false) { }
		}

		/// <summary>Initializes a new instance of the <see cref="IoReadAttribute"/> class.</summary>
		public IoReadAttribute() { }

		/// <summary>Initializes a new instance of the <see cref="IoReadAttribute"/> class.</summary>
		/// <param name="value">
		/// Exact amount bytes read.
		/// The <see cref="double.NaN"/> marks the value as unset but updateable during the annotation.
		/// Use <seealso cref="double.PositiveInfinity"/> if value is positive infinity (ignored, essentially).
		/// </param>
		/// <param name="binarySize">The binary size unit.</param>
		public IoReadAttribute(double value, BinarySizeUnit binarySize = BinarySizeUnit.Byte) : base(value, binarySize) { }

		/// <summary>Initializes a new instance of the <see cref="IoReadAttribute"/> class.</summary>
		/// <param name="min">
		/// The minimum value.
		/// The <see cref="double.NaN"/> marks the value as unset but updateable during the annotation.
		/// The <seealso cref="double.NegativeInfinity"/> should be used if value is negative infinity (ignored, essentially).
		/// </param>
		/// <param name="max">
		/// The maximum value.
		/// The <see cref="double.NaN"/> marks the value as unset but updateable during the annotation.
		/// Use <seealso cref="double.PositiveInfinity"/> if value is positive infinity (ignored, essentially).
		/// </param>
		/// <param name="binarySize">The binary size unit.</param>
		public IoReadAttribute(double min, double max, BinarySizeUnit binarySize = BinarySizeUnit.Byte)
			: base(min, max, binarySize) { }

		/// <summary>Gets unit of measurement for the metric.</summary>
		/// <value>The unit of measurement for the metric.</value>
		public new BinarySizeUnit UnitOfMeasurement => (BinarySizeUnit?)base.UnitOfMeasurement ?? BinarySizeUnit.Byte;
	}


	public class UseIoMetricModifierAttribute : CompetitionModifierAttribute
	{
		private class ModifierImpl : ICompetitionModifier
		{
			public void Modify(ManualCompetitionConfig competitionConfig)
			{
				competitionConfig.Metrics.Add(
					MetricInfo.FromAttribute<IoReadAttribute>());
			}
		}

		public UseIoMetricModifierAttribute() : base(() => new ModifierImpl()) { }
	}
}