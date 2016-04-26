using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

using JetBrains.Annotations;

namespace CodeJam.Strings
{
	static partial class StringExtensions
	{
		/// <summary>
		/// Infix form of <see cref="string.IsNullOrEmpty"/>.
		/// </summary>
		/// <param name="str">The string.</param>
		/// <returns><c>true</c> if <paramref name="str"/> is null or empty; otherwise, <c>false</c>.</returns>
		[Pure]
		[ContractAnnotation("str:null => true")]
		public static bool IsNullOrEmpty([CanBeNull] this string str) => string.IsNullOrEmpty(str);

		/// <summary>
		/// Returns true if argument is not null nor empty.
		/// </summary>
		/// <param name="str">The string.</param>
		/// <returns><c>true</c> if <paramref name="str"/> is not null nor empty; otherwise, <c>false</c>.</returns>
		[Pure]
		[ContractAnnotation("str:null => false")]
		public static bool NotNullNorEmpty([CanBeNull] this string str) => !string.IsNullOrEmpty(str);

		/// <summary>
		/// Infix form of <see cref="string.IsNullOrWhiteSpace"/>.
		/// </summary>
		/// <param name="str">The string.</param>
		/// <returns>
		/// <c>true</c> if <paramref name="str"/> is null, empty or contains only whitespaces; otherwise <c>false</c>.
		/// </returns>
		[Pure]
		[ContractAnnotation("str:null => true")]
		public static bool IsNullOrWhiteSpace([CanBeNull] this string str) => string.IsNullOrWhiteSpace(str);

		/// <summary>
		/// Returns true if argument is not null nor whitespace.
		/// </summary>
		/// <returns>
		/// <param name="str">The string.</param>
		/// <c>true</c> if <paramref name="str"/> is not null, nor empty or contains not only whitespaces;
		/// otherwise <c>false</c>.
		/// </returns>
		[Pure]
		[ContractAnnotation("str:null => false")]
		public static bool NotNullNorWhiteSpace([CanBeNull] this string str) => !string.IsNullOrWhiteSpace(str);

		/// <summary>
		/// Replaces one or more format items in a specified string with the string representation of a specified object.
		/// </summary>
		/// <param name="format">A composite format string.</param>
		/// <param name="arg">The object to format.</param>
		/// <returns>
		/// A copy of <paramref name="format"/> in which any format items are replaced by the string representation of
		/// <paramref name="arg"/>.
		/// </returns>
		[NotNull]
		[Pure]
		[StringFormatMethod("format")]
		public static string FormatWith([NotNull] this string format, object arg) => string.Format(format, arg);

		/// <summary>
		/// Replaces the format items in a specified string with the string representation of two specified objects.
		/// </summary>
		/// <param name="format">A composite format string.</param>
		/// <param name="arg0">The first object to format.</param>
		/// <param name="arg1">The second object to format.</param>
		/// <returns>
		/// A copy of <paramref name="format"/> in which format items are replaced by the string representations
		/// of <paramref name="arg0"/> and <paramref name="arg1"/>.
		/// </returns>
		[NotNull, Pure]
		[StringFormatMethod("format")]
		public static string FormatWith([NotNull] this string format, object arg0, object arg1) =>
			string.Format(format, arg0, arg1);

		/// <summary>
		/// Replaces the format items in a specified string with the string representation of three specified objects.
		/// </summary>
		/// <param name="format">A composite format string.</param>
		/// <param name="arg0">The first object to format.</param>
		/// <param name="arg1">The second object to format.</param>
		/// <param name="arg2">The third object to format.</param>
		/// <returns>
		/// A copy of <paramref name="format"/> in which the format items have been replaced by the string representations
		/// of <paramref name="arg0"/>, <paramref name="arg1"/>, and <paramref name="arg2"/>.
		/// </returns>
		[NotNull, Pure]
		[StringFormatMethod("format")]
		public static string FormatWith([NotNull] this string format, object arg0, object arg1, object arg2) =>
			string.Format(format, arg0, arg1, arg2);

		/// <summary>
		/// Replaces the format items in a specified string with the string representations
		/// of corresponding objects in a specified array.
		/// </summary>
		/// <param name="format">A composite format string.</param>
		/// <param name="args">An object array that contains zero or more objects to format.</param>
		/// <returns>
		/// A copy of format in which the format items have been replaced by the string representation of the corresponding
		/// objects in args
		/// </returns>
		[NotNull, Pure]
		[StringFormatMethod("format")]
		public static string FormatWith([NotNull] this string format, params object[] args) => string.Format(format, args);

		/// <summary>
		/// Concatenates all the elements of a string array, using the specified separator between each element.
		/// </summary>
		/// <remarks>
		/// Infix form of <see cref="string.Join(string,string[])"/>.
		/// </remarks>
		/// <param name="values">An array that contains the elements to concatenate.</param>
		/// <param name="separator">
		/// The string to use as a separator. <paramref name="separator"/> is included in the returned string only
		/// if <paramref name="values"/> has more than one element.
		/// </param>
		/// <returns>
		/// A string that consists of the members of <paramref name="values"/> delimited by the <paramref name="separator"/>
		/// string.
		/// If <paramref name="values"/> has no members, the method returns <see cref="string.Empty"/>.
		/// </returns>
		[NotNull, Pure]
		public static string Join([NotNull] this string[] values, [CanBeNull] string separator) =>
			string.Join(separator, values);

		/// <summary>
		/// Concatenates the members of a constructed <see cref="IEnumerable{T}"/> collection of type <see cref="string"/>,
		/// using the specified separator between each member.
		/// </summary>
		/// <remarks>
		/// Infix form of <see cref="string.Join(string,IEnumerable{string})"/>.
		/// </remarks>
		/// <param name="values">A collection that contains the strings to concatenate.</param>
		/// <param name="separator">
		/// The string to use as a separator. <paramref name="separator"/> is included in the returned string only
		/// if <paramref name="values"/> has more than one element.
		/// </param>
		/// <returns>
		/// A string that consists of the members of <paramref name="values"/> delimited by the <paramref name="separator"/>
		/// string.
		/// If <paramref name="values"/> has no members, the method returns <see cref="string.Empty"/>.
		/// </returns>
		[NotNull, Pure]
		public static string Join([NotNull, InstantHandle] this IEnumerable<string> values, [CanBeNull] string separator) =>
			string.Join(separator, values);

		/// <summary>
		/// Concatenates the members of a collection, using the specified separator between each member.
		/// </summary>
		/// <remarks>
		/// Infix form of <see cref="string.Join{T}(string, IEnumerable{T})"/>.
		/// </remarks>
		/// <param name="values">A collection that contains the strings to concatenate.</param>
		/// <param name="separator">
		/// The string to use as a separator. <paramref name="separator"/> is included in the returned string only
		/// if <paramref name="values"/> has more than one element.
		/// </param>
		/// <returns>
		/// A string that consists of the members of <paramref name="values"/> delimited by the <paramref name="separator"/>
		/// string.
		/// If <paramref name="values"/> has no members, the method returns <see cref="string.Empty"/>.
		/// </returns>
		[NotNull, Pure]
		public static string Join<T>([NotNull, InstantHandle] this IEnumerable<T> values, [CanBeNull] string separator) =>
			string.Join(separator, values);

		/// <summary>
		/// Concatenates the members of a collection.
		/// </summary>
		/// <param name="values">A collection that contains the strings to concatenate.</param>
		/// <returns>
		/// A string that consists of the members of <paramref name="values"/>.
		/// If <paramref name="values"/> has no members, the method returns <see cref="string.Empty"/>.
		/// </returns>
		[NotNull, Pure]
		public static string Join<T>([NotNull, InstantHandle] this IEnumerable<T> values) =>
			string.Join("", values);

		/// <summary>
		/// Returns length of argument, even if argument is null.
		/// </summary>
		/// <param name="str">The string.</param>
		/// <returns>Length of the <paramref name="str"/> or 0, if <paramref name="str"/> is null.</returns>
		[Pure]
		public static int Length([CanBeNull] this string str) => str?.Length ?? 0;

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

		/// <summary>
		/// Converts the specified string, which encodes binary data as base-64 digits, to an equivalent byte array.
		/// </summary>
		/// <param name="str">The string to convert.</param>
		/// <returns>An array of bytes that is equivalent to <paramref name="str"/>.</returns>
		[NotNull]
		[Pure]
		public static byte[] FromBase64([NotNull] this string str) => Convert.FromBase64String(str);

		/// <summary>
		/// Converts an array of bytes to its equivalent string representation that is encoded with base-64 digits.
		/// A parameter specifies whether to insert line breaks in the return value.
		/// </summary>
		/// <param name="data">an array of bytes.</param>
		/// <param name="options">
		/// <see cref="Base64FormattingOptions.InsertLineBreaks"/> to insert a line break every 76 characters,
		/// or <see cref="Base64FormattingOptions.None"/> to not insert line breaks.
		/// </param>
		/// <returns>The string representation in base 64 of the elements in <paramref name="data"/>.</returns>
		[NotNull]
		[Pure]
		public static string ToBase64(
			[NotNull] this byte[] data,
			Base64FormattingOptions options = Base64FormattingOptions.None) =>
				Convert.ToBase64String(data, options);

		/// <summary>
		/// Encodes all the characters in the specified string into a sequence of bytes.
		/// </summary>
		/// <param name="str">The string containing the characters to encode.</param>
		/// <param name="encoding">Encoding to convert.</param>
		/// <returns>A byte array containing the results of encoding the specified set of characters.</returns>
		[NotNull]
		[Pure]
		public static byte[] ToBytes([NotNull] this string str, [NotNull] Encoding encoding)
		{
			Code.NotNull(str, nameof(str));
			Code.NotNull(encoding, nameof(encoding));

			return encoding.GetBytes(str);
		}

		/// <summary>
		/// Encodes all the characters in the specified string into a sequence of bytes using UTF-8 encoding.
		/// </summary>
		/// <param name="str">The string containing the characters to encode.</param>
		/// <returns>A byte array containing the results of encoding the specified set of characters.</returns>
		[NotNull]
		[Pure]
		public static byte[] ToBytes([NotNull] this string str) => ToBytes(str, Encoding.UTF8);

		/// <summary>
		/// Converts the string representation of a number in a specified style and culture-specific format to its 32-bit
		/// signed integer equivalent. A return value indicates whether the conversion succeeded.
		/// </summary>
		/// <param name="str">
		/// A string containing a number to convert. The string is interpreted using the style specified by
		/// <paramref name="numberStyle"/>.
		/// </param>
		/// <param name="numberStyle">
		/// A bitwise combination of enumeration values that indicates the style elements that can be present in
		/// <paramref name="str"/>. Default value is Integer.
		/// </param>
		/// <param name="provider">
		/// An object that supplies culture-specific formatting information about <paramref name="str"/>.
		/// </param>
		/// <returns>
		/// When this method returns, contains the 32-bit signed integer value equivalent of the number contained in
		/// <paramref name="str"/>, if the conversion succeeded, or null if the conversion failed. The conversion fails if
		/// the <paramref name="str"/> parameter is null or String.Empty, is not in a format compliant withstyle, or
		/// represents a number less than <see cref="int.MinValue"/> or greater than <see cref="int.MaxValue"/>.
		/// </returns>
		[Pure]
		public static int? ToInt(
			[CanBeNull] this string str,
			NumberStyles numberStyle = NumberStyles.Integer,
			[CanBeNull] IFormatProvider provider = null)
		{
			int result;
			return int.TryParse(str, numberStyle, provider, out result) ? (int?)result : null;
		}

		/// <summary>
		/// Converts the string representation of a number in a specified style and culture-specific format to its
		/// <see cref="double"/> equivalent. A return value indicates whether the conversion succeeded.
		/// </summary>
		/// <param name="str">
		/// A string containing a number to convert. The string is interpreted using the style specified by
		/// <paramref name="numberStyle"/>.
		/// </param>
		/// <param name="numberStyle">
		/// A bitwise combination of enumeration values that indicates the style elements that can be present in
		/// <paramref name="str"/>. Default value is Float.
		/// </param>
		/// <param name="provider">
		/// An object that supplies culture-specific formatting information about <paramref name="str"/>.
		/// </param>
		/// <returns>
		/// When this method returns, contains the <see cref="double"/> value equivalent of the number contained in
		/// <paramref name="str"/>, if the conversion succeeded, or null if the conversion failed. The conversion fails if
		/// the <paramref name="str"/> parameter is null or String.Empty, is not in a format compliant withstyle, or
		/// represents a number less than <see cref="double.MinValue"/> or greater than <see cref="double.MaxValue"/>.
		/// </returns>
		[Pure]
		public static double? ToDouble(
			[CanBeNull] this string str,
			NumberStyles numberStyle = NumberStyles.Float,
			[CanBeNull] IFormatProvider provider = null)
		{
			double result;
			return double.TryParse(str, numberStyle, provider, out result) ? (int?)result : null;
		}
	}
}