using System;
using System.IO;
using System.Reflection;
using System.Text;

using CodeJam.Strings;

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
		[NotNull]
		public static StringBuilder ToDiagnosticString(this Exception exception, [NotNull] StringBuilder stringBuilder)
		{
			Code.NotNull(stringBuilder, nameof(stringBuilder));

			// ReSharper disable once PossibleNullReferenceException
			for (var ex = exception; ex != null; ex = ex.InnerException)
			{
				var exceptionText = $"Exception: {ex.GetType()}";

				if (stringBuilder.Length != 0)
					stringBuilder
						.Append('-', exceptionText.Length)
						.AppendLine();

				stringBuilder.AppendLine(exceptionText);

				if (ex.Message.NotNullNorEmpty())
					stringBuilder.AppendLine(ex.Message);

				if (ex.StackTrace.NotNullNorEmpty())
					stringBuilder.AppendLine(ex.StackTrace);

				switch (ex) {
					case FileNotFoundException notFoundException:
						var fex = notFoundException;

						stringBuilder.AppendLine($"File Name: {fex.FileName}");

						if (fex.FusionLog.IsNullOrEmpty())
							stringBuilder.AppendLine("Fusion log is empty or disabled.");
						else
							stringBuilder.Append(fex.FusionLog);
						break;
					case AggregateException aex when aex.InnerExceptions != null:
						var foundInnerException = false;

						foreach (var e in aex.InnerExceptions)
						{
							foundInnerException = foundInnerException || e != ex.InnerException;
							ToDiagnosticString(e, stringBuilder);
						}

						if (foundInnerException)
							ex = ex.InnerException;
						break;

					case ReflectionTypeLoadException rtle:
						foreach (var e in rtle.LoaderExceptions)
						{
							ToDiagnosticString(e, stringBuilder);
						}
						break;
				}
			}

			return stringBuilder;
		}

		/// <summary>
		/// Returns detailed exception text.
		/// </summary>
		/// <param name="exception">Exception to process.</param>
		/// <returns>Detailed exception text.</returns>
		[Pure]
		[NotNull]
		public static string ToDiagnosticString(this Exception exception)
			=> exception.ToDiagnosticString(new StringBuilder()).ToString();
	}
}
