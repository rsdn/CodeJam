using System.Diagnostics.CodeAnalysis;
using NUnit.Framework;

namespace CodeJam
{
	/// <summary>NUnit NotNull assertion with nullable annotations</summary>
	public static class NAssert
	{

#pragma warning disable CS8777 // Parameter must have a non-null value when exiting.

		#region NotNull

		/// <summary>
		/// Verifies that the object that is passed in is not equal to <see langword="null" />. Returns without throwing an
		/// exception when inside a multiple assert block.
		/// </summary>
		/// <param name="anObject">The object that is to be tested</param>
		/// <param name="message">The message to display in case of failure</param>
		/// <param name="args">Array of objects to be used in formatting the message</param>
		/// <remarks><see cref="Assert.NotNull(object,string,object[])"/></remarks>
		public static void NotNull([NotNull] object? anObject, string message, params object?[]? args)
		{
			Assert.IsNotNull(anObject, message, args);
		}

		/// <summary>
		/// Verifies that the object that is passed in is not equal to <see langword="null" />. Returns without throwing an
		/// exception when inside a multiple assert block.
		/// </summary>
		/// <param name="anObject">The object that is to be tested</param>
		/// <param name="message">The message to display in case of failure</param>
		/// <param name="args">Array of objects to be used in formatting the message</param>
		/// <remarks><see cref="Assert.IsNotNull(object,string,object[])"/></remarks>
		public static void IsNotNull([NotNull] object? anObject, string message, params object?[]? args)
		{
			Assert.IsNotNull(anObject, message, args);
		}

		/// <summary>
		/// Verifies that the object that is passed in is not equal to <see langword="null" />. Returns without throwing an
		/// exception when inside a multiple assert block.
		/// </summary>
		/// <param name="anObject">The object that is to be tested</param>
		/// <remarks><see cref="Assert.NotNull(object)"/></remarks>
		public static void NotNull([NotNull] object? anObject)
		{
			Assert.IsNotNull(anObject);
		}

		/// <summary>
		/// Verifies that the object that is passed in is not equal to <see langword="null" />. Returns without throwing an
		/// exception when inside a multiple assert block.
		/// </summary>
		/// <param name="anObject">The object that is to be tested</param>
		/// <remarks><see cref="Assert.IsNotNull(object)"/></remarks>
		public static void IsNotNull([NotNull] object? anObject)
		{
			Assert.IsNotNull(anObject);
		}

		#endregion

#pragma warning restore CS8777 // Parameter must have a non-null value when exiting.

	}
}
