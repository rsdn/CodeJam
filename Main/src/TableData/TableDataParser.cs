using System;
using System.Collections.Generic;
using System.IO;

using JetBrains.Annotations;

namespace CodeJam.TableData
{
	/// <summary>
	/// Contains methods for table data parsing.
	/// </summary>
	[PublicAPI]
	public static class TableDataParser
	{
		/// <summary>
		/// Reads single line from table data and parses it.
		/// </summary>
		/// <param name="reader"><see cref="TextReader"/> to read data from</param>
		/// <param name="lineNum">current number of line</param>
		/// <returns>
		/// Null, if end of file reached, string[0] if line contains no valued, or array of values.
		/// </returns>
		[CanBeNull]
		public delegate string[] Parser([NotNull] TextReader reader, ref int lineNum);

		/// <summary>
		/// Parses table data.
		/// </summary>
		/// <param name="parser">Instance of specific parser.</param>
		/// <param name="text">Text to parse</param>
		/// <returns>Enumeration of <see cref="DataLine"/> contained parsed data.</returns>
		[NotNull]
		public static IEnumerable<DataLine> Parse([NotNull] this Parser parser, [NotNull] string text)
		{
			if (text == null) throw new ArgumentNullException(nameof(text));
			return Parse(parser, new StringReader(text));
		}

		/// <summary>
		/// Parses table data.
		/// </summary>
		/// <param name="reader">Text to parse</param>
		/// <param name="parser">Instance of specific parser.</param>
		/// <returns>Enumeration of <see cref="DataLine"/> contained parsed data.</returns>
		[NotNull]
		public static IEnumerable<DataLine> Parse([NotNull] this Parser parser, [NotNull] TextReader reader)
		{
			if (parser == null) throw new ArgumentNullException(nameof(parser));
			if (reader == null) throw new ArgumentNullException(nameof(reader));

			var lineNum = 1;
			while (true)
			{
				var lastLineNum = lineNum;
				var values = parser(reader, ref lineNum);
				if (values == null)
					yield break;
				if (values.Length > 0) // Skip empty lines
					yield return new DataLine(lastLineNum, values);
			}
		}
	}
}