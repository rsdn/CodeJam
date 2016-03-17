using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;

using JetBrains.Annotations;

namespace CodeJam.Csv
{
	/// <summary>
	/// RFC4180 compliant CSV parser.
	/// </summary>
	[PublicAPI]
	public static class CsvParser
	{
		/// <summary>
		/// Parses CSV.
		/// </summary>
		[NotNull]
		public static string[][] Parse([NotNull] string text)
		{
			if (text == null) throw new ArgumentNullException(nameof(text));
			return Parse(new StringReader(text));
		}

		/// <summary>
		/// Parses CSV.
		/// </summary>
		[NotNull]
		public static string[][] Parse([NotNull] TextReader reader)
		{
			if (reader == null) throw new ArgumentNullException(nameof(reader));

			var result = new List<string[]>();
			while (true)
			{
				var line = ParseLine(reader);
				if (line == null)
					break;
				if (line.Length > 0) // Skip empty lines
					result.Add(line);
			}
			return result.ToArray();
		}

		[CanBeNull]
		private static string[] ParseLine(TextReader reader)
		{
			var curChar = CharReader.Create(reader);
			if (curChar.IsEof)
				return null; // EOF reached

			var result = new List<string>();
			StringBuilder curField = null;
			var state = ParserState.ExpectField;
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

							if (curChar.IsComma)
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
							if (curChar.IsEof || curChar.IsEol || curChar.IsComma)
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
								throw new FormatException("Unexpected EOF");

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
							if (curChar.IsComma)
							{
								state = ParserState.ExpectField;
								break;
							}
							throw new FormatException($"Unexpected char '{curChar.Char}'");
					}

				curChar = curChar.Next();
			}
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

			public char Char => (char) m_Code;

			public bool IsEof => m_Code == s_Eof;

			public bool IsEol => m_Code == s_Eol;

			public bool IsWhitespace => char.IsWhiteSpace(Char);

			public bool IsComma => m_Code == ',';

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

			public static CharReader Create(TextReader reader)
			{
				return new CharReader(reader, Read(reader));
			}

			public CharReader Next()
			{
				return Create(m_Reader);
			}

			public CharReader Peek()
			{
				return new CharReader(m_Reader, m_Reader.Peek());
			}
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
	}
}