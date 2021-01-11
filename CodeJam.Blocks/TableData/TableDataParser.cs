using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
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
		/// Parses table data.
		/// </summary>
		/// <param name="parser">Instance of specific parser.</param>
		/// <param name="text">Text to parse</param>
		/// <returns>Enumeration of <see cref="DataLine" /> contained parsed data.</returns>
		[JetBrains.Annotations.NotNull]
		[Pure]
		public static IEnumerable<DataLine> Parse([JetBrains.Annotations.NotNull, InstantHandle] this Parser parser, [JetBrains.Annotations.NotNull] string text)
		{
			Code.NotNull(text, nameof(text));

			return Parse(parser, new StringReader(text));
		}

		/// <summary>
		/// Parses table data.
		/// </summary>
		/// <param name="parser">Instance of specific parser.</param>
		/// <param name="reader">Text to parse</param>
		/// <returns>Enumeration of <see cref="DataLine" /> contained parsed data.</returns>
		[JetBrains.Annotations.NotNull]
		[Pure]
		public static IEnumerable<DataLine> Parse([JetBrains.Annotations.NotNull, InstantHandle] this Parser parser, [JetBrains.Annotations.NotNull] TextReader reader)
		{
			Code.NotNull(parser, nameof(parser));
			Code.NotNull(reader, nameof(reader));

			return ParseCore(parser, reader);
		}

		[JetBrains.Annotations.NotNull]
		private static IEnumerable<DataLine> ParseCore([JetBrains.Annotations.NotNull] Parser parser, [JetBrains.Annotations.NotNull] TextReader reader)
		{
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

	/// <summary>
	/// Reads single line from table data and parses it.
	/// </summary>
	/// <param name="reader"><see cref="TextReader"/> to read data from</param>
	/// <param name="lineNum">current number of line</param>
	/// <returns>
	/// Null, if end of file reached, string[0] if line contains no valued, or array of values.
	/// </returns>
	[return: MaybeNull]
	public delegate string[] Parser([JetBrains.Annotations.NotNull] TextReader reader, ref int lineNum);
}