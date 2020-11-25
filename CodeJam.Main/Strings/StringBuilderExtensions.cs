using System;
using System.Text;

using JetBrains.Annotations;

namespace CodeJam.Strings
{
	/// <summary>
	/// <see cref="StringBuilder"/> class extensions.
	/// </summary>
	[PublicAPI]
	public static class StringBuilderExtensions
	{
		/// <summary>
		/// Determines whether the StringBuilder is null or empty.
		/// </summary>
		/// <param name="value">The StringBuilder to test.</param>
		/// <returns>
		/// <c>true</c> if <paramref name="value"/> is null or empty; otherwise, <c>false</c>.
		/// </returns>
		[Pure]
 		[ContractAnnotation("value:null => true")]
		public static bool IsNullOrEmpty(this StringBuilder? value) =>
			value == null || value.Length == 0;

		/// <summary>
		/// Determines whether the StringBuilder is neither null nor empty.
		/// </summary>
		/// <param name="value">The StringBuilder to test.</param>
		/// <returns>
		/// <c>true</c> if <paramref name="value"/> is not null and not empty; otherwise, <c>false</c>.
		/// </returns>
		[Pure]
		[ContractAnnotation("value:null => false")]
		public static bool NotNullNorEmpty(this StringBuilder? value) =>
			!IsNullOrEmpty(value);
	}
}