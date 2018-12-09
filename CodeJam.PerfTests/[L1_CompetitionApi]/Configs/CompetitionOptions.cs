using System;

using BenchmarkDotNet.Characteristics;

using JetBrains.Annotations;

namespace CodeJam.PerfTests.Configs
{
	/// <summary>Competition options class.</summary>
	/// <seealso cref="CharacteristicObject"/>
	[PublicAPI]
	public sealed class CompetitionOptions : CharacteristicObject<CompetitionOptions>
	{
		#region Characteristics
		/// <summary>Competition run parameters characteristic.</summary>
		public static readonly Characteristic<CompetitionRunMode> RunOptionsCharacteristic = Characteristic.Create(
			(CompetitionOptions o) => o.RunOptions);

		/// <summary>Competition annotations parameters characteristic.</summary>
		public static readonly Characteristic<CompetitionAnnotationMode> AnnotationsCharacteristic = Characteristic.Create(
			(CompetitionOptions o) => o.Annotations);

		/// <summary>Competition validation parameters characteristic.</summary>
		public static readonly Characteristic<CompetitionCheckMode> ChecksCharacteristic = Characteristic.Create(
			(CompetitionOptions o) => o.Checks);

		/// <summary>Competition adjustment parameters characteristic.</summary>
		public static readonly Characteristic<CompetitionAdjustmentMode> AdjustmentsCharacteristic = Characteristic.Create(
			(CompetitionOptions o) => o.Adjustments);
		#endregion

		/// <summary>Default competition options.</summary>
		public static readonly CompetitionOptions Default = new CompetitionOptions();

		#region .ctors
		/// <summary>Initializes a new instance of the <see cref="CompetitionOptions"/> class.</summary>
		public CompetitionOptions() : this((string)null) { }

		/// <summary>Initializes a new instance of the <see cref="CompetitionOptions"/> class.</summary>
		/// <param name="other">Mode to apply.</param>
		public CompetitionOptions(CharacteristicObject other) : this((string)null, other) { }

		/// <summary>Initializes a new instance of the <see cref="CompetitionOptions"/> class.</summary>
		/// <param name="others">Modes to apply.</param>
		public CompetitionOptions(params CharacteristicObject[] others) : this(null, others) { }

		/// <summary>Initializes a new instance of the <see cref="CompetitionOptions"/> class.</summary>
		/// <param name="id">The identifier.</param>
		public CompetitionOptions(string id) : base(id)
		{
			RunOptionsCharacteristic[this] = new CompetitionRunMode();
			AnnotationsCharacteristic[this] = new CompetitionAnnotationMode();
			ChecksCharacteristic[this] = new CompetitionCheckMode();
			AdjustmentsCharacteristic[this] = new CompetitionAdjustmentMode();
		}

		/// <summary>Initializes a new instance of the <see cref="CompetitionOptions"/> class.</summary>
		/// <param name="id">The identifier.</param>
		/// <param name="other">Mode to apply.</param>
		public CompetitionOptions(string id, CharacteristicObject other) : this(id)
		{
			Apply(other);
		}

		/// <summary>Initializes a new instance of the <see cref="CompetitionOptions"/> class.</summary>
		/// <param name="id">The identifier.</param>
		/// <param name="others">Modes to apply.</param>
		public CompetitionOptions(string id, params CharacteristicObject[] others) : this(id)
		{
			Apply(others);
		}
		#endregion

		/// <summary>Competition run parameters.</summary>
		/// <value>Competition run parameters.</value>
		public CompetitionRunMode RunOptions => RunOptionsCharacteristic[this];

		/// <summary>Competition annotations parameters.</summary>
		/// <value>Competition annotations parameters.</value>
		public CompetitionAnnotationMode Annotations => AnnotationsCharacteristic[this];

		/// <summary>Competition validation parameters.</summary>
		/// <value>Competition validation parameters.</value>
		public CompetitionCheckMode Checks => ChecksCharacteristic[this];

		/// <summary>Competition adjustment parameters.</summary>
		/// <value>Competition adjustment parameters.</value>
		public CompetitionAdjustmentMode Adjustments => AdjustmentsCharacteristic[this];
	}
}