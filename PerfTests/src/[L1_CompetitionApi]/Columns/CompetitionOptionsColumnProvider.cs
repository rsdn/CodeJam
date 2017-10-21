using System;
using System.Collections.Generic;

using BenchmarkDotNet.Columns;
using BenchmarkDotNet.Reports;

namespace CodeJam.PerfTests.Columns
{
	/// <summary>
	/// A column provider for <see cref="CompetitionCharacteristicColumn"/>
	/// </summary>
	/// <seealso cref="BenchmarkDotNet.Columns.IColumnProvider"/>
	/// <seealso cref="CompetitionCharacteristicColumn"/>
	public sealed class CompetitionOptionsColumnProvider : IColumnProvider
	{
		/// <summary>The instance of column provider.</summary>
		public static readonly CompetitionOptionsColumnProvider Instance = new CompetitionOptionsColumnProvider();

		/// <summary>
		/// Prevents a default instance of the <see cref="CompetitionOptionsColumnProvider"/> class from being created.
		/// </summary>
		private CompetitionOptionsColumnProvider() { }

		/// <summary>Gets competition options columns.</summary>
		/// <param name="summary">The summary.</param>
		/// <returns>The competition options columns</returns>
		public IEnumerable<IColumn> GetColumns(Summary summary) => CompetitionCharacteristicColumn.AllColumns;
	}
}