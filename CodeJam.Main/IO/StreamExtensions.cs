﻿#if NET45_OR_GREATER || TARGETS_NETSTANDARD || TARGETS_NETCOREAPP // PUBLIC_API_CHANGES
using System.IO;
using System.Text;
using System.Threading.Tasks;

using JetBrains.Annotations;

namespace CodeJam.IO
{
	/// <summary>
	/// Stream extensions.
	/// </summary>
	[PublicAPI]
	public static class StreamExtensions
	{
		private const int _defaultBufferSize = 1024;

		/// <summary>
		/// Wraps <paramref name="stream" /> with <see cref="StreamReader" />.
		/// </summary>
		/// <param name="stream">The stream to read.</param>
		/// <param name="encoding">The character encoding to use.</param>
		/// <param name="leaveOpen">true to leave the stream open after the <see cref="StreamReader" /> object is disposed; otherwise, false.</param>
		[NotNull]
		public static StreamReader ToStreamReader(
			[NotNull] this Stream stream,
			[CanBeNull] Encoding encoding = null,
			bool leaveOpen = false) =>
			new StreamReader(stream, encoding ?? Encoding.UTF8, true, _defaultBufferSize, leaveOpen);

		/// <summary>
		/// Wraps <paramref name="stream"/> with <see cref="BinaryReader"/>.
		/// </summary>
		/// <param name="stream">The stream to read.</param>
		/// <param name="encoding">The character encoding to use.</param>
		/// <param name="leaveOpen">true to leave the stream open after the <see cref="BinaryReader" /> object is disposed; otherwise, false.</param>
		[NotNull]
		public static BinaryReader ToBinaryReader(
			[NotNull] this Stream stream,
			[CanBeNull] Encoding encoding = null,
			bool leaveOpen = false) =>
			new BinaryReader(stream, encoding ?? Encoding.UTF8, leaveOpen);

		/// <summary>
		/// Wraps <paramrefref name="stream"/> with <see cref="StreamWriter"/>.
		/// </summary>
		/// <param name="stream">The stream to write.</param>
		/// <param name="encoding">The character encoding to use.</param>
		/// <param name="leaveOpen">true to leave the stream open after the <see cref="StreamWriter" /> object is disposed; otherwise, false.</param>
		[NotNull]
		public static StreamWriter ToStreamWriter(
			[NotNull] this Stream stream,
			[CanBeNull] Encoding encoding = null,
			bool leaveOpen = false) =>
			new StreamWriter(stream, encoding ?? Encoding.UTF8, _defaultBufferSize, leaveOpen);

		/// <summary>
		/// Wraps <paramref name="stream"/> with <see cref="BinaryWriter"/>.
		/// </summary>
		/// <param name="stream">The stream to write.</param>
		/// <param name="encoding">The character encoding to use.</param>
		/// <param name="leaveOpen">true to leave the stream open after the <see cref="BinaryWriter" /> object is disposed; otherwise, false.</param>
		[NotNull]
		public static BinaryWriter ToBinaryWriter(
			[NotNull] this Stream stream,
			[CanBeNull] Encoding encoding = null,
			bool leaveOpen = false) =>
				new BinaryWriter(stream, encoding ?? Encoding.UTF8, leaveOpen);

		/// <summary>
		/// Returns content of the stream as a byte array.
		/// </summary>
		/// <param name="stream">The stream to read.</param>
		/// <param name="encoding">The character encoding to use.</param>
		[NotNull]
		public static string ReadAsString([NotNull] this Stream stream, [CanBeNull] Encoding encoding = null)
		{
			// DO NOT dispose the reader
			using (var reader = stream.ToStreamReader(encoding, true))
			{
				return reader.ReadToEnd();
			}
		}

		/// <summary>
		/// Returns content of the stream as a string.
		/// </summary>
		/// <param name="stream">The stream to read.</param>
		/// <param name="encoding">The character encoding to use.</param>
		[NotNull]
		public static async Task<string> ReadAsStringAsync(
			[NotNull] this Stream stream,
			[CanBeNull] Encoding encoding = null)
		{
			// DO NOT dispose the reader
			using (var reader = stream.ToStreamReader(encoding, true))
			{
				return await reader.ReadToEndAsync().ConfigureAwait(false);
			}
		}
		/// <summary>
		/// Returns content of the stream as a byte array.
		/// </summary>
		/// <param name="stream">The stream to read.</param>
		[NotNull]
		public static byte[] ReadAsByteArray([NotNull] this Stream stream)
		{
			// DO NOT dispose the reader
			using (var reader = stream.ToBinaryReader(null, true))
			{
				var readCount = checked((int)(stream.Length - stream.Position));
				return reader.ReadBytes(readCount);
			}
		}
	}
}
#endif
