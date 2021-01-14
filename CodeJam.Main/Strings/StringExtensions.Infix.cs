using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;

using JetBrains.Annotations;
#if NET40_OR_GREATER || TARGETS_NETSTANDARD || TARGETS_NETCOREAPP
using StringEx = System.String;

#else
using StringEx = System.StringEx;

#endif

namespace CodeJam.Strings
{
	static partial class StringExtensions
	{
		/// <summary>
		/// Infix form of <see cref="string.IsNullOrEmpty"/>.
		/// </summary>
		/// <param name="str">The string.</param>
		/// <returns><c>true</c> if <paramref name="str"/> is null or empty; otherwise, <c>false</c>.</returns>
		[Pure, System.Diagnostics.Contracts.Pure]
		public static bool IsNullOrEmpty([NotNullWhen(false)] this string? str)
		{
			// DONTTOUCH: Do not remove return statements
			// https://github.com/dotnet/coreclr/issues/914

			if (str == null || 0u >= (uint)str.Length)
				return true;

			return false;
		}

		/// <summary>
		/// Returns true if argument is not null nor empty.
		/// </summary>
		/// <param name="str">The string.</param>
		/// <returns><c>true</c> if <paramref name="str"/> is not null nor empty; otherwise, <c>false</c>.</returns>
		[Pure, System.Diagnostics.Contracts.Pure]
		public static bool NotNullNorEmpty([NotNullWhen(true)] this string? str) => !str.IsNullOrEmpty();

		/// <summary>
		/// Infix form of string.IsNullOrWhiteSpace.
		/// </summary>
		/// <param name="str">The string.</param>
		/// <returns>
		/// <c>true</c> if <paramref name="str"/> is null, empty or contains only whitespaces; otherwise <c>false</c>.
		/// </returns>
		[Pure, System.Diagnostics.Contracts.Pure]
		// ReSharper disable once BuiltInTypeReferenceStyle
		public static bool IsNullOrWhiteSpace([NotNullWhen(false)] this string? str) =>
			StringEx.IsNullOrWhiteSpace(
				str
#if LESSTHAN_NET47
					!
#endif
				);

		/// <summary>
		/// Returns an empty string for null value.
		/// </summary>
		/// <param name="str">The string.</param>
		/// <returns>The string or <see cref="string.Empty"/> if the string is <c>null</c>.</returns>
		[Pure, System.Diagnostics.Contracts.Pure]
		public static string EmptyIfNull(this string? str) => str ?? string.Empty;

		/// <summary>
		/// Returns <c>null</c> for empty strings.
		/// </summary>
		/// <param name="str">The string.</param>
		/// <returns>The string or <c>null</c> if the string is empty.</returns>
		[Pure, System.Diagnostics.Contracts.Pure]
		public static string? NullIfEmpty(this string? str) => str.IsNullOrEmpty() ? null : str;

		/// <summary>
		/// Returns <c>null</c> for empty or whitespace strings.
		/// </summary>
		/// <param name="str">The string.</param>
		/// <returns>The string or <c>null</c> if the string is empty.</returns>
		[Pure, System.Diagnostics.Contracts.Pure]
		[return: NotNullIfNotNull("str")]
		public static string? NullIfWhiteSpace(this string? str) => str.IsNullOrWhiteSpace() ? null : str;

		/// <summary>
		/// Returns true if argument is not null nor whitespace.
		/// </summary>
		/// <param name="str">The string.</param>
		/// <returns>
		/// <c>true</c> if <paramref name="str"/> is not null, nor empty or contains not only whitespaces;
		/// otherwise <c>false</c>.
		/// </returns>
		[Pure, System.Diagnostics.Contracts.Pure]
		// ReSharper disable once BuiltInTypeReferenceStyle
		public static bool NotNullNorWhiteSpace([NotNullWhen(true)] this string? str) =>
			!StringEx.IsNullOrWhiteSpace(
				str
#if LESSTHAN_NET47
					!
#endif
				);

		/// <summary>
		/// Replaces one or more format items in a specified string with the string representation of a specified object.
		/// </summary>
		/// <param name="format">A composite format string.</param>
		/// <param name="arg">The object to format.</param>
		/// <returns>
		/// A copy of <paramref name="format"/> in which any format items are replaced by the string representation of
		/// <paramref name="arg"/>.
		/// </returns>
		[Pure, System.Diagnostics.Contracts.Pure]
		[StringFormatMethod("format")]
		public static string FormatWith(this string format, object? arg) => string.Format(format, arg);

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
		[Pure, System.Diagnostics.Contracts.Pure]
		[StringFormatMethod("format")]
		public static string FormatWith(this string format, object? arg0, object? arg1) =>
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
		[Pure, System.Diagnostics.Contracts.Pure]
		[StringFormatMethod("format")]
		public static string FormatWith(this string format, object? arg0, object? arg1, object? arg2) =>
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
		[Pure, System.Diagnostics.Contracts.Pure]
		[StringFormatMethod("format")]
		public static string FormatWith(this string format, params object[] args) =>
			string.Format(format, args);

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
		[Pure, System.Diagnostics.Contracts.Pure]
		public static string Join(this string[] values, string? separator) =>
			string.Join(separator, values);

		/// <summary>
		/// Concatenates the members of a constructed <see cref="IEnumerable{T}"/> collection of type <see cref="string"/>,
		/// using the specified separator between each member.
		/// </summary>
		/// <remarks>
		/// Infix form of string.Join(string,IEnumerable{string}).
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
		[Pure, System.Diagnostics.Contracts.Pure]
		public static string Join([InstantHandle] this IEnumerable<string> values, string? separator) =>
			// ReSharper disable once BuiltInTypeReferenceStyle
			StringEx.Join(
				separator
#if LESSTHAN_NET47
					!
#endif
				,
				values);

		/// <summary>
		/// Concatenates the members of a collection, using the specified separator between each member.
		/// </summary>
		/// <typeparam name="T">Type of value</typeparam>
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
		[Pure, System.Diagnostics.Contracts.Pure]
		public static string Join<T>([InstantHandle] this IEnumerable<T> values, string? separator) =>
			// ReSharper disable once BuiltInTypeReferenceStyle
			StringEx.Join(
				separator
#if LESSTHAN_NET47
					!
#endif
				,
				values);

		/// <summary>
		/// Concatenates the members of a collection.
		/// </summary>
		/// <typeparam name="T">Type of value</typeparam>
		/// <param name="values">A collection that contains the strings to concatenate.</param>
		/// <returns>
		/// A string that consists of the members of <paramref name="values"/>.
		/// If <paramref name="values"/> has no members, the method returns <see cref="string.Empty"/>.
		/// </returns>
		[Pure, System.Diagnostics.Contracts.Pure]
		public static string Join<T>([InstantHandle] this IEnumerable<T> values) =>
			// ReSharper disable once BuiltInTypeReferenceStyle
			StringEx.Join("", values);

		/// <summary>
		/// Returns length of argument, even if argument is null.
		/// </summary>
		/// <param name="str">The string.</param>
		/// <returns>Length of the <paramref name="str"/> or 0, if <paramref name="str"/> is null.</returns>
		[Pure, System.Diagnostics.Contracts.Pure]
		public static int Length(this string? str) => str?.Length ?? 0;

		/// <summary>
		/// Converts the specified string, which encodes binary data as base-64 digits, to an equivalent byte array.
		/// </summary>
		/// <param name="str">The string to convert.</param>
		/// <returns>An array of bytes that is equivalent to <paramref name="str"/>.</returns>
		[Pure, System.Diagnostics.Contracts.Pure]
		public static byte[] FromBase64(this string str) => Convert.FromBase64String(str);

#if TARGETS_NET || NETSTANDARD20_OR_GREATER || NETCOREAPP20_OR_GREATER // PUBLIC_API_CHANGES
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
		[Pure, System.Diagnostics.Contracts.Pure]
		public static string ToBase64(this byte[] data, Base64FormattingOptions options) =>
			Convert.ToBase64String(data, options);
#endif

		/// <summary>
		/// Converts an array of bytes to its equivalent string representation that is encoded with base-64 digits.
		/// A parameter specifies whether to insert line breaks in the return value.
		/// </summary>
		/// <param name="data">an array of bytes.</param>
		/// <returns>The string representation in base 64 of the elements in <paramref name="data"/>.</returns>
		[Pure, System.Diagnostics.Contracts.Pure]
		public static string ToBase64(this byte[] data) =>
			Convert.ToBase64String(data);

		/// <summary>
		/// Encodes all the characters in the specified string into a sequence of bytes.
		/// </summary>
		/// <param name="str">The string containing the characters to encode.</param>
		/// <param name="encoding">Encoding to convert.</param>
		/// <returns>A byte array containing the results of encoding the specified set of characters.</returns>
		[Pure, System.Diagnostics.Contracts.Pure]
		public static byte[] ToBytes(this string str, Encoding encoding)
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
		[Pure, System.Diagnostics.Contracts.Pure]
		public static byte[] ToBytes(this string str) => ToBytes(str, Encoding.UTF8);
	}
}