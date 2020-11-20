using System;

using JetBrains.Annotations;

namespace CodeJam
{
	/// <summary>
	/// Helper methods for <see cref="Uri"/> class.
	/// </summary>
	[PublicAPI]
	public static class UriHelper
	{
		/// <summary>
		/// Combine two uris.
		/// </summary>
		/// <param name="baseUri">Base uri</param>
		/// <param name="relativeUri">Relative uri</param>
		/// <returns>Combined uri.</returns>
		[NotNull, Pure]
		public static Uri Combine([NotNull] this Uri baseUri, [NotNull] Uri relativeUri) => new(baseUri, relativeUri);

		/// <summary>
		/// Combine two uris.
		/// </summary>
		/// <param name="baseUri">Base uri</param>
		/// <param name="relativeUri">Relative uri</param>
		/// <returns>Combined uri.</returns>
		[NotNull, Pure]
		public static Uri Combine([NotNull] this Uri baseUri, [NotNull] string relativeUri) => new(baseUri, relativeUri);

		/// <summary>
		/// Combine two uris.
		/// </summary>
		/// <param name="baseUri">Base uri</param>
		/// <param name="relativeUri">Relative uri</param>
		/// <returns>Combined uri.</returns>
		[NotNull, Pure]
		public static Uri Combine([NotNull] string baseUri, [NotNull] string relativeUri)
		{
			var baseParsed = new Uri(baseUri);
			Code.AssertArgument(!baseParsed.IsAbsoluteUri, nameof(baseUri), "Base uri must be absolute");
			return new Uri(baseParsed, relativeUri);
		}
	}
}