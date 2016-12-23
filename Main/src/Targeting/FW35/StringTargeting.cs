#if FW35
// BASEDON: https://github.com/dotnet/coreclr/blob/b638af3a4dd52fa7b1ea1958164136c72096c25c/src/mscorlib/src/System/String.cs#L182

using System;

using JetBrains.Annotations;
using System.Collections.Generic;
using System.Text;

namespace CodeJam.Targeting
{
	/// <summary>
	/// Targeting methods for <see cref="string"/>.
	/// </summary>
	public static class StringTargeting
	{
		/// <summary>
		/// Indicates whether a specified string is <c>null</c>, empty, or consists only of white-space characters.
		/// </summary>
		/// <param name="value">The string to test.</param>
		/// <returns>
		/// <c>true</c> if the <paramref name="value"/> parameter is null or <see cref="String.Empty"/>,
		/// or if <paramref name="value"/> consists exclusively of white-space characters.
		/// </returns>
		[Pure]
		public static bool IsNullOrWhiteSpace(String value)
		{
			if (value == null) return true;

			foreach (var chr in value)
				if (!char.IsWhiteSpace(chr))
					return false;

			return true;
		}

		/// <summary>
		/// Concatenates the members of a constructed <see cref="IEnumerable{T}"/> collection of type <see cref="string"/>,
		/// using the specified separator between each member.
		/// </summary>
		/// <param name="separator">
		/// The string to use as a separator. <paramref name="separator"/> is included in the returned string only if
		/// <paramref name="values"/> has more than one element.
		/// </param>
		/// <param name="values">A collection that contains the strings to concatenate.</param>
		/// <returns>
		/// A string that consists of the members of <paramref name="values"/> delimited by the <paramref name="separator"/>
		/// string. If values has no members, the method returns <see cref="String.Empty"/>.
		/// </returns>
		[Pure]
		public static string Join(string separator, IEnumerable<string> values)
		{
			if (values == null)
				throw new ArgumentNullException(nameof(values));

			using (var en = values.GetEnumerator())
			{
				if (!en.MoveNext())
					return "";

				var firstValue = en.Current;

				if (!en.MoveNext())
				{
					// Only one value available
					return firstValue ?? "";
				}

				// Null separator and values are handled by the StringBuilder
				var result = new StringBuilder();
				result.Append(firstValue);

				do
				{
					result.Append(separator);
					result.Append(en.Current);
				} while (en.MoveNext());

				return result.ToString();
			}
		}

		/// <summary>
		/// Concatenates the members of a constructed <see cref="IEnumerable{T}"/> collection of type <see cref="string"/>,
		/// using the specified separator between each member.
		/// </summary>
		/// <param name="separator">
		/// The string to use as a separator. <paramref name="separator"/> is included in the returned string only if
		/// <paramref name="values"/> has more than one element.
		/// </param>
		/// <param name="values">A collection that contains the strings to concatenate.</param>
		/// <returns>
		/// A string that consists of the members of <paramref name="values"/> delimited by the <paramref name="separator"/>
		/// string. If values has no members, the method returns <see cref="String.Empty"/>.
		/// </returns>
		[Pure]
		public static string Join<T>(string separator, IEnumerable<T> values)
		{
			if (values == null)
				throw new ArgumentNullException(nameof(values));

			using (var en = values.GetEnumerator())
			{
				if (!en.MoveNext())
					return "";

				var result = new StringBuilder();
				var currentValue = en.Current;

				if (currentValue != null)
					result.Append(currentValue);

				while (en.MoveNext())
				{
					currentValue = en.Current;

					result.Append(separator);
					if (currentValue != null)
						result.Append(currentValue);
				}
				return result.ToString();
			}
		}
	}
}
#endif