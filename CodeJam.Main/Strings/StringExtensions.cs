using System;
using System.Collections.Generic;
using System.Globalization;

using JetBrains.Annotations;

namespace CodeJam.Strings
{
	/// <summary>
	/// <see cref="string"/> class extensions.
	/// </summary>
	[PublicAPI]
	public partial class StringExtensions
	{
		/// <summary>
		/// Determines whether the beginning of the string instance matches the specified character.
		/// </summary>
		/// <param name="value">The string to test.</param>
		/// <param name="ch">The character to compare.</param>
		/// <returns>
		/// <c>true</c> if <paramref name="ch"/> matches the beginning of the <paramref name="value"/>; otherwise, <c>false</c>.
		/// </returns>
		[Pure][System.Diagnostics.Contracts.Pure]
		public static bool StartsWith(this string value, char ch)
		{
			Code.NotNull(value, nameof(value));

			// PERF: Range check elimination
			// ===========================================================
			//
			// 1. Bounds check with Int32 constant value not eliminated
			// return value.Length != 0
			//     && value[0] == ch; // <== Bounds check
			// }
			//
			// 2. Bounds check with the flipped condition not eliminated
			// return (uint)value.Length > 0u
			//     && value[0] == ch; <== Bounds check

			return 0u < (uint)value.Length && value[0] == ch;
		}

		/// <summary>
		/// Determines whether the end of the string instance matches the specified character.
		/// </summary>
		/// <param name="value">The string to test.</param>
		/// <param name="ch">The character to compare.</param>
		/// <returns>
		/// <c>true</c> if <paramref name="ch"/> matches the end of the <paramref name="value"/>; otherwise, <c>false</c>.
		/// </returns>
		[Pure][System.Diagnostics.Contracts.Pure]
		public static bool EndsWith([NotNull] this string value, char ch)
		{
			Code.NotNull(value, nameof(value));

			// PERF: Range check elimination
			// ===========================================================
			//
			// 1. Bounds check with Int32 constant value not eliminated
			// return value.Length != 0
			//     && value[value.Length - 1] == ch; // <== Bounds check
			// }
			//
			// 2. Bounds check not eliminated
			// return 0u < (uint)value.Length
			//     && value[value.Length - 1] == ch; <== Bounds check
			//

			// In this case the `value.Length - 1u` is CSE (Common subexpression)
			// and JIT eliminates range checking
			return (uint)value.Length - 1u < (uint)value.Length && value[value.Length - 1] == ch;
		}

		/// <summary>
		/// Retrieves a substring from <paramref name="str"/>.
		/// </summary>
		/// <param name="str">
		/// String to retrieve substring from.
		/// </param>
		/// <param name="origin">
		/// Specifies the beginning, or the end as a reference point for offset, using a value of type
		/// <see cref="StringOrigin"/>.
		/// </param>
		/// <param name="length">The number of characters in the substring.</param>
		/// <returns>
		/// A string that is equivalent to the substring of length <paramref name="length"/> that begins at
		/// <paramref name="origin"/> in  <paramref name="str"/>, or Empty if length of <paramref name="str"/>
		/// or <paramref name="length"/> is zero.
		/// </returns>
		[Pure][System.Diagnostics.Contracts.Pure]
		public static string Substring(this string str, StringOrigin origin, int length)
		{
			Code.NotNull(str, nameof(str));

			// Fast path
			var strLen = str.Length;
			if (strLen == 0 || length == 0)
				return "";
			if (length >= strLen)
				return str;
			return
				origin switch
				{
					StringOrigin.Begin => str.Substring(0, length),
					StringOrigin.End => str.Substring(strLen - length, length),
					_ => throw CodeExceptions.Argument(nameof(origin), $"Invalid {nameof(StringOrigin)} value.")
				};
		}

		/// <summary>
		/// Retrieves prefix of length <paramref name="length"/>.
		/// </summary>
		/// <param name="str">String to retrieve prefix from.</param>
		/// <param name="length">The number of characters in the substring.</param>
		/// <returns>
		/// Prefix of specified length, or <paramref name="str"/> itself, if total length less than required.
		/// </returns>
		[Pure][System.Diagnostics.Contracts.Pure]
		public static string Prefix(this string str, int length) => str.Substring(StringOrigin.Begin, length);

		/// <summary>
		/// Retrieves prefix of length <paramref name="length"/>.
		/// </summary>
		/// <param name="str">String to retrieve suffix from.</param>
		/// <param name="length">The number of characters in the substring.</param>
		/// <returns>
		/// Suffix of specified length, or <paramref name="str"/> itself, if total length less than required.
		/// </returns>
		[Pure][System.Diagnostics.Contracts.Pure]
		public static string Suffix(this string str, int length) => str.Substring(StringOrigin.End, length);

		/// <summary>
		/// Trims <paramref name="str"/> prefix if it equals to <paramref name="prefix"/>.
		/// </summary>
		/// <param name="str">String to trim.</param>
		/// <param name="prefix">Prefix to trim.</param>
		/// <returns>Trimmed <paramref name="str"/>, or original <paramref name="str"/> if prefix not exists.</returns>
		[Pure][System.Diagnostics.Contracts.Pure]
		public static string TrimPrefix(this string str, string? prefix) =>
			TrimPrefix(str, prefix, StringComparer.CurrentCulture);

		/// <summary>
		/// Trims <paramref name="str"/> prefix if it equals to <paramref name="prefix"/>.
		/// </summary>
		/// <param name="str">String to trim.</param>
		/// <param name="prefix">Prefix to trim.</param>
		/// <param name="comparer">Comparer to compare value of prefix.</param>
		/// <returns>Trimmed <paramref name="str"/>, or original <paramref name="str"/> if prefix not exists.</returns>
		[Pure][System.Diagnostics.Contracts.Pure]
		public static string TrimPrefix(
			this string str,
			string? prefix,
			IEqualityComparer<string>? comparer)
		{
			Code.NotNull(str, nameof(str));
			comparer ??= EqualityComparer<string>.Default;

			// FastPath
			if (prefix == null)
				return str;
			var prefixLen = prefix.Length;
			if (prefixLen == 0 || str.Length < prefixLen)
				return str;

			var actPrefix = str.Prefix(prefixLen);
			return !comparer.Equals(prefix, actPrefix) ? str : str.Substring(prefixLen);
		}

		/// <summary>
		/// Trims <paramref name="str"/> suffix if it equals to <paramref name="suffix"/>.
		/// </summary>
		/// <param name="str">String to trim.</param>
		/// <param name="suffix">Suffix to trim.</param>
		/// <param name="comparer">Comparer to compare value of suffix.</param>
		/// <returns>
		/// Trimmed <paramref name="str"/>, or original <paramref name="str"/> if suffix does not exists.
		/// </returns>
		[Pure][System.Diagnostics.Contracts.Pure]
		public static string TrimSuffix(
			this string str,
			string? suffix,
			IEqualityComparer<string>? comparer)
		{
			Code.NotNull(str, nameof(str));

			comparer ??= EqualityComparer<string>.Default;

			// FastPath
			if (suffix == null)
				return str;
			var strLen = str.Length;
			var suffixLen = suffix.Length;
			if (suffixLen == 0 || strLen < suffixLen)
				return str;

			var actPrefix = str.Suffix(suffixLen);
			return !comparer.Equals(suffix, actPrefix) ? str : str.Substring(0, strLen - suffixLen);
		}

		/// <summary>
		/// Trims <paramref name="str"/> prefix if it equals to <paramref name="suffix"/>.
		/// </summary>
		/// <param name="str">String to trim.</param>
		/// <param name="suffix">Suffix to trim.</param>
		/// <returns>
		/// Trimmed <paramref name="str"/>, or original <paramref name="str"/> if suffix does not exists.
		/// </returns>
		[Pure][System.Diagnostics.Contracts.Pure]
		public static string TrimSuffix(this string str, string? suffix) =>
			TrimSuffix(str, suffix, StringComparer.CurrentCulture);

		[ItemNotNull]
		private static readonly string[] _sizeSuffixes = { "bytes", "KB", "MB", "GB", "TB", "PB", "EB" };

		/// <summary>
		/// Returns size in bytes string representation.
		/// </summary>
		/// <param name="value">Value to represent.</param>
		/// <returns>Value as size in bytes</returns>
		[Pure][System.Diagnostics.Contracts.Pure]
		public static string ToByteSizeString(this long value) => ToByteSizeString(value, null);

		/// <summary>
		/// Returns size in bytes string representation.
		/// </summary>
		/// <param name="value">Value to represent.</param>
		/// <returns>Value as size in bytes</returns>
		[Pure][System.Diagnostics.Contracts.Pure]
		public static string ToByteSizeString(this int value) => ToByteSizeString(value, null);

		/// <summary>
		/// Returns size in bytes string representation.
		/// </summary>
		/// <param name="value">Value to represent.</param>
		/// <param name="provider">Format provider for number part of value</param>
		/// <returns>Value as size in bytes</returns>
		[Pure][System.Diagnostics.Contracts.Pure]
		public static string ToByteSizeString(this long value, IFormatProvider? provider)
		{
			switch (value)
			{
				case < 0:
					return "-" + (-value).ToByteSizeString(provider);
				case 0:
					return "0";
			}

			var i = 0;
			var d = (decimal)value;
			while (Math.Round(d / 1024) >= 1)
			{
				d /= 1024;
				i++;
			}

			return string.Format(provider, "{0:#.##} {1}", d, _sizeSuffixes[i]);
		}

		/// <summary>
		/// Returns size in bytes string representation.
		/// </summary>
		/// <param name="value">Value to represent.</param>
		/// <param name="provider">Format provider for number part of value</param>
		/// <returns>Value as size in bytes</returns>
		[Pure][System.Diagnostics.Contracts.Pure]
		public static string ToByteSizeString(this int value, IFormatProvider? provider) =>
			ToByteSizeString((long)value, provider);

		/// <summary>
		/// Splits <paramref name="source"/> and returns whitespace trimmed parts.
		/// </summary>
		/// <param name="source">Source string.</param>
		/// <param name="separators">Separator chars</param>
		/// <returns>Enumeration of parts.</returns>
		[Pure][System.Diagnostics.Contracts.Pure]
		public static IEnumerable<string> SplitWithTrim(this string source, params char[] separators)
		{
			Code.NotNull(source, nameof(source));

			// TODO: For performance reasons must be reimplemented using FSM parser.
			var parts = source.Split(separators);
			foreach (var part in parts)
				if (!part.IsNullOrWhiteSpace())
					yield return part.Trim();
		}

		/// <summary>
		/// Creates hex representation of byte array.
		/// </summary>
		/// <param name="data">Byte array.</param>
		/// <returns>
		/// <paramref name="data"/> represented as a series of hexadecimal representations.
		/// </returns>
		/// <exception cref="ArgumentNullException"><paramref name="data"/> is null.</exception>
		[Pure][System.Diagnostics.Contracts.Pure]
		public static unsafe string ToHexString(this byte[] data)
		{
			Code.NotNull(data, nameof(data));

			if (data.Length == 0)
				return string.Empty;

			var length = data.Length * 2;
			var result = new string('\0', length);

			fixed (char* res = result)
			fixed (byte* buf = &data[0])
			{
				var pres = res;

				for (var i = 0; i < data.Length; pres += 2, i++)
				{
					var b = buf[i];
					var n = b >> 4;
					pres[0] = (char)(55 + n + (((n - 10) >> 31) & -7));

					n = b & 0xF;
					pres[1] = (char)(55 + n + (((n - 10) >> 31) & -7));
				}

				return result;
			}
		}

		/// <summary>
		/// Creates hex representation of byte array.
		/// </summary>
		/// <param name="data">Byte array.</param>
		/// <param name="byteSeparator">Separator between bytes. If null - no separator used.</param>
		/// <returns>
		/// <paramref name="data"/> represented as a series of hexadecimal representations divided by separator.
		/// </returns>
		/// <exception cref="ArgumentNullException"><paramref name="data"/> is null.</exception>
		[Pure][System.Diagnostics.Contracts.Pure]
		public static unsafe string ToHexString(this byte[] data, string? byteSeparator)
		{
			Code.NotNull(data, nameof(data));

			if (data.Length == 0)
				return string.Empty;

			var hasSeparator = byteSeparator.NotNullNorEmpty();
			var length = data.Length * 2;
			if (hasSeparator)
				length += (data.Length - 1) * byteSeparator!.Length;

			var result = new string('\0', length);

			fixed (char* res = result, sep = byteSeparator)
			fixed (byte* buf = &data[0])
			{
				var pres = res;

				for (var i = 0; i < data.Length; pres += 2, i++)
				{
					if (hasSeparator & (i != 0))
						for (var j = 0; j < byteSeparator!.Length; pres++, j++)
							pres[0] = sep[j];

					var b = buf[i];
					var n = b >> 4;
					pres[0] = (char)(55 + n + (((n - 10) >> 31) & -7));

					n = b & 0xF;
					pres[1] = (char)(55 + n + (((n - 10) >> 31) & -7));
				}

				return result;
			}
		}

		/// <summary>
		/// Removes one set of leading and trailing double quote characters, if both are present.
		/// </summary>
		/// <param name="arg">String to unquote.</param>
		/// <returns>
		/// Unquoted <paramref name="arg"/>, if <paramref name="arg"/> is quoted, or <paramref name="arg"/> itself.
		/// </returns>
		[Pure][System.Diagnostics.Contracts.Pure]
		public static string Unquote(this string arg) => Unquote(arg, out _);

		/// <summary>
		/// Removes one set of leading and trailing double quote characters, if both are present.
		/// </summary>
		/// <param name="arg">String to unquote.</param>
		/// <param name="quoted">Set to true, if <paramref name="arg"/> was quoted.</param>
		/// <returns>
		/// Unquoted <paramref name="arg"/>, if <paramref name="arg"/> is quoted, or <paramref name="arg"/> itself.
		/// </returns>
		[Pure][System.Diagnostics.Contracts.Pure]
		public static string Unquote(this string arg, out bool quoted) => Unquote(arg, '"', out quoted);

		/// <summary>
		/// Removes one set of leading and trailing <paramref name="quotationChar"/>, if both are present.
		/// </summary>
		/// <param name="arg">String to unquote.</param>
		/// <param name="quotationChar">Quotation char</param>
		/// <param name="quoted">Set to true, if <paramref name="arg"/> was quoted.</param>
		/// <returns>
		/// Unquoted <paramref name="arg"/>, if <paramref name="arg"/> is quoted, or <paramref name="arg"/> itself.
		/// </returns>
		[Pure][System.Diagnostics.Contracts.Pure]
		public static string Unquote(this string arg, char quotationChar, out bool quoted)
		{
			Code.NotNull(arg, nameof(arg));

			if (arg.Length > 1 && arg[0] == quotationChar && arg[arg.Length - 1] == quotationChar)
			{
				quoted = true;
				return arg.Substring(1, arg.Length - 2);
			}
			quoted = false;
			return arg;
		}

		/// <summary>
		/// Removes substring from provided strings.
		/// </summary>
		/// <param name="str">String to remove.</param>
		/// <param name="toRemoveStrings">Substrings to remove.</param>
		/// <returns>New string without provided substrings.</returns>
		[Pure][System.Diagnostics.Contracts.Pure]
		public static string Remove(this string str, params string[] toRemoveStrings)
		{
			Code.NotNull(str,             nameof(str));
			Code.NotNull(toRemoveStrings, nameof(toRemoveStrings));

			foreach (var removeString in toRemoveStrings)
				str = str.Replace(removeString, "");

			return str;
		}

		/// <summary>
		/// Culture invariant version of <see cref="string.Format(string, object)"/>
		/// </summary>
		/// <param name="format">A composite format string. </param>
		/// <param name="arg0">The object to format. </param>
		/// <returns>A copy of <paramref name="format" /> in which any format items are replaced by the string representation of <paramref name="arg0" />.</returns>
		[NotNull, Pure]
		public static string FormatInvariant([NotNull] this string format, object arg0) =>
			string.Format(CultureInfo.InvariantCulture, format, arg0);

		/// <summary>
		/// Culture invariant version of <see cref="string.Format(string, object, object)"/>
		/// </summary>
		/// <param name="format">A composite format string. </param>
		/// <param name="arg0">The first object to format. </param>
		/// <param name="arg1">The second object to format. </param>
		/// <returns>A copy of <paramref name="format" /> in which format items are replaced by the string representations of <paramref name="arg0" /> and <paramref name="arg1" />.</returns>
		[NotNull, Pure]
		public static string FormatInvariant([NotNull] this string format, object arg0, object arg1) =>
			string.Format(CultureInfo.InvariantCulture, format, arg0, arg1);

		/// <summary>
		/// Culture invariant version of <see cref="string.Format(string, object, object, object)"/>
		/// </summary>
		/// <param name="format">A composite format string. </param>
		/// <param name="arg0">The first object to format. </param>
		/// <param name="arg1">The second object to format. </param>
		/// <param name="arg2">The third object to format. </param>
		/// <returns>A copy of <paramref name="format" /> in which the format items have been replaced by the string representations of <paramref name="arg0" />, <paramref name="arg1" />, and <paramref name="arg2" />.</returns>
		[NotNull, Pure]
		public static string FormatInvariant([NotNull] this string format, object arg0, object arg1, string arg2) =>
			string.Format(CultureInfo.InvariantCulture, format, arg0, arg1, arg2);

		/// <summary>
		/// Culture invariant version of <see cref="string.Format(string, object[])"/>
		/// </summary>
		/// <param name="format">A composite format string. </param>
		/// <param name="args">An object array that contains zero or more objects to format. </param>
		/// <returns>A copy of <paramref name="format" /> in which the format items have been replaced by the string representation of the corresponding objects in <paramref name="args" />.</returns>
		[NotNull, Pure]
		public static string FormatInvariant([NotNull] this string format,params object[] args) =>
			string.Format(CultureInfo.InvariantCulture, format, args);

#if TARGETS_NET || NETSTANDARD20_OR_GREATER || NETCOREAPP20_OR_GREATER // PUBLIC_API_CHANGES
		/// <summary>
		/// Determines whether the beginning of this string instance matches the specified string when compared using the
		/// invariant culture.
		/// </summary>
		/// <param name="str">String to check.</param>
		/// <param name="value">The string to compare.</param>
		/// <returns><c>true</c> if this instance begins with value; otherwise, <c>false</c>. </returns>
		[Pure][System.Diagnostics.Contracts.Pure]
		public static bool StartsWithInvariant([NotNull] this string str, [NotNull] string value) =>
			str.StartsWith(value, StringComparison.InvariantCulture);

		/// <summary>
		/// Determines whether the beginning of this string instance matches the specified string when compared using the
		/// ordinal culture.
		/// </summary>
		/// <param name="str">String to check.</param>
		/// <param name="value">The string to compare.</param>
		/// <returns><c>true</c> if this instance begins with value; otherwise, <c>false</c>. </returns>
		[Pure][System.Diagnostics.Contracts.Pure]
		public static bool StartsWithOrdinal([NotNull] this string str, [NotNull] string value) =>
			str.StartsWith(value, StringComparison.Ordinal);

		/// <summary>
		/// Determines whether the end of this string instance matches the specified string when compared using the
		/// invariant culture.
		/// </summary>
		/// <param name="str">String to check.</param>
		/// <param name="value">The string to compare.</param>
		/// <returns><c>true</c> if this instance ends with value; otherwise, <c>false</c>. </returns>
		[Pure][System.Diagnostics.Contracts.Pure]
		public static bool EndsWithInvariant([NotNull] this string str, [NotNull] string value) =>
			str.EndsWith(value, StringComparison.InvariantCulture);

		/// <summary>
		/// Determines whether the end of this string instance matches the specified string when compared using the
		/// ordinal culture.
		/// </summary>
		/// <param name="str">String to check.</param>
		/// <param name="value">The string to compare.</param>
		/// <returns><c>true</c> if this instance ends with value; otherwise, <c>false</c>. </returns>
		[Pure][System.Diagnostics.Contracts.Pure]
		public static bool EndsWithOrdinal([NotNull] this string str, [NotNull] string value) =>
			str.EndsWith(value, StringComparison.Ordinal);

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
		[Pure][System.Diagnostics.Contracts.Pure]
		public static int IndexOfInvariant([NotNull] this string str, [NotNull] string value) =>
			str.IndexOf(value, StringComparison.InvariantCulture);

		/// <summary>
		/// Reports the zero-based index of the first occurrence of the specified string in the <paramref name="str"/>
		/// using the ordinal culture.
		/// </summary>
		/// <param name="str">The string to check.</param>
		/// <param name="value">The string to seek.</param>
		/// <returns>
		/// The index position of the value parameter if that string is found, or -1 if it is not. If value is <see cref="string.Empty"/>,
		/// the return value is 0
		/// </returns>
		[Pure][System.Diagnostics.Contracts.Pure]
		public static int IndexOfOrdinal([NotNull] this string str, [NotNull] string value) =>
			str.IndexOf(value, StringComparison.Ordinal);

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
		[Pure][System.Diagnostics.Contracts.Pure]
		public static int IndexOfInvariant([NotNull] this string str, [NotNull] string value, [NonNegativeValue] int startIndex) =>
			str.IndexOf(value, startIndex, StringComparison.InvariantCulture);

		/// <summary>
		/// Reports the zero-based index of the first occurrence of the specified string in the <paramref name="str"/>
		/// using the ordinal culture. Parameter specify the starting search position in the current string.
		/// </summary>
		/// <param name="str">The string to check.</param>
		/// <param name="value">The string to seek.</param>
		/// <param name="startIndex">The search starting position.</param>
		/// <returns>
		/// The index position of the value parameter if that string is found, or -1 if it is not. If value is <see cref="string.Empty"/>,
		/// the return value is 0
		/// </returns>
		[Pure][System.Diagnostics.Contracts.Pure]
		public static int IndexOfOrdinal([NotNull] this string str, [NotNull] string value, [NonNegativeValue] int startIndex) =>
			str.IndexOf(value, startIndex, StringComparison.Ordinal);

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
		[Pure][System.Diagnostics.Contracts.Pure]
		public static int IndexOfInvariant([NotNull] this string str, [NotNull] string value, [NonNegativeValue] int startIndex, [NonNegativeValue] int count) =>
			str.IndexOf(value, startIndex, count, StringComparison.InvariantCulture);

		/// <summary>
		/// Reports the zero-based index of the first occurrence of the specified string in the <paramref name="str"/>
		/// using the ordinal culture. Parameters specify the starting search position in the current string and
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
		[Pure][System.Diagnostics.Contracts.Pure]
		public static int IndexOfOrdinal([NotNull] this string str, [NotNull] string value, [NonNegativeValue] int startIndex, [NonNegativeValue] int count) =>
			str.IndexOf(value, startIndex, count, StringComparison.Ordinal);

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
		[Pure][System.Diagnostics.Contracts.Pure]
		public static int LastIndexOfInvariant([NotNull] this string str, [NotNull] string value) =>
			str.LastIndexOf(value, StringComparison.InvariantCulture);

		/// <summary>
		/// Reports the zero-based index of the last occurrence of the specified string in the <paramref name="str"/>
		/// using the ordinal culture.
		/// </summary>
		/// <param name="str">The string to check.</param>
		/// <param name="value">The string to seek.</param>
		/// <returns>
		/// The index position of the value parameter if that string is found, or -1 if it is not. If value is <see cref="string.Empty"/>,
		/// the return value is 0
		/// </returns>
		[Pure][System.Diagnostics.Contracts.Pure]
		public static int LastIndexOfOrdinal([NotNull] this string str, [NotNull] string value) =>
			str.LastIndexOf(value, StringComparison.Ordinal);

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
		[Pure][System.Diagnostics.Contracts.Pure]
		public static int LastIndexOfInvariant([NotNull] this string str, [NotNull] string value, [NonNegativeValue] int startIndex) =>
			str.LastIndexOf(value, startIndex, StringComparison.InvariantCulture);

		/// <summary>
		/// Reports the zero-based index of the last occurrence of the specified string in the <paramref name="str"/>
		/// using the ordinal culture. Parameter specify the starting search position in the current string.
		/// </summary>
		/// <param name="str">The string to check.</param>
		/// <param name="value">The string to seek.</param>
		/// <param name="startIndex">The search starting position.</param>
		/// <returns>
		/// The index position of the value parameter if that string is found, or -1 if it is not. If value is <see cref="string.Empty"/>,
		/// the return value is 0
		/// </returns>
		[Pure][System.Diagnostics.Contracts.Pure]
		public static int LastIndexOfOrdinal([NotNull] this string str, [NotNull] string value, [NonNegativeValue] int startIndex) =>
			str.LastIndexOf(value, startIndex, StringComparison.Ordinal);

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
		[Pure][System.Diagnostics.Contracts.Pure]
		public static int LastIndexOfInvariant(
			[NotNull] this string str,
			[NotNull] string value,
			[NonNegativeValue] int startIndex,
			[NonNegativeValue] int count) =>
				str.LastIndexOf(value, startIndex, count, StringComparison.InvariantCulture);

		/// <summary>
		/// Reports the zero-based index of the last occurrence of the specified string in the <paramref name="str"/>
		/// using the ordinal culture. Parameters specify the starting search position in the current string and
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
		[Pure][System.Diagnostics.Contracts.Pure]
		public static int LastIndexOfOrdinal(
			[NotNull] this string str,
			[NotNull] string value,
			[NonNegativeValue] int startIndex,
			[NonNegativeValue] int count) =>
				str.LastIndexOf(value, startIndex, count, StringComparison.Ordinal);

#endif
	}
}
