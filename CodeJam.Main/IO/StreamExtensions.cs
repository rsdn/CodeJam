
#if NET45_OR_GREATER || TARGETS_NETSTANDARD || TARGETS_NETCOREAPP // PUBLIC_API_CHANGES
using System;
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
		public static StreamReader ToStreamReader(
			this Stream stream,
			Encoding? encoding = null,
			bool leaveOpen = false) =>
				new(
					stream,
					encoding ?? Encoding.UTF8,
					true,
					_defaultBufferSize,
					leaveOpen);

		/// <summary>
		/// Wraps <paramref name="stream"/> with <see cref="BinaryReader"/>.
		/// </summary>
		/// <param name="stream">The stream to read.</param>
		/// <param name="encoding">The character encoding to use.</param>
		/// <param name="leaveOpen">true to leave the stream open after the <see cref="BinaryReader" /> object is disposed; otherwise, false.</param>
		public static BinaryReader ToBinaryReader(
			this Stream stream,
			Encoding? encoding = null,
			bool leaveOpen = false) =>
				new(stream, encoding ?? Encoding.UTF8, leaveOpen);

		/// <summary>
		/// Wraps <paramref name="stream"/> with <see cref="StreamWriter"/>.
		/// </summary>
		/// <param name="stream">The stream to write.</param>
		/// <param name="encoding">The character encoding to use.</param>
		/// <param name="leaveOpen">true to leave the stream open after the <see cref="StreamWriter" /> object is disposed; otherwise, false.</param>
		public static StreamWriter ToStreamWriter(
			this Stream stream,
			Encoding? encoding = null,
			bool leaveOpen = false) =>
				new(stream, encoding ?? Encoding.UTF8, _defaultBufferSize, leaveOpen);

		/// <summary>
		/// Wraps <paramref name="stream"/> with <see cref="BinaryWriter"/>.
		/// </summary>
		/// <param name="stream">The stream to write.</param>
		/// <param name="encoding">The character encoding to use.</param>
		/// <param name="leaveOpen">true to leave the stream open after the <see cref="BinaryWriter" /> object is disposed; otherwise, false.</param>
		public static BinaryWriter ToBinaryWriter(
			this Stream stream,
			Encoding? encoding = null,
			bool leaveOpen = false) =>
				new(stream, encoding ?? Encoding.UTF8, leaveOpen);

		/// <summary>
		/// Returns content of the stream as a byte array.
		/// </summary>
		/// <param name="stream">The stream to read.</param>
		/// <param name="encoding">The character encoding to use.</param>
		public static string ReadAsString(this Stream stream, Encoding? encoding = null)
		{
			// DO NOT dispose the reader
			using (var reader = stream.ToStreamReader(encoding, true))
				return reader.ReadToEnd();
		}

		/// <summary>
		/// Returns content of the stream as a string.
		/// </summary>
		/// <param name="stream">The stream to read.</param>
		/// <param name="encoding">The character encoding to use.</param>
		public static async Task<string> ReadAsStringAsync(
			this Stream stream,
			Encoding? encoding = null)
		{
			// DO NOT dispose the reader
			using (var reader = stream.ToStreamReader(encoding, true))
				return await reader.ReadToEndAsync().ConfigureAwait(false);
		}

		/// <summary>
		/// Returns content of the stream as a byte array.
		/// </summary>
		/// <param name="stream">The stream to read.</param>
		public static byte[] ReadAsByteArray(this Stream stream)
		{
			if (stream.CanSeek)
				// DO NOT dispose underlying stream
				using (var reader = stream.ToBinaryReader(null, true))
				{
					var readCount = checked((int)(stream.Length - stream.Position));
					return reader.ReadBytes(readCount);
				}
			using (var tempStream = new MemoryStream())
			{
				stream.CopyTo(tempStream);
				return tempStream.ToArray();
			}
		}
	}
}

#endif
