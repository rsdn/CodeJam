using System;

using BenchmarkDotNet.Characteristics;
using BenchmarkDotNet.Jobs;

namespace CodeJam.PerfTests.Configs
{
	/// <summary>Competition parameters class.</summary>
	/// <seealso cref="BenchmarkDotNet.Jobs.JobMode{CompetitionMode}" />
	public sealed class CompetitionMode : JobMode<CompetitionMode>
	{
		/// <summary>Competition run parameters characteristic.</summary>
		public static readonly Characteristic<CompetitionRunMode> RunModeCharacteristic = Characteristic.Create(
			(CompetitionMode m) => m.RunMode);

		/// <summary>Competition limit parameters characteristic.</summary>
		public static readonly Characteristic<CompetitionLimitsMode> LimitsCharacteristic = Characteristic.Create(
			(CompetitionMode m) => m.Limits);

		/// <summary>Source annotation parameters characteristic.</summary>
		public static readonly Characteristic<SourceAnnotationsMode> SourceAnnotationsCharacteristic = Characteristic.Create(
			(CompetitionMode m) => m.SourceAnnotations);

		/// <summary>Initializes a new instance of the <see cref="CompetitionMode"/> class.</summary>
		public CompetitionMode() : this((string)null)
		{ }

		/// <summary>Initializes a new instance of the <see cref="CompetitionMode"/> class.</summary>
		/// <param name="id">The identifier.</param>
		public CompetitionMode(string id) : base(id)
		{
			RunModeCharacteristic[this] = new CompetitionRunMode();
			LimitsCharacteristic[this] = new CompetitionLimitsMode();
			SourceAnnotationsCharacteristic[this] = new SourceAnnotationsMode();
		}

		/// <summary>Initializes a new instance of the <see cref="CompetitionMode"/> class.</summary>
		/// <param name="other">Mode to apply.</param>
		public CompetitionMode(JobMode other) : this((string)null, other)
		{
		}

		/// <summary>Initializes a new instance of the <see cref="CompetitionMode"/> class.</summary>
		/// <param name="others">Modes to apply.</param>
		// ReSharper disable once RedundantCast
		public CompetitionMode(params JobMode[] others) : this((string)null, others)
		{
		}

		/// <summary>Initializes a new instance of the <see cref="CompetitionMode"/> class.</summary>
		/// <param name="id">The identifier.</param>
		/// <param name="other">Mode to apply.</param>
		public CompetitionMode(string id, JobMode other) : this(id)
		{
			Apply(other);
		}

		/// <summary>Initializes a new instance of the <see cref="CompetitionMode"/> class.</summary>
		/// <param name="id">The identifier.</param>
		/// <param name="others">Modes to apply.</param>
		public CompetitionMode(string id, params JobMode[] others) : this(id)
		{
			Apply(others);
		}

		/// <summary>Competition run parameters.</summary>
		/// <value>Competition run parameters.</value>
		public CompetitionRunMode RunMode => RunModeCharacteristic[this];

		/// <summary>Competition limit parameters.</summary>
		/// <value>Competition limit parameters.</value>
		public CompetitionLimitsMode Limits => LimitsCharacteristic[this];

		/// <summary>Source annotation parameters.</summary>
		/// <value>Source annotation parameters.</value>
		public SourceAnnotationsMode SourceAnnotations => SourceAnnotationsCharacteristic[this];
	}
}