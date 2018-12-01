using System;
using System.Collections.Generic;
using System.Text;

namespace CodeJam.CmdLine
{
	///<summary>
	/// Helper methods for <see cref="ICharInput"/>.
	///</summary>
	internal static partial class CommandLineParser
	{
		/// <summary>
		/// True, if end of file reached.
		/// </summary>
		private static bool IsEof(this ICharInput input) => input.Current == CharInput.Eof;

		/// <summary>
		/// Throw exception if EOF reached.
		/// </summary>
		private static void ThowIfEof(this ICharInput input)
		{
			if (input.IsEof())
				throw new ParsingException("unexpected end of file", input.Position);
		}

		///<summary>
		/// Convert string to char input.
		///</summary>
		private static ICharInput ToCharInput(this string source) => new CharInput(source);

		/// <summary>
		/// Consume single char.
		/// </summary>
		private static ICharInput ConsumeChar(this ICharInput input, char charToConsume)
		{
			if (input.Current != charToConsume)
				throw new ParsingException(
					$"'{charToConsume}' expected, but '{input.Current}' found",
					input.Position);
			return input.GetNext();
		}

		/// <summary>
		/// Consume leading spaces.
		/// </summary>
		private static ICharInput ConsumeSpaces(this ICharInput input)
		{
			while (char.IsWhiteSpace(input.Current))
				input = input.GetNext();
			return input;
		}

		/// <summary>
		/// Consume while space character or end of file reached.
		/// </summary>
		private static ParseResult<string> ConsumeWhileNonSpace(this ICharInput input)
		{
			var sb = new StringBuilder();
			while (!input.IsEof() && !char.IsWhiteSpace(input.Current))
			{
				sb.Append(input.Current);
				input = input.GetNext();
			}
			return new ParseResult<string>(sb.ToString(), input);
		}

		/// <summary>
		/// Consume while stop char occured.
		/// </summary>
		private static ParseResult<string> ConsumeWhile(this ICharInput input, char stopChar)
		{
			var sb = new StringBuilder();
			while (input.Current != stopChar)
			{
				input.ThowIfEof();
				sb.Append(input.Current);
				input = input.GetNext();
			}
			return new ParseResult<string>(sb.ToString(), input);
		}

		/// <summary>
		/// Consume while predicate is true.
		/// </summary>
		private static ParseResult<string> ConsumeWhile(this ICharInput input, Func<char, bool> predicate)
		{
			var sb = new StringBuilder();
			while (predicate(input.Current))
			{
				input.ThowIfEof();
				sb.Append(input.Current);
				input = input.GetNext();
			}
			return new ParseResult<string>(sb.ToString(), input);
		}

		/// <summary>
		/// Consume many elements.
		/// </summary>
		private static ParseResult<T[]> ConsumeTillEof<T>(
			this ICharInput input,
			Func<ICharInput, ParseResult<T>> consumer)
		{
			var list = new List<T>();
			while (true)
			{
				var res = consumer(input);
				if (res == null)
					break;
				list.Add(res.Result);
				input = res.InputRest;
			}
			return new ParseResult<T[]>(list.ToArray(), input);
		}
	}
}