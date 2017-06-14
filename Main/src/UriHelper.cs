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
		public static Uri Combine(this Uri baseUri, Uri relativeUri)
		{
			return new Uri(baseUri, relativeUri);
		}

		/// <summary>
		/// Combine two uris.
		/// </summary>
		/// <param name="baseUri">Base uri</param>
		/// <param name="relativeUri">Relative uri</param>
		/// <returns>Combined uri.</returns>
		public static Uri Combine(this Uri baseUri, string relativeUri)
		{
			return new Uri(baseUri, relativeUri);
		}

		/// <summary>
		/// Combine two uris.
		/// </summary>
		/// <param name="baseUri">Base uri</param>
		/// <param name="relativeUri">Relative uri</param>
		/// <returns>Combined uri.</returns>
		public static Uri Combine(string baseUri, string relativeUri)
		{
			var baseParsed = new Uri(baseUri);
			Code.AssertArgument(!baseParsed.IsAbsoluteUri, nameof(baseUri), "Base uri must be absolute");
			return new Uri(baseParsed, relativeUri);
		}
	}
}