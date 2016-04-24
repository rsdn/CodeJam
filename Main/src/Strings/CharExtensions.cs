using System;
using System.Globalization;

using JetBrains.Annotations;

namespace CodeJam.Strings
{
	/// <summary>
	/// <see cref="char"/> structure extensions.
	/// </summary>
	[PublicAPI]
	public static class CharExtensions
	{
		/// <summary>
		/// Indicates whether a Unicode character is categorized as a Unicode letter.
		/// </summary>
		/// <param name="chr">The Unicode character to evaluate.</param>
		/// <returns><c>true</c> if <paramref name="chr"/> is a letter; otherwise, <c>false</c>.</returns>
		[Pure]
		public static bool IsLetter(this char chr) => char.IsLetter(chr);

		/// <summary>
		/// Indicates whether a Unicode character is categorized as a decimal digit.
		/// </summary>
		/// <param name="chr">The Unicode character to evaluate.</param>
		/// <returns><c>true</c> if <paramref name="chr"/> is a digit; otherwise, <c>false</c>.</returns>
		[Pure]
		public static bool IsDigit(this char chr) => char.IsDigit(chr);

		/// <summary>
		/// Indicates whether the specified Unicode character is categorized as a letter or a decimal digit.
		/// </summary>
		/// <param name="chr">The Unicode character to evaluate.</param>
		/// <returns><c>true</c> if <paramref name="chr"/> is a letter or digit; otherwise, <c>false</c>.</returns>
		[Pure]
		public static bool IsLetterOrDigit(this char chr) => char.IsLetterOrDigit(chr);

		/// <summary>
		/// Indicates whether the specified Unicode character is categorized as white space.
		/// </summary>
		/// <param name="chr">The Unicode character to evaluate.</param>
		/// <returns><c>true</c> if <paramref name="chr"/> is a white space character; otherwise, <c>false</c>.</returns>
		[Pure]
		public static bool IsWhiteSpace(this char chr) => char.IsWhiteSpace(chr);

		/// <summary>
		/// Indicates whether a specified Unicode character is categorized as a control character.
		/// </summary>
		/// <param name="chr">The Unicode character to evaluate.</param>
		/// <returns><c>true</c> if <paramref name="chr"/> is a control character; otherwise, <c>false</c>.</returns>
		[Pure]
		public static bool IsControl(this char chr) => char.IsControl(chr);

		/// <summary>
		/// Indicates whether a character has a surrogate code unit.
		/// </summary>
		/// <param name="chr">The Unicode character to evaluate.</param>
		/// <returns><c>true</c> if <paramref name="chr"/> is a surrogate character; otherwise, <c>false</c>.</returns>
		[Pure]
		public static bool IsSurrogate(this char chr) => char.IsSurrogate(chr);

		/// <summary>
		/// Indicates whether the specified Char object is a high surrogate.
		/// </summary>
		/// <param name="chr">The Unicode character to evaluate.</param>
		/// <returns><c>true</c> if <paramref name="chr"/> is a high surrogate; otherwise, <c>false</c>.</returns>
		[Pure]
		public static bool IsHighSurrogate(this char chr) => char.IsHighSurrogate(chr);

		/// <summary>
		/// Indicates whether the specified Char object is a low surrogate.
		/// </summary>
		/// <param name="chr">The Unicode character to evaluate.</param>
		/// <returns><c>true</c> if <paramref name="chr"/> is a low surrogate; otherwise, <c>false</c>.</returns>
		[Pure]
		public static bool IsLowSurrogate(this char chr) => char.IsLowSurrogate(chr);

		/// <summary>
		/// Indicates whether the specified Unicode character is categorized as a lowercase letter.
		/// </summary>
		/// <param name="chr">The Unicode character to evaluate.</param>
		/// <returns><c>true</c> if <paramref name="chr"/> is a lowercase letter; otherwise, <c>false</c>.</returns>
		[Pure]
		public static bool IsLower(this char chr) => char.IsLower(chr);

		/// <summary>
		/// Indicates whether the specified Unicode character is categorized as a uppercase letter.
		/// </summary>
		/// <param name="chr">The Unicode character to evaluate.</param>
		/// <returns><c>true</c> if <paramref name="chr"/> is a uppercase letter; otherwise, <c>false</c>.</returns>
		[Pure]
		public static bool IsUpper(this char chr) => char.IsUpper(chr);

		/// <summary>
		/// Indicates whether a Unicode character is categorized as a number.
		/// </summary>
		/// <param name="chr">The Unicode character to evaluate.</param>
		/// <returns><c>true</c> if <paramref name="chr"/> is a number; otherwise, <c>false</c>.</returns>
		[Pure]
		public static bool IsNumber(this char chr) => char.IsNumber(chr);

		/// <summary>
		/// Indicates whether a Unicode character is categorized as a punctuation mark.
		/// </summary>
		/// <param name="chr">The Unicode character to evaluate.</param>
		/// <returns><c>true</c> if <paramref name="chr"/> is a punctuation mark; otherwise, <c>false</c>.</returns>
		[Pure]
		public static bool IsPunctuation(this char chr) => char.IsPunctuation(chr);

		/// <summary>
		/// Indicates whether the specified Unicode character is categorized as a separator character.
		/// </summary>
		/// <param name="chr">The Unicode character to evaluate.</param>
		/// <returns><c>true</c> if <paramref name="chr"/> is a separator character; otherwise, <c>false</c>.</returns>
		[Pure]
		public static bool IsSeparator(this char chr) => char.IsSeparator(chr);

		/// <summary>
		/// Indicates whether a Unicode character is categorized as a symbol character.
		/// </summary>
		/// <param name="chr">The Unicode character to evaluate.</param>
		/// <returns><c>true</c> if <paramref name="chr"/> is a symbol character; otherwise, <c>false</c>.</returns>
		[Pure]
		public static bool IsSymbol(this char chr) => char.IsSymbol(chr);

		/// <summary>
		/// Converts the value of a Unicode character to its lowercase equivalent.
		/// </summary>
		/// <param name="chr">The Unicode character to convert.</param>
		/// <returns>
		/// The lowercase equivalent of <paramref name="chr"/>, or the unchanged value of <paramref name="chr"/>,
		/// if <paramref name="chr"/> is already lowercase or not alphabetic.
		/// </returns>
		[Pure]
		public static char ToLower(this char chr) => char.ToLower(chr);

		/// <summary>
		/// Converts the value of a Unicode character to its lowercase equivalent.
		/// </summary>
		/// <param name="chr">The Unicode character to convert.</param>
		/// <param name="culture">An object that supplies culture-specific casing rules.</param>
		/// <returns>
		/// The lowercase equivalent of <paramref name="chr"/>, modified according to <paramref name="culture"/>,
		/// or the unchanged value of <paramref name="chr"/>, if <paramref name="chr"/> is already lowercase or not
		/// alphabetic.
		/// </returns>
		[Pure]
		public static char ToLower(this char chr, CultureInfo culture) => char.ToLower(chr, culture);

		/// <summary>
		/// Converts the value of a Unicode character to its lowercase equivalent using the casing rules of the invariant
		/// culture.
		/// </summary>
		/// <param name="chr">The Unicode character to convert.</param>
		/// <returns>
		/// The lowercase equivalent of <paramref name="chr"/>, or the unchanged value of <paramref name="chr"/>,
		/// if <paramref name="chr"/> is already lowercase or not alphabetic.
		/// </returns>
		[Pure]
		public static char ToLowerInvariant(this char chr) => char.ToLowerInvariant(chr);

		/// <summary>
		/// Converts the value of a Unicode character to its uppercase equivalent.
		/// </summary>
		/// <param name="chr">The Unicode character to convert.</param>
		/// <returns>
		/// The uppercase equivalent of <paramref name="chr"/>, or the unchanged value of <paramref name="chr"/>,
		/// if <paramref name="chr"/> is already uppercase or not alphabetic.
		/// </returns>
		[Pure]
		public static char ToUpper(this char chr) => char.ToUpper(chr);

		/// <summary>
		/// Converts the value of a Unicode character to its uppercase equivalent.
		/// </summary>
		/// <param name="chr">The Unicode character to convert.</param>
		/// <param name="culture">An object that supplies culture-specific casing rules.</param>
		/// <returns>
		/// The uppercase equivalent of <paramref name="chr"/>, modified according to <paramref name="culture"/>,
		/// or the unchanged value of <paramref name="chr"/>,  if <paramref name="chr"/> is already uppercase or not
		/// alphabetic.
		/// </returns>
		[Pure]
		public static char ToUpper(this char chr, CultureInfo culture) => char.ToUpper(chr, culture);

		/// <summary>
		/// Converts the value of a Unicode character to its uppercase equivalent using the casing rules of the invariant
		/// culture.
		/// </summary>
		/// <param name="chr">The Unicode character to convert.</param>
		/// <returns>
		/// The uppercase equivalent of <paramref name="chr"/>, or the unchanged value of <paramref name="chr"/>,
		/// if <paramref name="chr"/> is already uppercase or not alphabetic.
		/// </returns>
		[Pure]
		public static char ToUpperInvariant(this char chr) => char.ToUpperInvariant(chr);
	}
}