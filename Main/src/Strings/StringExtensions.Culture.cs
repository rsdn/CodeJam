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

		/// <summary>
		/// Culture invariant version of <see cref="DateTime.Parse(string)"/>
		/// </summary>
		/// <param name="s">String to parse.</param>
		/// <returns>Parsed value of <paramref name="s"/> according to rules of invariant culture.</returns>
		[Pure]
		public static DateTime ToDateTimeInvariant([NotNull] this string s) =>
			DateTime.Parse(s, CultureInfo.InvariantCulture);

		/// <summary>
		/// Culture invariant version of <see cref="byte.TryParse(string, out byte)"/>
		/// </summary>
		/// <param name="s">String to parse.</param>
		/// <returns>Parsed value of <paramref name="s"/> according to rules of invariant culture, or <c>null</c> if string can't be parsed.</returns>
		[Pure]
		public static DateTime? TryToDateTimeInvariant([NotNull] this string s)
		{
			DateTime res;
			if (DateTime.TryParse(s, CultureInfo.InvariantCulture, DateTimeStyles.None, out res))
				return res;
			return null;
		}

		/// <summary>
		/// Culture invariant version of <see cref="byte.TryParse(string, out byte)"/>
		/// </summary>
		/// <param name="s">String to parse.</param>
		/// <param name="style">value style</param>
		/// <returns>Parsed value of <paramref name="s"/> according to rules of invariant culture, or <c>null</c> if string can't be parsed.</returns>
		[Pure]
		public static DateTime? TryToDateTimeInvariant([NotNull] this string s, DateTimeStyles style)
		{
			DateTime res;
			if (DateTime.TryParse(s, CultureInfo.InvariantCulture, style, out res))
				return res;
			return null;
		}
	}
}
