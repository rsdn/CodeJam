using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;

namespace CodeJam.PerfTests.Running.Core
{
	/// <summary>
	/// IO helpers
	/// </summary>
	public static class IoHelpers
	{
		/// <summary>Reads file content and fails if not able to detect encoding.</summary>
		/// <param name="path">The path.</param>
		/// <returns>File lines.</returns>
		public static string[] ReadFileContent(string path)
		{
			var lines = new List<string>();
			using (var streamReader = new StreamReader(path))
			{
				string line;
				while ((line = streamReader.ReadLine()) != null)
				{
					var fallback = streamReader.CurrentEncoding.DecoderFallback as DecoderReplacementFallback;
					if (fallback != null)
					{
						var idx = line.IndexOf(fallback.DefaultString, StringComparison.Ordinal);
						if (idx >= 0)
							throw new DecoderFallbackException(
								$"Invalid character at line {lines.Count + 1}, position {idx + 1}.");
					}
					lines.Add(line);
				}
			}
			return lines.ToArray();
		}

		/// <summary>Writes file content without empty line at the end.</summary>
		/// <param name="path">The path.</param>
		/// <param name="lines">The lines to write.</param>
		// THANKSTO: http://stackoverflow.com/a/11689630
		public static void WriteFileContent(string path, string[] lines)
		{
			if (path == null)
				throw new ArgumentNullException(nameof(path));
			if (lines == null)
				throw new ArgumentNullException(nameof(lines));

			using (var writer = File.CreateText(path))
			{
				if (lines.Length > 0)
				{
					for (var i = 0; i < lines.Length - 1; i++)
					{
						writer.WriteLine(lines[i]);
					}
					writer.Write(lines[lines.Length - 1]);
				}
			}
		}

		/// <summary>Tries to obtain text from the given URI.</summary>
		/// <param name="uri">The URI to geth the text from.</param>
		/// <returns>The text reader or <c>null</c> if none.</returns>
		public static TextReader TryGetTextFromUri(string uri) =>
			TryGetTextFromUri(uri, null);

		/// <summary>Tries to obtain text from the given URI.</summary>
		/// <param name="uri">The URI to geth the text from.</param>
		/// <param name="timeOut">The timeout.</param>
		/// <returns>The text reader or <c>null</c> if none.</returns>
		public static TextReader TryGetTextFromUri(string uri, TimeSpan? timeOut)
		{
			if (uri == null)
				throw new ArgumentNullException(nameof(uri));

			var uriInst = new Uri(uri, UriKind.RelativeOrAbsolute);
			if (uriInst.IsAbsoluteUri && !uriInst.IsFile)
			{
				try
				{
					var webRequest = WebRequest.Create(uriInst);
					if (timeOut != null)
					{
						webRequest.Timeout = (int)timeOut.Value.TotalMilliseconds;
					}
					using (var response = webRequest.GetResponse())
					using (var content = response.GetResponseStream())
					{
						if (content == null)
							return null;

						// TODO: check encoding using DecoderReplacementFallback
						using (var reader = new StreamReader(content))
						{
							return new StringReader(reader.ReadToEnd());
						}
					}
				}
				catch (WebException)
				{
					return null;
				}
			}

			var path = uriInst.IsAbsoluteUri ? uriInst.LocalPath : uri;

			return File.Exists(path) ? File.OpenText(path) : null;
		}
	}
}