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
		/// <typeparam name="T">Type of object.</typeparam>
		/// <param name="s">Object to convert.</param>
		/// <returns>String representation of <paramref name="s"/> according to rules of invariant culture.</returns>
		[NotNull, Pure]
		public static string ToInvariantString<T>([NotNull] this T s) where T : IFormattable =>
			s.ToString(null, CultureInfo.InvariantCulture);

		/// <summary>
		/// Culture invariant version of <see cref="IFormattable.ToString(string,System.IFormatProvider)"/>
		/// </summary>
		/// <typeparam name="T">Type of object.</typeparam>
		/// <param name="s">Object to convert.</param>
		/// <param name="format">Format string</param>
		/// <returns>String representation of <paramref name="s"/> according to rules of invariant culture.</returns>
		[NotNull, Pure]
		public static string ToInvariantString<T>([NotNull] this T s, string format) where T : IFormattable =>
			s.ToString(format, CultureInfo.InvariantCulture);

		/// <summary>
		///Tries to convert the specified string representation of a logical value to its <see cref="bool"/> equivalent.
		/// </summary>
		/// <param name="str">The string to convert.</param>
		/// <returns>A structure that contains the value that was parsed.</returns>
		[Pure]
		public static bool? ToBoolean([CanBeNull] this string? str) =>
			bool.TryParse(str, out var result) ? (bool?)result : null;

		/// <summary>
		/// Converts the string representation of a date in a specified style and culture-invariant format to its
		/// <see cref="DateTime"/> equivalent. A return value indicates whether the conversion succeeded.
		/// </summary>
		/// <param name="str">
		/// A string containing a date to convert. The string is interpreted using the style specified by
		/// <paramref name="dateStyle"/>.
		/// </param>
		/// <param name="dateStyle">
		/// A bitwise combination of enumeration values that indicates the style elements that can be present in
		/// <paramref name="str"/>. Default value is DateTimeStyles.None.
		/// </param>
		/// <param name="provider">
		/// An object that supplies culture-specific formatting information about <paramref name="str"/>.
		/// </param>
		/// <returns>
		/// When this method returns, contains the <see cref="DateTime"/> value equivalent of the date contained in
		/// <paramref name="str"/>, if the conversion succeeded, or null if the conversion failed. The conversion fails if
		/// the <paramref name="str"/> parameter is null or String.Empty, is not in a format compliant with style, or
		/// represents a date less than <see cref="DateTime.MinValue"/> or greater than <see cref="DateTime.MaxValue"/>.
		/// </returns>
		[Pure]
		public static DateTime? ToDateTime(
			[CanBeNull] this string? str,
			DateTimeStyles dateStyle = DateTimeStyles.None,
			[CanBeNull] IFormatProvider? provider = null) =>
				DateTime.TryParse(str, provider, dateStyle, out var result) ? (DateTime?)result : null;

		/// <summary>
		/// Converts the string representation of a date in a specified style and culture-invariant format to its
		/// <see cref="DateTime"/> equivalent. A return value indicates whether the conversion succeeded.
		/// </summary>
		/// <param name="str">
		/// A string containing a date to convert. The string is interpreted using the style specified by
		/// <paramref name="dateStyle"/>.
		/// </param>
		/// <param name="dateStyle">
		/// A bitwise combination of enumeration values that indicates the style elements that can be present in
		/// <paramref name="str"/>. Default value is DateTimeStyles.None.
		/// </param>
		/// <returns>
		/// When this method returns, contains the <see cref="DateTime"/> value equivalent of the date contained in
		/// <paramref name="str"/>, if the conversion succeeded, or null if the conversion failed. The conversion fails if
		/// the <paramref name="str"/> parameter is null or String.Empty, is not in a format compliant with style, or
		/// represents a date less than <see cref="DateTime.MinValue"/> or greater than <see cref="DateTime.MaxValue"/>.
		/// </returns>
		[Pure]
		public static DateTime? ToDateTimeInvariant(
			[CanBeNull] this string? str,
			DateTimeStyles dateStyle = DateTimeStyles.None) =>
				DateTime.TryParse(str, CultureInfo.InvariantCulture, dateStyle, out var result) ? (DateTime?)result : null;

		/// <summary>
		/// Converts the string representation of a date in a specified style and culture-invariant format to its
		/// <see cref="DateTimeOffset"/> equivalent. A return value indicates whether the conversion succeeded.
		/// </summary>
		/// <param name="str">
		/// A string containing a date to convert. The string is interpreted using the style specified by
		/// <paramref name="dateStyle"/>.
		/// </param>
		/// <param name="dateStyle">
		/// A bitwise combination of enumeration values that indicates the style elements that can be present in
		/// <paramref name="str"/>. Default value is DateTimeStyles.None.
		/// </param>
		/// <param name="provider">
		/// An object that supplies culture-specific formatting information about <paramref name="str"/>.
		/// </param>
		/// <returns>
		/// When this method returns, contains the <see cref="DateTimeOffset"/> value equivalent of the date contained in
		/// <paramref name="str"/>, if the conversion succeeded, or null if the conversion failed. The conversion fails if
		/// the <paramref name="str"/> parameter is null or String.Empty, is not in a format compliant with style, or
		/// represents a date less than <see cref="DateTimeOffset.MinValue"/> or greater than <see cref="DateTimeOffset.MaxValue"/>.
		/// </returns>
		[Pure]
		public static DateTimeOffset? ToDateTimeOffset(
			[CanBeNull] this string? str,
			DateTimeStyles dateStyle = DateTimeStyles.None,
			[CanBeNull] IFormatProvider? provider = null) =>
				DateTimeOffset.TryParse(str, provider, dateStyle, out var result) ? (DateTimeOffset?)result : null;

		/// <summary>
		/// Converts the string representation of a date in a specified style and culture-invariant format to its
		/// <see cref="DateTimeOffset"/> equivalent. A return value indicates whether the conversion succeeded.
		/// </summary>
		/// <param name="str">
		/// A string containing a date to convert. The string is interpreted using the style specified by
		/// <paramref name="dateStyle"/>.
		/// </param>
		/// <param name="dateStyle">
		/// A bitwise combination of enumeration values that indicates the style elements that can be present in
		/// <paramref name="str"/>. Default value is DateTimeStyles.None.
		/// </param>
		/// <returns>
		/// When this method returns, contains the <see cref="DateTimeOffset"/> value equivalent of the date contained in
		/// <paramref name="str"/>, if the conversion succeeded, or null if the conversion failed. The conversion fails if
		/// the <paramref name="str"/> parameter is null or String.Empty, is not in a format compliant with style, or
		/// represents a date less than <see cref="DateTimeOffset.MinValue"/> or greater than <see cref="DateTimeOffset.MaxValue"/>.
		/// </returns>
		[Pure]
		public static DateTimeOffset? ToDateTimeOffsetInvariant(
			[CanBeNull] this string? str,
			DateTimeStyles dateStyle = DateTimeStyles.None) =>
				DateTimeOffset.TryParse(str, CultureInfo.InvariantCulture, dateStyle, out var result)
					? (DateTimeOffset?)result
					: null;

		/// <summary>
		/// Creates a new <see cref="T:System.Uri" /> using the specified <see cref="T:System.String" /> instance
		/// and a <see cref="T:System.UriKind" />.
		/// </summary>
		/// <param name="str">The <see cref="T:System.String" /> representing the <see cref="T:System.Uri" />.</param>
		/// <param name="uriKind">The type of the Uri. DefaultValue is <see cref="UriKind.RelativeOrAbsolute"/>.</param>
		/// <returns>Constructed <see cref="T:System.Uri" />.</returns>
		[Pure, CanBeNull]
		public static Uri ToUri([CanBeNull] this string? str, UriKind uriKind = UriKind.RelativeOrAbsolute) =>
			Uri.TryCreate(str, uriKind, out var result) ? result : null;

#if NET40_OR_GREATER || TARGETS_NETSTANDARD || TARGETS_NETCOREAPP // PUBLIC_API_CHANGES
		/// <summary>
		/// Converts the string representation of a GUID to the equivalent <see cref="Guid"/> structure.
		/// </summary>
		/// <param name="str">The string to convert.</param>
		/// <returns>A structure that contains the value that was parsed.</returns>
		[Pure]
		public static Guid? ToGuid([CanBeNull] this string? str) =>
			Guid.TryParse(str, out var result) ? (Guid?)result : null;

		/// <summary>
		/// Converts the string representation of a time interval to its <see cref="T:System.TimeSpan" /> equivalent
		/// by using the specified culture-specific format information.
		/// </summary><param name="str">A string that specifies the time interval to convert.</param>
		/// <param name="formatProvider">An object that supplies culture-specific formatting information.</param>
		/// <returns>A time interval that corresponds to <paramref name="str" />, as specified by <paramref name="formatProvider" />.</returns>
		[Pure]
		public static TimeSpan? ToTimeSpan([CanBeNull] this string? str, IFormatProvider formatProvider) =>
			TimeSpan.TryParse(str, formatProvider, out var result) ? (TimeSpan?)result : null;

		/// <summary>
		/// Converts the string representation of a time interval to its <see cref="T:System.TimeSpan" /> equivalent
		/// by using invariant culture format information.
		/// </summary><param name="str">A string that specifies the time interval to convert.</param>
		/// <returns>A time interval that corresponds to <paramref name="str" />.</returns>
		[Pure]
		public static TimeSpan? ToTimeSpanInvariant([CanBeNull] this string? str) =>
			TimeSpan.TryParse(str, CultureInfo.InvariantCulture, out var result) ? (TimeSpan?)result : null;
#endif

	}
}