using System;
using System.IO;
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
		public static StringBuilder ToDiagnosticString(this Exception exception, StringBuilder stringBuilder)
		{
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

				var notFoundException = ex as FileNotFoundException;

				if (notFoundException != null)
				{
					var fex = notFoundException;

					stringBuilder.AppendLine($"File Name: {fex.FileName}");

					if (fex.FusionLog.IsNullOrEmpty())
					{
						stringBuilder.AppendLine("Fusion log is empty or disabled.");
					}
					else
					{
						stringBuilder.Append(fex.FusionLog);
					}
				}
				else
				{
					var aex = ex as AggregateException;

					if (aex?.InnerExceptions != null)
					{
						var foundInnerException = false;

						foreach (var e in aex.InnerExceptions)
						{
							foundInnerException = foundInnerException || e != ex.InnerException;
							ToDiagnosticString(e, stringBuilder);
						}

						if (foundInnerException)
							ex = ex.InnerException;
					}
				}
			}

			return stringBuilder;
		}

		/// <summary>
		/// Returns detailed exception text.
		/// </summary>
		/// <param name="exception">Exception to process.</param>
		/// <returns>Detailed exception text.</returns>
		public static string ToDiagnosticString(this Exception exception)
			=> exception.ToDiagnosticString(new StringBuilder()).ToString();
	}
}
