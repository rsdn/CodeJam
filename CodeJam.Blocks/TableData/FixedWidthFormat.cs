using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using CodeJam.Collections;
using CodeJam.Strings;

using JetBrains.Annotations;

namespace CodeJam.TableData
{
	/// <summary>
	/// Fixed width format support.
	/// </summary>
	[PublicAPI]
	public static class FixedWidthFormat
	{
		#region Parser
		/// <summary>
		/// Creates fixed width format parser.
		/// </summary>
		/// <param name="widths">Array of column widths</param>
		/// <returns>Parser to use with <see cref="TableDataParser.Parse(Parser,string)"/></returns>
		[Pure]
		[NotNull]
		public static Parser CreateParser([NotNull] int[] widths)
		{
			Code.NotNull(widths, nameof(widths));
			Code.AssertArgument(widths.Length > 0, nameof(widths), "At least one column must be specified");
			Code.AssertArgument(widths.All(w => w > 0), nameof(widths), "Column width must be greater than 0");

			return (TextReader rdr, ref int ln) => Parse(rdr, ref ln, widths);
		}

		/// <summary>
		/// Parses table data.
		/// </summary>
		/// <param name="reader">Text to parse</param>
		/// <param name="widths">Array of column widths</param>
		/// <returns>Enumeration of <see cref="DataLine" /> contained parsed data.</returns>
		[Pure]
		[NotNull]
		public static IEnumerable<DataLine> Parse([NotNull] TextReader reader, [NotNull] int[] widths) =>
			CreateParser(widths).Parse(reader);

		[CanBeNull]
		private static string[]? Parse([NotNull] TextReader reader, ref int lineNum, [NotNull] int[] widths)
		{
			var line = reader.ReadLine();
			if (line == null)
				return null;
			if (line.IsNullOrWhiteSpace())
				return Array<string>.Empty;

			var pos = 0;
			var result = new string[widths.Length];
			for (var i = 0; i < widths.Length; i++)
			{
				var width = widths[i];
				var len = Math.Min(width, line.Length - pos);
				if (len <= 0)
					throw new FormatException($"Line {lineNum} too short");
				result[i] = line.Substring(pos, len).Trim();
				pos += len;
			}
			lineNum++;
			return result;
		}
		#endregion

		#region Printer
		/// <summary>
		/// Prints full data table
		/// </summary>
		/// <param name="writer">Instance of <see cref="TextWriter"/> to write to.</param>
		/// <param name="data">Data to write.</param>
		/// <param name="widths">Array of column widths</param>
		/// <param name="indent">The indent.</param>
		public static void Print(
			[NotNull] TextWriter writer,
			[NotNull, ItemNotNull] IEnumerable<string[]> data,
			[NotNull] int[] widths,
			[CanBeNull] string? indent = null)
		{
			Code.NotNull(writer, nameof (writer));
			Code.NotNull(data, nameof (writer));
			Code.NotNull(widths, nameof(widths));
			Code.AssertArgument(widths.Length > 0, nameof(widths), "At least one column must be specified");
			Code.AssertArgument(widths.All(w => w > 0), nameof(widths), "Column width must be greater than 0");

			var first = true;
			foreach (var line in data)
			{
				Code.AssertState(line.Length <= widths.Length, $"{nameof(widths)} array to short.");
				if (first)
					first = false;
				else
					writer.WriteLine();
				if (indent != null)
					writer.Write(indent);
				for (var i = 0; i < line.Length; i++)
				{
					var val = line[i];
					var width = widths[i];
					writer.Write(
						val.Length > width
							? val.Substring(0, width)
							: val.PadRight(width));
				}
			}
		}
		#endregion
	}
}
