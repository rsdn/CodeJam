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
		/// Retrieves a substring from <paramref name="str"/>.
		/// </summary>
		/// <param name="str"></param>
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
		[NotNull]
		[Pure]
		public static string Substring([NotNull] this string str, StringOrigin origin, int length)
		{
			if (str == null) throw new ArgumentNullException(nameof(str));

			// Fast path
			var strLen = str.Length;
			if (strLen == 0 || length == 0)
				return "";
			if (length >= strLen)
				return str;
			switch (origin)
			{
				case StringOrigin.Begin:
					return str.Substring(0, length);
				case StringOrigin.End:
					return str.Substring(strLen - length, length);
				default:
					throw new ArgumentOutOfRangeException(nameof(origin), origin, null);
			}
		}

		/// <summary>
		/// Retireves prefix of length <paramref name="length"/>.
		/// </summary>
		/// <param name="str"></param>
		/// <param name="length">The number of characters in the substring.</param>
		[NotNull]
		[Pure]
		public static string Prefix([NotNull] this string str, int length) => str.Substring(StringOrigin.Begin, length);

		/// <summary>
		/// Retireves prefix of length <paramref name="length"/>.
		/// </summary>
		/// <param name="str"></param>
		/// <param name="length">The number of characters in the substring.</param>
		[NotNull]
		[Pure]
		public static string Suffix([NotNull] this string str, int length) => str.Substring(StringOrigin.End, length);

		/// <summary>
		/// Trims <paramref name="str"/> prefix if it equals to <paramref name="prefix"/>.
		/// </summary>
		[NotNull]
		[Pure]
		public static string TrimPrefix(
			[NotNull] this string str,
			[CanBeNull] string prefix,
			[NotNull] IEqualityComparer<string> comparer)
		{
			if (str == null) throw new ArgumentNullException(nameof(str));
			if (comparer == null) throw new ArgumentNullException(nameof(comparer));

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
		/// Trims <paramref name="str"/> prefix if it equals to <paramref name="prefix"/>.
		/// </summary>
		[NotNull]
		[Pure]
		public static string TrimPrefix([NotNull] this string str, [CanBeNull] string prefix) =>
			TrimPrefix(str, prefix, StringComparer.CurrentCulture);

		/// <summary>
		/// Trims <paramref name="str"/> suffix if it equals to <paramref name="suffix"/>.
		/// </summary>
		[NotNull]
		[Pure]
		public static string TrimSuffix(
			[NotNull] this string str,
			[CanBeNull] string suffix,
			[NotNull] IEqualityComparer<string> comparer)
		{
			if (str == null) throw new ArgumentNullException(nameof(str));
			if (comparer == null) throw new ArgumentNullException(nameof(comparer));

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
		[NotNull]
		[Pure]
		public static string TrimSuffix([NotNull] this string str, [CanBeNull] string suffix) =>
			TrimSuffix(str, suffix, StringComparer.CurrentCulture);

		private static readonly string[] _sizeSuffixes = { "bytes", "KB", "MB", "GB", "TB", "PB", "EB" };

		/// <summary>
		/// Returns size in bytes string representation.
		/// </summary>
		[NotNull]
		[Pure]
		public static string ToByteSizeString(this long value) => ToByteSizeString(value, CultureInfo.CurrentCulture);

		/// <summary>
		/// Returns size in bytes string representation.
		/// </summary>
		[NotNull]
		[Pure]
		public static string ToByteSizeString(this long value, [CanBeNull] IFormatProvider provider)
		{
			if (value < 0)
				return "-" + (-value).ToByteSizeString(provider);

			if (value == 0)
				return "0";

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
		/// Splits <paramref name="source"/> and returns whitespace trimmed parts.
		/// </summary>
		/// <param name="source">Source string.</param>
		/// <param name="separators">Separator chars</param>
		/// <returns>Enumeration of parts.</returns>
		[NotNull]
		[Pure]
		public static IEnumerable<string> SplitWithTrim([NotNull] this string source, params char[] separators)
		{
			Code.NotNull(source, nameof(source));

			// TODO: For performance reasons must be reimplemented using FSM parser.
			var parts = source.Split(separators);
			foreach (var part in parts)
			{
				if (!part.IsNullOrWhiteSpace())
					yield return part.Trim();
			}
		}

		/// <summary>
		/// Creates hex representation of byte array.
		/// </summary>
		/// <param name="data">Byte array.</param>
		/// <returns>
		/// <paramref name="data"/> represented as a series of hexadecimal representations.
		/// </returns>
		/// <exception cref="ArgumentNullException"><paramref name="data"/> is null.</exception>
		[NotNull]
		[Pure]
		public static unsafe string ToHexString([NotNull] this byte[] data)
		{
			Code.NotNull(data, nameof(data));
			if (data.Length == 0)
				return string.Empty;

			var length = data.Length << 1;
			var result = new string('\0', length);

			fixed (char* res = result)
			fixed (byte* buf = data)
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
			}

			return result;
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
		[NotNull]
		[Pure]
		public static unsafe string ToHexString([NotNull] this byte[] data, [CanBeNull] string byteSeparator)
		{
			Code.NotNull(data, nameof(data));

			if (data.Length == 0)
				return string.Empty;

			var hasSeparator = byteSeparator.NotNullNorEmpty();
			var length = data.Length * 2 + (hasSeparator ? (data.Length - 1) * byteSeparator.Length : 0);
			var result = new string('\0', length);

			fixed (char* res = result, sep = byteSeparator)
			fixed (byte* buf = data)
			{
				var pres = res;

				for (var i = 0; i < data.Length; pres += 2, i++)
				{
					if (hasSeparator & (i != 0))
						for (var j = 0; j < byteSeparator.Length; pres++, j++)
							pres[0] = sep[j];

					var b = buf[i];
					var n = b >> 4;
					pres[0] = (char)(55 + n + (((n - 10) >> 31) & -7));

					n = b & 0xF;
					pres[1] = (char)(55 + n + (((n - 10) >> 31) & -7));
				}
			}

			return result;
		}

		/// <summary>
		/// Remove one set of leading and trailing double quote characters, if both are present.
		/// </summary>
		public static string Unquote(this string arg)
		{
			bool quoted;
			return Unquote(arg, out quoted);
		}

		/// <summary>
		/// Remove one set of leading and trailing double quote characters, if both are present.
		/// </summary>
		public static string Unquote(this string arg, out bool quoted) => Unquote(arg, '"', out quoted);

		/// <summary>
		/// Remove one set of leading and trailing d<paramref name="quotationChar"/>, if both are present.
		/// </summary>
		public static string Unquote(this string arg, char quotationChar, out bool quoted)
		{
			if (arg.Length > 1 && arg[0] == quotationChar && arg[arg.Length - 1] == quotationChar)
			{
				quoted = true;
				return arg.Substring(1, arg.Length - 2);
			}
			quoted = false;
			return arg;
		}
	}
}