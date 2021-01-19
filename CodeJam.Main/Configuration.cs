﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;

// ReSharper disable once RedundantUsingDirective
using CodeJam.Threading;

using JetBrains.Annotations;

// ReSharper disable once CheckNamespace

namespace CodeJam.Internal
{
	/// <summary>
	/// Helper class for overriding default library behaviour.
	/// </summary>
	[PublicAPI]
	public static class Configuration
	{
		#region Exceptions
		/// <summary>
		/// If true, breaks execution if debugger is attached and assertion is failed.
		/// Enabled by default.
		/// </summary>
		/// <value><c>true</c> if the execution will break on exception creation; otherwise, <c>false</c>.</value>
		public static bool BreakOnFailedAssertions { get; set; } = true;
		#endregion

		#region Logging
		private static readonly Lazy<TraceSource> _codeTraceSource = new(
			() => CreateTraceSource(typeof(Code).Namespace + "." + nameof(CodeTraceSource)));

		private static TraceSource CreateTraceSource(string sourceName) =>
			new(sourceName) { Switch = { Level = SourceLevels.Information } };

		private static TraceSource? _customCodeTraceSource;

		/// <summary>
		/// Sets custom trace source for code exceptions.
		/// Pass <c>null</c> to restore default behaviour.
		/// </summary>
		/// <param name="codeTraceSource">The custom trace source.</param>
		public static void SetCodeTraceSource(TraceSource? codeTraceSource) =>
			_customCodeTraceSource = codeTraceSource;

		/// <summary>Returns trace source for code exceptions.</summary>
		/// <value>The code trace source.</value>
		public static TraceSource CodeTraceSource => _customCodeTraceSource ?? _codeTraceSource.Value;

		private static readonly string _assertionFailedMessageWithStack =
			"Assertion failed: {0}" + Environment.NewLine + "{1}";

		private const string _exceptionCaughtMessage = "Exception caught safely: {0}";

		private const string _exceptionSwallowedMessage = "Exception swallowed: {0}";

		/// <summary>Logs the exception that will be thrown to the <see cref="CodeTraceSource"/>.</summary>
		/// <typeparam name="TException">The type of the exception.</typeparam>
		/// <param name="exception">The exception.</param>
		/// <returns>The original exception.</returns>
		[MustUseReturnValue]
		public static TException LogToCodeTraceSourceBeforeThrow<TException>(this TException exception)
			where TException : Exception
		{
			CodeTraceSource.TraceEvent(
				TraceEventType.Verbose,
				0,
				_assertionFailedMessageWithStack,
				exception,
				Environment.StackTrace);

			return exception;
		}

		/// <summary>
		/// Logs the caught exception to the <see cref="CodeTraceSource" />.
		/// </summary>
		/// <typeparam name="TException">The type of the exception.</typeparam>
		/// <param name="exception">The exception.</param>
		/// <param name="safe">If set to <c>true</c> the exception is expected and can be ignored safely.</param>
		/// <returns>
		/// The original exception.
		/// </returns>
		public static TException LogToCodeTraceSourceOnCatch<TException>(this TException exception, bool safe)
			where TException : Exception
		{
			if (safe)
				CodeTraceSource.TraceEvent(
					TraceEventType.Verbose,
					0,
					_exceptionCaughtMessage,
					exception);

			else
				CodeTraceSource.TraceEvent(
					TraceEventType.Warning,
					0,
					_exceptionSwallowedMessage,
					exception);

			return exception;
		}
		#endregion

		#region TempDataRetry
		private static void TempDataRetry(Action callback)
		{
			if (callback == null)
				throw new ArgumentNullException(nameof(callback));

			List<Exception>? exceptions = null;
			var throttleDelay = TimeSpan.FromMilliseconds(100);
			var maxThrottleDelay = TimeSpan.FromSeconds(10);

			for (var i = 0; i < 5; i++)
			{
				try
				{
					callback();
					break;
				}
				catch (IOException ex)
				{
					ex.LogToCodeTraceSourceOnCatch(true);
					exceptions ??= new List<Exception>();
					exceptions.Add(ex);

					var delay = TimeoutHelper.ExponentialBackoffTimeout(i + 1, throttleDelay, maxThrottleDelay);

#if TARGETS_NET || NETSTANDARD20_OR_GREATER || TARGETS_NETCOREAPP
					Thread.Sleep(delay);
#else
					Task.Delay(delay).Wait();
#endif
				}
			}

			if (exceptions != null)
			{
				var exception = new AggregateException(exceptions);
				throw exception.LogToCodeTraceSourceBeforeThrow();
			}
		}

		private static readonly Action<Action> _tempDataRetryCallback = TempDataRetry;

		private static Action<Action>? _customTempDataRetryCallback;

		/// <summary>
		/// Sets the custom retry callback for <see cref="CodeJam.IO.TempData"/> disposal.
		/// Pass <c>null</c> to restore default behaviour.
		/// </summary>
		/// <param name="tempDataRetryCallback">The retry callback.</param>
		public static void SetTempDataRetryCallback(Action<Action>? tempDataRetryCallback) =>
			_customTempDataRetryCallback = tempDataRetryCallback;

		/// <summary>Returns retry callback for <see cref="CodeJam.IO.TempData"/> disposal.</summary>
		/// <value>The retry callback for <see cref="CodeJam.IO.TempData"/> disposal.</value>
		public static Action<Action> TempDataRetryCallback => _customTempDataRetryCallback ?? _tempDataRetryCallback;
		#endregion
	}
}