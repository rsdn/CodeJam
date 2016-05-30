using System;

using JetBrains.Annotations;

namespace CodeJam
{
	/// <summary>
	/// Helper method for nullable types.
	/// </summary>
	[PublicAPI]
	public static class NullableHelper
	{
		/// <summary>
		/// Retrieves the value of the current <see cref="Nullable{T}"/> object, or value returned by factory.
		/// </summary>
		/// <typeparam name="T">The underlying value type of the <see cref="Nullable{T}"/> generic type.</typeparam>
		/// <param name="value">Nullable value.</param>
		/// <param name="defaultFactory">
		/// A function to return default value if the <see cref="Nullable{T}.HasValue"/> property is <c>false</c>.
		/// </param>
		/// <returns>
		/// The value of the <see cref="Nullable{T}.Value"/> property if the <see cref="Nullable{T}.HasValue"/> property is
		/// <c>true</c>; otherwise, the value returned by <paramref name="defaultFactory"/> parameter.
		/// </returns>
		[Pure]
		public static T GetValueOrDefault<T>(T? value, [NotNull, InstantHandle] Func<T> defaultFactory) where T : struct
		{
			Code.NotNull(defaultFactory, nameof (defaultFactory));
			return value ?? defaultFactory();
		}

		/// <summary>
		/// Returns nullable of specified value.
		/// </summary>
		/// <typeparam name="T">Type of value</typeparam>
		/// <param name="value">THe value</param>
		/// <returns><paramref name="value"/> wrapped in nullabe.</returns>
		[Pure]
		public static T? AsNullable<T>(this T value) where T : struct => value;
	}
}