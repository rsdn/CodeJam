using System;
using System.Collections.Generic;
using System.Linq;

using CodeJam.Ranges;

namespace CodeJam.PerfTests.Running.SourceAnnotations
{
	/// <summary>
	/// Information about benchmark descriptor source lines
	/// </summary>
	internal sealed class TargetSourceLines
	{
		private readonly Dictionary<RuntimeTypeHandle, int> _attributeLineNumbers;

		/// <summary>Initializes a new instance of the <see cref="TargetSourceLines"/> class.</summary>
		/// <param name="targetMethodHandle">BenchmarkCase descriptor method handle.</param>
		/// <param name="primaryAttributeLineNumber">Number of source line that contain primary annotation attribute.</param>
		/// <param name="attributeCandidateLineNumbers">Range of source lines that may contain attribute annotations.</param>
		/// <param name="attributeLineNumbers">Source lines that contain attribute annotations.</param>
		public TargetSourceLines(
			RuntimeMethodHandle targetMethodHandle,
			int primaryAttributeLineNumber,
			Range<int> attributeCandidateLineNumbers,
			Dictionary<RuntimeTypeHandle, int> attributeLineNumbers)
		{
			Code.InRange(primaryAttributeLineNumber, nameof(primaryAttributeLineNumber), 1, int.MaxValue);

			Code.AssertArgument(
				Range.Create(1, int.MaxValue).Contains(attributeCandidateLineNumbers),
				nameof(attributeCandidateLineNumbers),
				"Incorrect candidate line numbers range.");

			Code.AssertArgument(
				attributeCandidateLineNumbers.Contains(primaryAttributeLineNumber),
				nameof(primaryAttributeLineNumber),
				"Incorrect primery attribute line number.");

			DebugCode.AssertArgument(
				attributeLineNumbers.Values.All(l => attributeCandidateLineNumbers.Contains(l)),
				nameof(attributeLineNumbers),
				"Incorrect attribute line numbers.");

			TargetMethodHandle = targetMethodHandle;
			PrimaryAttributeLineNumber = primaryAttributeLineNumber;
			AttributeCandidateLineNumbers = attributeCandidateLineNumbers;
			_attributeLineNumbers = attributeLineNumbers;
		}

		/// <summary>Gets benchmark descriptor method handle.</summary>
		/// <value>The benchmark descriptor method handle.</value>
		public RuntimeMethodHandle TargetMethodHandle { get; }

		/// <summary>Gets number of source line that contain primary annotation attribute.</summary>
		/// <value>The number of source line that contain primary annotation attribute.</value>
		public int PrimaryAttributeLineNumber { get; private set; }

		/// <summary>Gets range of source lines that may contain attribute annotations.</summary>
		/// <value>The range of source lines that may contain attribute annotations.</value>
		public Range<int> AttributeCandidateLineNumbers { get; private set; }

		/// <summary>Gets source lines that contain attribute annotations.</summary>
		/// <value>The source lines that contain attribute annotations.</value>
		public IReadOnlyDictionary<RuntimeTypeHandle, int> AttributeLineNumbers => _attributeLineNumbers;

		/// <summary>Fixes stored line numbers after line insert.</summary>
		/// <param name="insertLineNumber">The inserted line number.</param>
		public void FixOnInsert(int insertLineNumber)
		{
			Code.InRange(insertLineNumber, nameof(insertLineNumber), 1, int.MaxValue);

			var range = AttributeCandidateLineNumbers;
			if (range.EndsBefore(insertLineNumber))
				return;

			PrimaryAttributeLineNumber = FixOnInsertTo(PrimaryAttributeLineNumber, insertLineNumber);
			var newFrom = FixOnInsertFrom(range.FromValue, insertLineNumber);
			var newTo = FixOnInsertTo(range.ToValue, insertLineNumber);
			AttributeCandidateLineNumbers = Range.Create(newFrom, newTo);

			foreach (var attributeLine in _attributeLineNumbers.Keys.ToArray())
			{
				var line = _attributeLineNumbers[attributeLine];
				if (insertLineNumber <= line)
				{
					_attributeLineNumbers[attributeLine] = line + 1;
				}
			}
		}

		private static int FixOnInsertFrom(int lineNumber, int insertLineNumber) =>
			insertLineNumber < lineNumber ? lineNumber + 1 : lineNumber;

		private static int FixOnInsertTo(int lineNumber, int insertLineNumber) =>
			insertLineNumber <= lineNumber ? lineNumber + 1 : lineNumber;

		/// <summary>Adds information about source line that contains attribute annotation.</summary>
		/// <param name="attributeType">Type of the attribute.</param>
		/// <param name="attributeLineNumber">The source line number.</param>
		public void AddAttribute(RuntimeTypeHandle attributeType, int attributeLineNumber)
		{
			Code.InRange(
				attributeLineNumber, nameof(attributeLineNumber),
				AttributeCandidateLineNumbers.FromValue, AttributeCandidateLineNumbers.ToValue);

			_attributeLineNumbers.Add(attributeType, attributeLineNumber);
		}
	}
}