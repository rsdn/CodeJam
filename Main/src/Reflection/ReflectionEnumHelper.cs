using System;
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