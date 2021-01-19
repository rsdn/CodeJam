using System;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

using CodeJam.Strings;
using CodeJam.Targeting;

using JetBrains.Annotations;

namespace CodeJam
{
	/// <summary>
	/// The <see cref="Exception"/> class extensions.
	/// </summary>
	[PublicAPI]
	public static class ExceptionExtensions
	{
		/// <summary>
		/// Returns detailed exception text.
		/// </summary>
		/// <param name="exception">Exception to process.</param>
		/// <param name="stringBuilder"><see cref="StringBuilder"/> instance.</param>
		/// <returns>Detailed exception text.</returns>
		public static StringBuilder ToDiagnosticString(this Exception exception, StringBuilder stringBuilder)
		{
			using var writer = new StringWriter(stringBuilder);
			ToDiagnosticString(exception, writer, stringBuilder.Length == 0);
			writer.Flush();
			return stringBuilder;
		}

		/// <summary>
		/// Returns detailed exception text.
		/// </summary>
		/// <param name="exception">Exception to process.</param>
		/// <param name="writer"><see cref="TextWriter"/> instance.</param>
		/// <param name="fromNewLine">If <c>true</c> - do not inject separator line from start.</param>
		/// <returns>Detailed exception text.</returns>
		public static void ToDiagnosticString(
			this Exception? exception,
			TextWriter writer,
			bool fromNewLine = true)
		{
			Code.NotNull(writer, nameof(writer));

			// ReSharper disable once PossibleNullReferenceException
			for (var ex = exception; ex != null; ex = ex?.InnerException)
			{
				var exceptionText = $"Exception: {ex.GetType()}";

				if (!fromNewLine)
				{
					for (var i = 0; i < exceptionText.Length; i++)
						writer.Write('-');
					writer.WriteLine();
				}
				else
					fromNewLine = false;

				writer.WriteLine(exceptionText);

				if (ex.Message.NotNullNorEmpty())
					writer.WriteLine(ex.Message);

				if (ex.StackTrace.NotNullNorEmpty())
					writer.WriteLine(ex.StackTrace);

				switch (ex)
				{
					case FileNotFoundException notFoundException:
						var fex = notFoundException;

						writer.WriteLine($"File Name: {fex.FileName}");

						if (fex.GetFusionLog().IsNullOrEmpty())
							writer.WriteLine("Fusion log is empty or disabled.");
						else
							writer.Write(fex.GetFusionLog());
						break;

					case AggregateException aex:
						var foundInnerException = false;

						foreach (var e in aex.InnerExceptions)
						{
							foundInnerException = foundInnerException || e != ex.InnerException;
							ToDiagnosticString(e, writer, false);
						}

						if (foundInnerException)
							ex = ex.InnerException;
						break;

					case ReflectionTypeLoadException{LoaderExceptions: { } inners }:
						foreach (var e in inners)
							if (e != null)
								ToDiagnosticString(e, writer, false);
						break;
				}
			}
		}

#if NET45_OR_GREATER || TARGETS_NETSTANDARD || TARGETS_NETCOREAPP // PUBLIC_API_CHANGES. TODO: update after fixes in Theraot.Core
		/// <summary>
		/// Returns detailed exception text.
		/// </summary>
		/// <param name="exception">Exception to process.</param>
		/// <param name="writer"><see cref="TextWriter"/> instance.</param>
		/// <param name="fromNewLine">If <c>true</c> - do not inject separator line from start.</param>
		/// <returns>Detailed exception text.</returns>
		public static Task ToDiagnosticStringAsync(
			this Exception? exception,
			TextWriter writer,
			bool fromNewLine = true)
		{
			Code.NotNull(writer, nameof(writer));

			return ToDiagnosticStringImplAsync(exception, writer, fromNewLine);
		}

		private static async Task ToDiagnosticStringImplAsync(
			this Exception? exception,
			TextWriter writer,
			bool fromNewLine = true)
		{
			// ReSharper disable once PossibleNullReferenceException
			for (var ex = exception; ex != null; ex = ex?.InnerException)
			{
				var exceptionText = $"Exception: {ex.GetType()}";

				if (!fromNewLine)
				{
					for (var i = 0; i < exceptionText.Length; i++)
						await writer.WriteAsync('-').ConfigureAwait(false);
					await writer.WriteLineAsync().ConfigureAwait(false);
				}
				else
					fromNewLine = false;

				await writer.WriteLineAsync(exceptionText).ConfigureAwait(false);

				if (ex.Message.NotNullNorEmpty())
					await writer.WriteLineAsync(ex.Message).ConfigureAwait(false);

				if (ex.StackTrace.NotNullNorEmpty())
					await writer.WriteLineAsync(ex.StackTrace).ConfigureAwait(false);

				switch (ex)
				{
					case FileNotFoundException notFoundException:
						var fex = notFoundException;

						await writer.WriteLineAsync($"File Name: {fex.FileName}").ConfigureAwait(false);

						if (fex.GetFusionLog().IsNullOrEmpty())
							await writer.WriteLineAsync("Fusion log is empty or disabled.").ConfigureAwait(false);
						else
							await writer.WriteAsync(fex.GetFusionLog()).ConfigureAwait(false);
						break;

					case AggregateException aex:
						var foundInnerException = false;

						foreach (var e in aex.InnerExceptions)
						{
							foundInnerException = foundInnerException || e != ex.InnerException;
							await ToDiagnosticStringAsync(e, writer, false).ConfigureAwait(false);
						}

						if (foundInnerException)
							ex = ex.InnerException;
						break;

					case ReflectionTypeLoadException {LoaderExceptions:{} inners}:
						foreach (var e in inners)
							if (e != null)
								await ToDiagnosticStringAsync(e, writer, false).ConfigureAwait(false);
						break;
				}
			}
		}
#endif

		/// <summary>
		/// Returns detailed exception text.
		/// </summary>
		/// <param name="exception">Exception to process.</param>
		/// <returns>Detailed exception text.</returns>
		[Pure, System.Diagnostics.Contracts.Pure]
		public static string ToDiagnosticString(this Exception? exception) =>
			exception == null ? "" : exception.ToDiagnosticString(new StringBuilder()).ToString();
	}
}