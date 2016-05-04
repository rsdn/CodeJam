using System;
using System.IO;
using System.Linq;
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
		public static StringBuilder GetText(this Exception exception, StringBuilder stringBuilder)
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

				if (ex is FileNotFoundException)
				{
					var fex = (FileNotFoundException)ex;

					stringBuilder
						.AppendLine($"File Name: {fex.FileName}")
						;

					if (fex.FusionLog.IsNullOrEmpty())
						stringBuilder.AppendLine("Fusion log is empty or disabled.")
						;
					else
						stringBuilder.AppendLine(fex.FusionLog);
				}
				else if (ex is AggregateException)
				{
					var aex = (AggregateException)ex;

					if (aex.InnerExceptions != null)
					{
						var foundInnerException = false;

						foreach (var e in aex.InnerExceptions)
						{
							foundInnerException = foundInnerException || e != ex.InnerException;
							GetText(e, stringBuilder);
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
		public static string GetText(this Exception exception)
			=> exception.GetText(new StringBuilder()).ToString();
	}
}
