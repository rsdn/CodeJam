using System;
using System.Linq;
using System.Reflection;

using JetBrains.Annotations;

namespace CodeJam.Reflection
{
	/// <summary>
	/// Reflection helper methods for enumeration.
	/// </summary>
	[PublicAPI]
	public static class ReflectionEnumHelper
	{
		/// <summary>
		/// Searches for the public field with the specified enumeration value.
		/// </summary>
		/// <typeparam name="TEnum">An enumeration type.</typeparam>
		/// <param name="value">An enumeration value.</param>
		/// <returns>
		/// An object representing the public field with the specified enumeration value, if found;
		/// otherwise, null.
		/// </returns>
		public static FieldInfo? GetField<TEnum>(TEnum value) where TEnum : struct, Enum
		{
			var type = typeof(TEnum);
			var name = Enum.GetName(type, value);
			if (name == null)
				return null;

			return type.GetField(name, BindingFlags.Static | BindingFlags.Public);
		}

		/// <summary>Returns enum fields for enum type.</summary>
		/// <typeparam name="TEnum">The type of the enum.</typeparam>
		/// <returns>List of enum fields.</returns>
		public static FieldInfo[] GetFields<TEnum>() where TEnum : struct =>
			GetFields(typeof(TEnum));

		/// <summary>Returns enum fields for enum type.</summary>
		/// <param name="enumType">Type of the enum.</param>
		/// <returns>List of enum fields.</returns>
		public static FieldInfo[] GetFields(Type enumType)
		{
			if (!typeof(Enum).IsAssignableFrom(enumType))
				throw CodeExceptions.Argument(nameof(enumType), $"The {nameof(enumType)} should be derived from {typeof(Enum)}");

			return enumType
				.GetFields(BindingFlags.Public | BindingFlags.Static)
				.Where(f => f.IsLiteral && !f.IsInitOnly)
				.ToArray();
		}
	}
}