using System;

using BenchmarkDotNet.Characteristics;
using BenchmarkDotNet.Jobs;

using JetBrains.Annotations;

namespace CodeJam.PerfTests.Configs
{
	/// <summary>Competition options class.</summary>
	/// <seealso cref="BenchmarkDotNet.Jobs.JobMode{CompetitionOptions}"/>
	[PublicAPI]
	public sealed class CompetitionOptions : JobMode<CompetitionOptions>
	{
		/// <summary>Competition run parameters characteristic.</summary>
		public static readonly Characteristic<CompetitionRunMode> RunOptionsCharacteristic = Characteristic.Create(
			(CompetitionOptions o) => o.RunOptions);

		/// <summary>Competition limit parameters characteristic.</summary>
		public static readonly Characteristic<CompetitionLimitsMode> LimitsCharacteristic = Characteristic.Create(
			(CompetitionOptions o) => o.Limits);

		/// <summary>Source annotation parameters characteristic.</summary>
		public static readonly Characteristic<SourceAnnotationsMode> SourceAnnotationsCharacteristic = Characteristic.Create(
			(CompetitionOptions o) => o.SourceAnnotations);

		/// <summary>Initializes a new instance of the <see cref="CompetitionOptions"/> class.</summary>
		public CompetitionOptions() : this((string)null) { }

		/// <summary>Initializes a new instance of the <see cref="CompetitionOptions"/> class.</summary>
		/// <param name="id">The identifier.</param>
		public CompetitionOptions(string id) : base(id)
		{
			RunOptionsCharacteristic[this] = new CompetitionRunMode();
			LimitsCharacteristic[this] = new CompetitionLimitsMode();
			SourceAnnotationsCharacteristic[this] = new SourceAnnotationsMode();
		}

		/// <summary>Initializes a new instance of the <see cref="CompetitionOptions"/> class.</summary>
		/// <param name="other">Mode to apply.</param>
		public CompetitionOptions(JobMode other) : this((string)null, other) { }

		/// <summary>Initializes a new instance of the <see cref="CompetitionOptions"/> class.</summary>
		/// <param name="others">Modes to apply.</param>
		// ReSharper disable once RedundantCast
		public CompetitionOptions(params JobMode[] others) : this((string)null, others) { }

		/// <summary>Initializes a new instance of the <see cref="CompetitionOptions"/> class.</summary>
		/// <param name="id">The identifier.</param>
		/// <param name="other">Mode to apply.</param>
		public CompetitionOptions(string id, JobMode other) : this(id)
		{
			Apply(other);
		}

		/// <summary>Initializes a new instance of the <see cref="CompetitionOptions"/> class.</summary>
		/// <param name="id">The identifier.</param>
		/// <param name="others">Modes to apply.</param>
		public CompetitionOptions(string id, params JobMode[] others) : this(id)
		{
			Apply(others);
		}

		/// <summary>Competition run parameters.</summary>
		/// <value>Competition run parameters.</value>
		public CompetitionRunMode RunOptions => RunOptionsCharacteristic[this];

		/// <summary>Competition limit parameters.</summary>
		/// <value>Competition limit parameters.</value>
		public CompetitionLimitsMode Limits => LimitsCharacteristic[this];

		/// <summary>Source annotation parameters.</summary>
		/// <value>Source annotation parameters.</value>
		public SourceAnnotationsMode SourceAnnotations => SourceAnnotationsCharacteristic[this];
	}
}