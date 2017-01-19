#if !FW35
using System;

using JetBrains.Annotations;

namespace CodeJam.Mapping
{
	/// <summary>
	/// A helper class to convert a value of <i>TTo</i> type.
	/// </summary>
	/// <typeparam name="TTo"></typeparam>
	[PublicAPI]
	public static class ConvertTo<TTo>
	{
		/// <summary>
		/// Converts an object from the <i>TFrom</i> type to the <i>TTo</i>.
		/// </summary>
		/// <typeparam name="TFrom"></typeparam>
		/// <param name="o">An object to convert.</param>
		/// <returns>Converted object.</returns>
		/// <example>
		/// ConvertTo&lt;int&gt;.From("123");
		/// </example>
		[Pure]
		public static TTo From<TFrom>(TFrom o) => Convert<TFrom,TTo>.From(o);
	}
}
#endif