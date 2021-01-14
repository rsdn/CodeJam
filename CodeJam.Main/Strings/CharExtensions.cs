using System.Globalization;

using JetBrains.Annotations;
#if TARGETS_NET || NETSTANDARD20_OR_GREATER || NETCOREAPP20_OR_GREATER
using CharEx = System.Char;

// ReSharper disable BuiltInTypeReferenceStyleForMemberAccess
#else
using CharEx = System.CharEx;
#endif

namespace CodeJam.Strings
{
	/// <summary>
	/// <see cref="char"/> structure extensions.
	/// </summary>
	[PublicAPI]
	public static partial class CharExtensions
	{
		/// <summary>
		/// Converts the value of a Unicode character to its lowercase equivalent.
		/// </summary>
		/// <param name="chr">The Unicode character to convert.</param>
		/// <returns>
		/// The lowercase equivalent of <paramref name="chr"/>, or the unchanged value of <paramref name="chr"/>,
		/// if <paramref name="chr"/> is already lowercase or not alphabetic.
		/// </returns>
		[Pure, System.Diagnostics.Contracts.Pure]
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
		[Pure, System.Diagnostics.Contracts.Pure]
		public static char ToLower(this char chr, CultureInfo culture) =>
			CharEx.ToLower(chr, culture);

		/// <summary>
		/// Converts the value of a Unicode character to its lowercase equivalent using the casing rules of the invariant
		/// culture.
		/// </summary>
		/// <param name="chr">The Unicode character to convert.</param>
		/// <returns>
		/// The lowercase equivalent of <paramref name="chr"/>, or the unchanged value of <paramref name="chr"/>,
		/// if <paramref name="chr"/> is already lowercase or not alphabetic.
		/// </returns>
		[Pure, System.Diagnostics.Contracts.Pure]
		public static char ToLowerInvariant(this char chr) => char.ToLowerInvariant(chr);

		/// <summary>
		/// Converts the value of a Unicode character to its uppercase equivalent.
		/// </summary>
		/// <param name="chr">The Unicode character to convert.</param>
		/// <returns>
		/// The uppercase equivalent of <paramref name="chr"/>, or the unchanged value of <paramref name="chr"/>,
		/// if <paramref name="chr"/> is already uppercase or not alphabetic.
		/// </returns>
		[Pure, System.Diagnostics.Contracts.Pure]
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
		[Pure, System.Diagnostics.Contracts.Pure]
		public static char ToUpper(this char chr, CultureInfo culture) =>
			CharEx.ToUpper(chr, culture);

		/// <summary>
		/// Converts the value of a Unicode character to its uppercase equivalent using the casing rules of the invariant
		/// culture.
		/// </summary>
		/// <param name="chr">The Unicode character to convert.</param>
		/// <returns>
		/// The uppercase equivalent of <paramref name="chr"/>, or the unchanged value of <paramref name="chr"/>,
		/// if <paramref name="chr"/> is already uppercase or not alphabetic.
		/// </returns>
		[Pure, System.Diagnostics.Contracts.Pure]
		public static char ToUpperInvariant(this char chr) => char.ToUpperInvariant(chr);
	}
}