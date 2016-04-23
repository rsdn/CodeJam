using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

using CodeJam.Collections;
using CodeJam.Strings;

using JetBrains.Annotations;

namespace CodeJam.TableData
{
	/// <summary>
	/// CSV format support.
	/// </summary>
	[PublicAPI]
	public static class CsvFormat
	{
		#region Parser
		/// <summary>
		/// Creates RFC4180 compliant CSV parser.
		/// </summary>
		/// <param name="allowEscaping">If true, allows values escaping.</param>
		/// <param name="columnSeparator">Char to use as column separator</param>
		/// <returns>Parser to use with <see cref="TableDataParser.Parse(Parser,string)"/></returns>
		[Pure]
		[NotNull]
		public static Parser CreateParser(bool allowEscaping = true, char columnSeparator = ',') =>
			allowEscaping
				? (Parser)((TextReader rdr, ref int ln) => ParseCsv(rdr, ref ln, columnSeparator))
				: ((TextReader rdr, ref int ln) => ParseCsvNoEscape(rdr, ref ln, columnSeparator));

		[CanBeNull]
		private static string[] ParseCsv(TextReader reader, ref int lineNum, char separator)
		{
			var curChar = CharReader.Create(reader);
			if (curChar.IsEof)
				return null; // EOF reached
			if (curChar.IsEol)
			{
				lineNum++;
#if !FW452
				return Array.Empty<string>();
#else
				return Array<string>.Empty;
#endif
			}

			var result = new List<string>();
			StringBuilder curField = null;
			var state = ParserState.ExpectField;
			var column = 1;
			while (true)
			{
				var skip = false;

				while (!skip)
					switch (state)
					{
						case ParserState.ExpectField:
							if (curChar.IsEof || curChar.IsEol)
							{
								if (result.Count > 0) // Special case - empty string not treated as single empty value
									result.Add("");
								return result.ToArray();
							}

							if (curChar.Char == separator)
							{
								result.Add("");
								state = ParserState.AfterField;
								break;
							}

							skip = true;
							if (curChar.IsWhitespace)
								break;

							curField = new StringBuilder();

							if (curChar.IsDoubleQuota)
							{
								state = ParserState.QuotedField;
								break;
							}

							state = ParserState.Field;
							curField.Append(curChar.Char);
							break;

						case ParserState.Field:
							Debug.Assert(curField != null, "curField != null");
							if (curChar.IsEof || curChar.IsEol || curChar.Char == separator)
							{
								result.Add(curField.ToString().Trim());
								state = ParserState.AfterField;
								break;
							}

							skip = true;
							curField.Append(curChar.Char);
							break;

						case ParserState.QuotedField:
							Debug.Assert(curField != null, "curField != null");
							if (curChar.IsEof)
								throw new FormatException($"Unexpected EOF at line {lineNum} column {column}");

							skip = true;
							if (curChar.IsEol)
							{
								curField.Append("\r\n");
								break;
							}
							if (curChar.IsDoubleQuota)
							{
								var peek = curChar.Peek();
								if (peek.IsDoubleQuota) // Escaped '"'
								{
									curField.Append('"');
									curChar = curChar.Next();
								}
								else
								{
									result.Add(curField.ToString());
									state = ParserState.AfterField;
								}
								break;
							}
							curField.Append(curChar.Char);
							break;

						case ParserState.AfterField:
							if (curChar.IsEof || curChar.IsEol)
								return result.ToArray();
							skip = true;
							if (curChar.IsWhitespace)
								continue;
							if (curChar.Char == separator)
							{
								state = ParserState.ExpectField;
								break;
							}
							throw new FormatException($"Unexpected char '{curChar.Char}' at line {lineNum} column {column}");

						default:
							throw new ArgumentOutOfRangeException();
					}

				curChar = curChar.Next();
				column++;
				if (curChar.IsEol)
				{
					lineNum++;
					column = 1;
				}
			}
		}

		[CanBeNull]
		private static string[] ParseCsvNoEscape(TextReader reader, ref int lineNum, char separator)
		{
			var line = reader.ReadLine();
			if (line == null)
				return null;
			lineNum++;
			var parts = line.Split(separator);
			// Special case - whitespace lines are ignored
			if (parts.Length == 1 && parts[0].IsNullOrWhiteSpace())
#if !FW452
				return Array.Empty<string>();
#else
				return Array<string>.Empty;
#endif
			return parts;
		}

		#region CharReader struct
		private struct CharReader
		{
			private const int s_Eof = -1;
			private const int s_Eol = -2;

			private readonly TextReader m_Reader;
			private readonly int m_Code;

			private CharReader(TextReader reader, int code)
			{
				m_Reader = reader;
				m_Code = code;
			}

			public char Char => (char)m_Code;

			public bool IsEof => m_Code == s_Eof;

			public bool IsEol => m_Code == s_Eol;

			public bool IsWhitespace => char.IsWhiteSpace(Char);

			public bool IsDoubleQuota => m_Code == '"';

			private static int Read(TextReader reader)
			{
				var code = reader.Read();
				if (code == '\r' || code == '\n')
				{
					if (code == '\r' && reader.Peek() == '\n')
						reader.Read();
					return s_Eol;
				}
				return code;
			}

			public static CharReader Create(TextReader reader) => new CharReader(reader, Read(reader));

			public CharReader Next() => Create(m_Reader);

			public CharReader Peek() => new CharReader(m_Reader, m_Reader.Peek());
		}
		#endregion

		#region ParserState enum
		private enum ParserState
		{
			ExpectField,
			Field,
			QuotedField,
			AfterField
		}
		#endregion
		#endregion

		#region Formatter
		/// <summary>
		/// Creates formatter for CSV.
		/// </summary>
		/// <param name="allowEscaping">If true, use escaping.</param>
		/// <returns>Formatter instance</returns>
		[NotNull]
		[Pure]
		public static ITableDataFormatter CreateFormatter(bool allowEscaping = true) =>
			allowEscaping
				? (ITableDataFormatter)new CsvFormatter()
				: new CsvNoEscapeFormatter();

		private class CsvFormatter : ITableDataFormatter
		{
			private static bool IsEscapeRequired(string value)
			{
				if (string.IsNullOrEmpty(value))
					return false;
				return
					char.IsWhiteSpace(value[0])
					|| char.IsWhiteSpace(value[value.Length - 1])
					|| value.IndexOfAny(new[] { '\r', '\n', '"', ',' }) >= 0;
			}

			public int GetValueLength(string value)
			{
				if (!IsEscapeRequired(value))
					return value.Length;
				return value.Length + 2 + value.Count(c => c == '"');
			}

			private static string EscapeValue(string value)
			{
				if (!IsEscapeRequired(value))
					return value;
				return '"' + value.Replace("\"", "\"\"") + '"';
			}

			public string FormatLine(string[] values, int[] columnWidths)
			{
				Code.NotNull(values, nameof(values));
				Code.NotNull(columnWidths, nameof(columnWidths));
				Code.AssertArgument(
					values.Length <= columnWidths.Length,
					nameof(columnWidths),
					"columnWidth array to short");

				return
					values
						.Select(EscapeValue)
						.Zip(columnWidths, (s, w) => s.PadLeft(w))
						.Join(", ");
			}
		}

		private class CsvNoEscapeFormatter : ITableDataFormatter
		{
		#region Implementation of ITableDataFormatter
			/// <summary>
			/// Returns length of formatted value.
			/// </summary>
			/// <param name="value">Value.</param>
			/// <returns>Length of formatted value representation.</returns>
			public int GetValueLength(string value) => value.Length;

			/// <summary>
			/// Prints line of table data.
			/// </summary>
			/// <param name="values">Line values.</param>
			/// <param name="columnWidths">Array of column widths. If null - value is ignored.</param>
			/// <returns>String representation of values</returns>
			public string FormatLine(string[] values, int[] columnWidths) =>
				values
					.Zip(columnWidths, (s, w) => s.PadLeft(w))
					.Join(", ");
#endregion
		}
		#endregion
	}
}