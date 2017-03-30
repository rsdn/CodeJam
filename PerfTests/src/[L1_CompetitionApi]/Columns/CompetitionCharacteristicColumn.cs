using System;
using System.Linq;

using BenchmarkDotNet.Characteristics;
using BenchmarkDotNet.Columns;
using BenchmarkDotNet.Reports;
using BenchmarkDotNet.Running;

using CodeJam.PerfTests.Configs;
using CodeJam.PerfTests.Running.Core;

using JetBrains.Annotations;

namespace CodeJam.PerfTests.Columns
{
	/// <summary>
	/// Column that displays one of <see cref="CompetitionOptions"/> characteristic.
	/// </summary>
	/// <seealso cref="IColumn"/>
	public class CompetitionCharacteristicColumn : IColumn
	{
		#region Static members
		private const string Competition = nameof(Competition);

		private static readonly CharacteristicPresenter _presenter = CharacteristicPresenter.SummaryPresenter;

		private static readonly Lazy<IColumn[]> _allColumns = new Lazy<IColumn[]>(
			() => CharacteristicHelper
				.GetAllPresentableCharacteristics(typeof(CompetitionOptions), true)
				.Select(c => new CompetitionCharacteristicColumn(c))
				.ToArray<IColumn>());

		/// <summary>Gets columns for all competition options characteristics.</summary>
		/// <value>Columns for all competition options characteristics.</value>
		internal static IColumn[] AllColumns => _allColumns.Value;

		private static CompetitionOptions GetOptions(Summary summary) =>
			CompetitionCore.RunState[summary].Options;
		#endregion

		#region Fields & .ctor
		private readonly Characteristic _characteristic;

		/// <summary>Initializes a new instance of the <see cref="CompetitionCharacteristicColumn"/> class.</summary>
		/// <param name="characteristic">The characteristic.</param>
		public CompetitionCharacteristicColumn([NotNull] Characteristic characteristic)
		{
			Code.NotNull(characteristic, nameof(characteristic));

			_characteristic = characteristic;
			Id = Competition + "." + characteristic.FullId;
			ColumnName = characteristic.Id;

			// The 'Id' characteristic is a special case:
			// here we just print 'Competition'
			if (characteristic.Id == "Id")
				ColumnName = Competition;
		}
		#endregion

		#region Properties
		/// <summary>Display column title in the summary.</summary>
		/// <value>Display column title in the summary</value>
		public string ColumnName { get; }

		/// <summary>
		/// An unique identificator of the column.
		/// <remarks>If there are several columns with the same Id, only one of them will be shown in the summary.</remarks>
		/// </summary>
		public string Id { get; }

		/// <summary>Gets a value indicating whether [always show].</summary>
		/// <value><c>true</c> if [always show]; otherwise, <c>false</c>.</value>
		public bool AlwaysShow => false;

		/// <summary>Gets the category.</summary>
		/// <value>The category.</value>
		public ColumnCategory Category => ColumnCategory.Custom;

		/// <summary>Defines order of column in the same category.</summary>
		public int PriorityInCategory => 0;
		#endregion

		/// <summary>Gets the value.</summary>
		/// <param name="summary">The summary.</param>
		/// <param name="benchmark">The benchmark.</param>
		/// <returns></returns>
		public string GetValue(Summary summary, Benchmark benchmark)
		{
			var options = GetOptions(summary);
			return _presenter.ToPresentation(options, _characteristic);
		}

		/// <summary>Determines whether the specified summary is default.</summary>
		/// <param name="summary">The summary.</param>
		/// <param name="benchmark">The benchmark.</param>
		/// <returns><c>true</c> if the specified summary is default; otherwise, <c>false</c>.</returns>
		public bool IsDefault(Summary summary, Benchmark benchmark) =>
			!GetOptions(summary).HasValue(_characteristic);

		/// <summary>Determines whether the specified summary is available.</summary>
		/// <param name="summary">The summary.</param>
		/// <returns><c>true</c> if the specified summary is available; otherwise, <c>false</c>.</returns>
		public bool IsAvailable(Summary summary) => true;
	}
}