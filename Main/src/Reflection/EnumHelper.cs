using System;
using System.Collections.Generic;
using System.Reflection;

using JetBrains.Annotations;

namespace CodeJam.Reflection
{
	/// <summary>
	/// Helper methods for enumeration.
	/// </summary>
	[PublicAPI]
	public static class EnumHelper
	{
		/// <summary>
		/// Retrieves an array of the names of the constants in a specified enumeration.
		/// </summary>
		/// <typeparam name="T">An enumeration type.</typeparam>
		/// <returns>A string array of the names of the constants in enumType.</returns>
		[NotNull]
		[Pure]
		public static string[] GetNames<T>() where T : struct => Enum.GetNames(typeof(T));

		/// <summary>
		/// Retrieves the name of the constant in the specified enumeration that has the specified value.
		/// </summary>
		/// <param name="value">The value of a particular enumerated constant in terms of its underlying type.</param>
		/// <typeparam name="T">An enumeration type.</typeparam>
		/// <returns>
		/// A string containing the name of the enumerated constant in enumType whose value is value;
		/// or null if no such constant is found.
		/// </returns>
		[CanBeNull]
		[Pure]
		public static string GetName<T>(T value) where T : struct => Enum.GetName(typeof(T), value);

		/// <summary>
		/// Retrieves an array of the values of the constants in a specified enumeration.
		/// </summary>
		/// <typeparam name="T">An enumeration type.</typeparam>
		/// <returns>An array that contains the values of the constants in enumType.</returns>
		[NotNull]
		[Pure]
		public static T[] GetValues<T>() where T : struct => (T[])Enum.GetValues(typeof (T));

		/// <summary>
		/// Returns an indication whether a constant with a specified value exists in a specified enumeration.
		/// </summary>
		/// <param name="value">The value or name of a constant in <typeparamref name="T"/>.</param>
		/// <typeparam name="T">An enumeration type.</typeparam>
		/// <returns>true if a constant in enumType has a value equal to value; otherwise, false.</returns>
		[Pure]
		public static bool IsDefined<T>(T value) where T : struct => Enum.IsDefined(typeof (T), value);

		/// <summary>
		/// Converts the string representation of the name or numeric <paramref name="value"/> of one or more
		/// enumerated constants to an equivalent enumerated object. A parameter <paramref name="ignoreCase"/> specifies
		/// whether the operation is case-insensitive.
		/// </summary>
		/// <param name="value">A string containing the name or value to convert.</param>
		/// <param name="ignoreCase">true to ignore case; false to regard case.</param>
		/// <typeparam name="T">An enumeration type.</typeparam>
		/// <returns>An object of type enumType whose value is represented by value.</returns>
		[Pure]
		public static T Parse<T>([NotNull] string value, bool ignoreCase = true) where T : struct =>
			(T)Enum.Parse(typeof (T), value, ignoreCase);

		/// <summary>
		/// Retrieves an array of name/value pairs in a specified enumeration.
		/// </summary>
		/// <typeparam name="T">An enumeration type.</typeparam>
		/// <returns>
		/// An array of the names of the constants in a specified enumeration.
		/// </returns>
		[NotNull]
		[Pure]
		public static KeyValuePair<string, T>[] GetPairs<T>() where T : struct
		{
			var fields = typeof (T).GetFields(BindingFlags.Static | BindingFlags.Public);
			var collection = new KeyValuePair<string, T>[fields.Length];

			for (var i = 0; i < fields.Length; i++)
			{
				var field = fields[i];
				collection[i] = new KeyValuePair<string, T>(field.Name, (T)field.GetValue(null));
			}

			return collection;
		}

		/// <summary>
		/// Searches for the public field with the specified enumeration value.
		/// </summary>
		/// <typeparam name="T">An enumeration type.</typeparam>
		/// <param name="value">An enumeration value.</param>
		/// <returns>
		/// An object representing the public field with the specified enumeration value, if found;
		/// otherwise, null.
		/// </returns>
		[CanBeNull]
		public static FieldInfo GetField<T>(T value) where T : struct
		{
			var type = typeof (T);
			var name = Enum.GetName(type, value);
			if (name == null)
				return null;

			return type.GetField(name, BindingFlags.Static | BindingFlags.Public);
		}
	}
}