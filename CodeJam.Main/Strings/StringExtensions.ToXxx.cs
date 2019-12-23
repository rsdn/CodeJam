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

#if TARGETS_NET || NETSTANDARD20_OR_GREATER || NETCOREAPP20_OR_GREATER // PUBLIC_API_CHANGES
		/// <summary>
		/// Determines whether the beginning of this string instance matches the specified string when compared using the
		/// invariant culture.
		/// </summary>
		/// <param name="str">String to check.</param>
		/// <param name="value">The string to compare.</param>
		/// <returns><c>true</c> if this instance begins with value; otherwise, <c>false</c>. </returns>
		[Pure]
		public static bool StartsWithInvariant([NotNull] this string str, [NotNull] string value) =>
			str.StartsWith(value, StringComparison.InvariantCulture);

		/// <summary>
		/// Reports the zero-based index of the first occurrence of the specified string in the <paramref name="str"/>
		/// using the invariant culture.
		/// </summary>
		/// <param name="str">The string to check.</param>
		/// <param name="value">The string to seek.</param>
		/// <returns>
		/// The index position of the value parameter if that string is found, or -1 if it is not. If value is <see cref="string.Empty"/>,
		/// the return value is 0
		/// </returns>
		[Pure]
		public static int IndexOfInvariant([NotNull] this string str, [NotNull] string value) =>
			str.IndexOf(value, StringComparison.InvariantCulture);

		/// <summary>
		/// Reports the zero-based index of the first occurrence of the specified string in the <paramref name="str"/>
		/// using the invariant culture. Parameter specify the starting search position in the current string.
		/// </summary>
		/// <param name="str">The string to check.</param>
		/// <param name="value">The string to seek.</param>
		/// <param name="startIndex">The search starting position.</param>
		/// <returns>
		/// The index position of the value parameter if that string is found, or -1 if it is not. If value is <see cref="string.Empty"/>,
		/// the return value is 0
		/// </returns>
		[Pure]
		public static int IndexOfInvariant([NotNull] this string str, [NotNull] string value, int startIndex) =>
			str.IndexOf(value, startIndex, StringComparison.InvariantCulture);

		/// <summary>
		/// Reports the zero-based index of the first occurrence of the specified string in the <paramref name="str"/>
		/// using the invariant culture. Parameters specify the starting search position in the current string and
		/// the number of characters in the current string to search.
		/// </summary>
		/// <param name="str">The string to check.</param>
		/// <param name="value">The string to seek.</param>
		/// <param name="startIndex">The search starting position.</param>
		/// <param name="count">The number of character positions to examine.</param>
		/// <returns>
		/// The index position of the value parameter if that string is found, or -1 if it is not. If value is <see cref="string.Empty"/>,
		/// the return value is 0
		/// </returns>
		[Pure]
		public static int IndexOfInvariant([NotNull] this string str, [NotNull] string value, int startIndex, int count) =>
			str.IndexOf(value, startIndex, count, StringComparison.InvariantCulture);

		/// <summary>
		/// Reports the zero-based index of the last occurrence of the specified string in the <paramref name="str"/>
		/// using the invariant culture.
		/// </summary>
		/// <param name="str">The string to check.</param>
		/// <param name="value">The string to seek.</param>
		/// <returns>
		/// The index position of the value parameter if that string is found, or -1 if it is not. If value is <see cref="string.Empty"/>,
		/// the return value is 0
		/// </returns>
		[Pure]
		public static int LastIndexOfInvariant([NotNull] this string str, [NotNull] string value) =>
			str.LastIndexOf(value, StringComparison.InvariantCulture);

		/// <summary>
		/// Reports the zero-based index of the last occurrence of the specified string in the <paramref name="str"/>
		/// using the invariant culture. Parameter specify the starting search position in the current string.
		/// </summary>
		/// <param name="str">The string to check.</param>
		/// <param name="value">The string to seek.</param>
		/// <param name="startIndex">The search starting position.</param>
		/// <returns>
		/// The index position of the value parameter if that string is found, or -1 if it is not. If value is <see cref="string.Empty"/>,
		/// the return value is 0
		/// </returns>
		[Pure]
		public static int LastIndexOfInvariant([NotNull] this string str, [NotNull] string value, int startIndex) =>
			str.LastIndexOf(value, startIndex, StringComparison.InvariantCulture);

		/// <summary>
		/// Reports the zero-based index of the last occurrence of the specified string in the <paramref name="str"/>
		/// using the invariant culture. Parameters specify the starting search position in the current string and
		/// the number of characters in the current string to search.
		/// </summary>
		/// <param name="str">The string to check.</param>
		/// <param name="value">The string to seek.</param>
		/// <param name="startIndex">The search starting position.</param>
		/// <param name="count">The number of character positions to examine.</param>
		/// <returns>
		/// The index position of the value parameter if that string is found, or -1 if it is not. If value is <see cref="string.Empty"/>,
		/// the return value is 0
		/// </returns>
		[Pure]
		public static int LastIndexOfInvariant(
			[NotNull] this string str,
			[NotNull] string value,
			int startIndex,
			int count) =>
				str.LastIndexOf(value, startIndex, count, StringComparison.InvariantCulture);
#endif

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
		/// the <paramref name="str"/> parameter is null or String.Empty, is not in a format compliant with style, or
		/// represents a number less than <see cref="DateTime.MinValue"/> or greater than <see cref="DateTime.MaxValue"/>.
		/// </returns>
		[Pure]
		public static DateTime? ToDateTime(
			[CanBeNull] this string str,
			DateTimeStyles dateStyle = DateTimeStyles.None,
			[CanBeNull] IFormatProvider provider = null) =>
				DateTime.TryParse(str, provider, dateStyle, out var result) ? (DateTime?)result : null;

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
		/// the <paramref name="str"/> parameter is null or String.Empty, is not in a format compliant with style, or
		/// represents a number less than <see cref="DateTime.MinValue"/> or greater than <see cref="DateTime.MaxValue"/>.
		/// </returns>
		[Pure]
		public static DateTime? ToDateTimeInvariant(
			[CanBeNull] this string str,
			DateTimeStyles dateStyle = DateTimeStyles.None) =>
				DateTime.TryParse(str, CultureInfo.InvariantCulture, dateStyle, out var result) ? (DateTime?)result : null;
		#endregion

		#region DateTimeOffset
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
		/// the <paramref name="str"/> parameter is null or String.Empty, is not in a format compliant with style, or
		/// represents a number less than <see cref="DateTimeOffset.MinValue"/> or greater than <see cref="DateTimeOffset.MaxValue"/>.
		/// </returns>
		[Pure]
		public static DateTimeOffset? ToDateTimeOffset(
			[CanBeNull] this string str,
			DateTimeStyles dateStyle = DateTimeStyles.None,
			[CanBeNull] IFormatProvider provider = null) =>
				DateTimeOffset.TryParse(str, provider, dateStyle, out var result) ? (DateTimeOffset?)result : null;

		/// <summary>
		/// Converts the string representation of a number in a specified style and culture-invariant format to its
		/// <see cref="DateTimeOffset"/> equivalent. A return value indicates whether the conversion succeeded.
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
		/// When this method returns, contains the <see cref="DateTimeOffset"/> value equivalent of the number contained in
		/// <paramref name="str"/>, if the conversion succeeded, or null if the conversion failed. The conversion fails if
		/// the <paramref name="str"/> parameter is null or String.Empty, is not in a format compliant with style, or
		/// represents a number less than <see cref="DateTimeOffset.MinValue"/> or greater than <see cref="DateTimeOffset.MaxValue"/>.
		/// </returns>
		[Pure]
		public static DateTimeOffset? ToDateTimeOffsetInvariant(
			[CanBeNull] this string str,
			DateTimeStyles dateStyle = DateTimeStyles.None) =>
				DateTimeOffset.TryParse(str, CultureInfo.InvariantCulture, dateStyle, out var result) ? (DateTimeOffset?)result : null;
		#endregion
	}
}