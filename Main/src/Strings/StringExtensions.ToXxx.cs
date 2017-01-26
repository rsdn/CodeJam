using System;
using System.Globalization;

using JetBrains.Annotations;

namespace CodeJam.Strings
{
	partial class StringExtensions
	{
		/// <summary>
		/// Culture invariant version of <see cref="IFormattable.ToString(string,System.IFormatProvider)"/>
		/// </summary>
		/// <param name="s">Object to convert.</param>
		/// <returns>String representation of <paramref name="s"/> according to rules of invariant culture.</returns>
		[NotNull, Pure]
		public static string ToInvariantString<T>([NotNull] this T s) where T : IFormattable =>
			s.ToString(null, CultureInfo.InvariantCulture);

		/// <summary>
		/// Culture invariant version of <see cref="IFormattable.ToString(string,System.IFormatProvider)"/>
		/// </summary>
		/// <param name="s">Object to convert.</param>
		/// <param name="format">Format string</param>
		/// <returns>String representation of <paramref name="s"/> according to rules of invariant culture.</returns>
		[NotNull, Pure]
		public static string ToInvariantString<T>([NotNull] this T s, string format) where T : IFormattable =>
			s.ToString(format, CultureInfo.InvariantCulture);

		#region DateTime
		/// <summary>
		/// Converts the string representation of a number in a specified style and culture-specific format to its
		/// <see cref="Byte"/> equivalent. A return value indicates whether the conversion succeeded.
		/// </summary>
		/// <param name="str">
		/// A string containing a number to convert. The string is interpreted using the style specified by
		/// <paramref name="dateStyle"/>.
		/// </param>
		/// <param name="dateStyle">
		/// A bitwise combination of enumeration values that indicates the style elements that can be present in
		/// <paramref name="str"/>. Default value is Integer.
		/// </param>
		/// <param name="provider">
		/// An object that supplies culture-specific formatting information about <paramref name="str"/>.
		/// </param>
		/// <returns>
		/// When this method returns, contains the <see cref="Byte"/> value equivalent of the number contained in
		/// <paramref name="str"/>, if the conversion succeeded, or null if the conversion failed. The conversion fails if
		/// the <paramref name="str"/> parameter is null or String.Empty, is not in a format compliant withstyle, or
		/// represents a number less than <see cref="DateTime.MinValue"/> or greater than <see cref="DateTime.MaxValue"/>.
		/// </returns>
		[Pure]
		public static DateTime? ToDateTime(
			[CanBeNull] this string str,
			DateTimeStyles dateStyle = DateTimeStyles.None,
			[CanBeNull] IFormatProvider provider = null)
		{
			DateTime result;
			return DateTime.TryParse(str, provider, dateStyle, out result) ? (DateTime?)result : null;
		}

		/// <summary>
		/// Converts the string representation of a number in a specified style and culture-invariant format to its
		/// <see cref="DateTime"/> equivalent. A return value indicates whether the conversion succeeded.
		/// </summary>
		/// <param name="str">
		/// A string containing a number to convert. The string is interpreted using the style specified by
		/// <paramref name="dateStyle"/>.
		/// </param>
		/// <param name="dateStyle">
		/// A bitwise combination of enumeration values that indicates the style elements that can be present in
		/// <paramref name="str"/>. Default value is Integer.
		/// </param>
		/// <returns>
		/// When this method returns, contains the <see cref="DateTime"/> value equivalent of the number contained in
		/// <paramref name="str"/>, if the conversion succeeded, or null if the conversion failed. The conversion fails if
		/// the <paramref name="str"/> parameter is null or String.Empty, is not in a format compliant withstyle, or
		/// represents a number less than <see cref="DateTime.MinValue"/> or greater than <see cref="DateTime.MaxValue"/>.
		/// </returns>
		[Pure]
		public static DateTime? ToDateTimeInvariant(
			[CanBeNull] this string str,
			DateTimeStyles dateStyle = DateTimeStyles.None)
		{
			DateTime result;
			return DateTime.TryParse(str, CultureInfo.InvariantCulture, dateStyle, out result) ? (DateTime?)result : null;
		}
		#endregion
	}
}
