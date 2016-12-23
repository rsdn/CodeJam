#if FW40
// BASEDON: https://github.com/dotnet/coreclr/blob/775003a4c72f0acc37eab84628fcef541533ba4e/src/mscorlib/src/System/Reflection/CustomAttributeExtensions.cs

// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.
using System.Collections.Generic;

using JetBrains.Annotations;

namespace System.Reflection
{
	/// <summary>
	/// Contains static methods for retrieving custom attributes.
	/// </summary>
	[PublicAPI]
	public static class CustomAttributeExtensions
	{
#region APIs that return a single attribute
		/// <summary>
		/// Retrieves a custom attribute of a specified type that is applied to a specified assembly.
		/// </summary>
		/// <param name="element">The assembly to inspect.</param>
		/// <param name="attributeType">The type of attribute to search for.</param>
		/// <returns>
		/// A custom attribute that matches <paramref name="attributeType"/>, or null if no such attribute is found.
		/// </returns>
		public static Attribute GetCustomAttribute(this Assembly element, Type attributeType) =>
			Attribute.GetCustomAttribute(element, attributeType);

		/// <summary>
		/// Retrieves a custom attribute of a specified type that is applied to a <paramref name="element"/>.
		/// </summary>
		/// <param name="element">The <paramref name="element"/> to inspect.</param>
		/// <param name="attributeType">The type of attribute to search for.</param>
		/// <returns>
		/// A custom attribute that matches <paramref name="attributeType"/>, or null if no such attribute is found.
		/// </returns>
		public static Attribute GetCustomAttribute(this Module element, Type attributeType) =>
			Attribute.GetCustomAttribute(element, attributeType);

		/// <summary>
		/// Retrieves a custom attribute of a specified type that is applied to a <paramref name="element"/>.
		/// </summary>
		/// <param name="element">The <paramref name="element"/> to inspect.</param>
		/// <param name="attributeType">The type of attribute to search for.</param>
		/// <returns>
		/// A custom attribute that matches <paramref name="attributeType"/>, or null if no such attribute is found.
		/// </returns>
		public static Attribute GetCustomAttribute(this MemberInfo element, Type attributeType) =>
			Attribute.GetCustomAttribute(element, attributeType);

		/// <summary>
		/// Retrieves a custom attribute of a specified type that is applied to a <paramref name="element"/>.
		/// </summary>
		/// <param name="element">The <paramref name="element"/> to inspect.</param>
		/// <param name="attributeType">The type of attribute to search for.</param>
		/// <returns>
		/// A custom attribute that matches <paramref name="attributeType"/>, or null if no such attribute is found.
		/// </returns>
		public static Attribute GetCustomAttribute(this ParameterInfo element, Type attributeType) =>
			Attribute.GetCustomAttribute(element, attributeType);

		/// <summary>
		/// Retrieves a custom attribute of a specified type that is applied to a <paramref name="element"/>.
		/// </summary>
		/// <param name="element">The <paramref name="element"/> to inspect.</param>
		/// <typeparam name="T">The type of attribute to search for.</typeparam>
		/// <returns>
		/// A custom attribute that matches <typeparamref name="T"/>, or null if no such attribute is found.
		/// </returns>
		public static T GetCustomAttribute<T>(this Assembly element) where T : Attribute =>
			(T)GetCustomAttribute(element, typeof(T));

		/// <summary>
		/// Retrieves a custom attribute of a specified type that is applied to a <paramref name="element"/>.
		/// </summary>
		/// <param name="element">The <paramref name="element"/> to inspect.</param>
		/// <typeparam name="T">The type of attribute to search for.</typeparam>
		/// <returns>
		/// A custom attribute that matches <typeparamref name="T"/>, or null if no such attribute is found.
		/// </returns>
		public static T GetCustomAttribute<T>(this Module element) where T : Attribute =>
			(T)GetCustomAttribute(element, typeof(T));

		/// <summary>
		/// Retrieves a custom attribute of a specified type that is applied to a <paramref name="element"/>.
		/// </summary>
		/// <param name="element">The <paramref name="element"/> to inspect.</param>
		/// <typeparam name="T">The type of attribute to search for.</typeparam>
		/// <returns>
		/// A custom attribute that matches <typeparamref name="T"/>, or null if no such attribute is found.
		/// </returns>
		public static T GetCustomAttribute<T>(this MemberInfo element) where T : Attribute =>
			(T)GetCustomAttribute(element, typeof(T));

		/// <summary>
		/// Retrieves a custom attribute of a specified type that is applied to a <paramref name="element"/>.
		/// </summary>
		/// <param name="element">The <paramref name="element"/> to inspect.</param>
		/// <typeparam name="T">The type of attribute to search for.</typeparam>
		/// <returns>
		/// A custom attribute that matches <typeparamref name="T"/>, or null if no such attribute is found.
		/// </returns>
		public static T GetCustomAttribute<T>(this ParameterInfo element) where T : Attribute =>
			(T)GetCustomAttribute(element, typeof(T));

		/// <summary>
		/// Retrieves a custom attribute of a specified type that is applied to a <paramref name="element"/>.
		/// </summary>
		/// <param name="element">The <paramref name="element"/> to inspect.</param>
		/// <param name="attributeType">The type of attribute to search for.</param>
		/// <param name="inherit">true to inspect the ancestors of element; otherwise, false.</param>
		/// <returns>
		/// A custom attribute that matches <paramref name="attributeType"/>, or null if no such attribute is found.
		/// </returns>
		public static Attribute GetCustomAttribute(this MemberInfo element, Type attributeType, bool inherit) =>
			Attribute.GetCustomAttribute(element, attributeType, inherit);

		/// <summary>
		/// Retrieves a custom attribute of a specified type that is applied to a <paramref name="element"/>.
		/// </summary>
		/// <param name="element">The <paramref name="element"/> to inspect.</param>
		/// <param name="attributeType">The type of attribute to search for.</param>
		/// <param name="inherit">true to inspect the ancestors of element; otherwise, false.</param>
		/// <returns>
		/// A custom attribute that matches <paramref name="attributeType"/>, or null if no such attribute is found.
		/// </returns>
		public static Attribute GetCustomAttribute(this ParameterInfo element, Type attributeType, bool inherit) =>
			Attribute.GetCustomAttribute(element, attributeType, inherit);

		/// <summary>
		/// Retrieves a custom attribute of a specified type that is applied to a <paramref name="element"/>.
		/// </summary>
		/// <param name="element">The <paramref name="element"/> to inspect.</param>
		/// <param name="inherit">true to inspect the ancestors of element; otherwise, false.</param>
		/// <typeparam name="T">The type of attribute to search for.</typeparam>
		/// <returns>
		/// A custom attribute that matches <typeparamref name="T"/>, or null if no such attribute is found.
		/// </returns>
		public static T GetCustomAttribute<T>(this MemberInfo element, bool inherit) where T : Attribute =>
			(T)GetCustomAttribute(element, typeof(T), inherit);

		/// <summary>
		/// Retrieves a custom attribute of a specified type that is applied to a <paramref name="element"/>.
		/// </summary>
		/// <param name="element">The <paramref name="element"/> to inspect.</param>
		/// <param name="inherit">true to inspect the ancestors of element; otherwise, false.</param>
		/// <typeparam name="T">The type of attribute to search for.</typeparam>
		/// <returns>
		/// A custom attribute that matches <typeparamref name="T"/>, or null if no such attribute is found.
		/// </returns>
		public static T GetCustomAttribute<T>(this ParameterInfo element, bool inherit) where T : Attribute =>
			(T)GetCustomAttribute(element, typeof(T), inherit);
#endregion

#region APIs that return all attributes
		/// <summary>
		/// Retrieves a collection of custom attributes that are applied to a specified member, and optionally inspects the
		/// ancestors of that member.
		/// </summary>
		/// <param name="element">The member to inspect.</param>
		/// <returns>
		/// A collection of the custom attributes that are applied to element that match the specified criteria, or an empty
		/// collection if no such attributes exist.
		/// </returns>
		public static IEnumerable<Attribute> GetCustomAttributes(this Assembly element) =>
			Attribute.GetCustomAttributes(element);

		/// <summary>
		/// Retrieves a collection of custom attributes that are applied to a specified member, and optionally inspects the
		/// ancestors of that member.
		/// </summary>
		/// <param name="element">The member to inspect.</param>
		/// <returns>
		/// A collection of the custom attributes that are applied to element that match the specified criteria, or an empty
		/// collection if no such attributes exist.
		/// </returns>
		public static IEnumerable<Attribute> GetCustomAttributes(this Module element) =>
			Attribute.GetCustomAttributes(element);

		/// <summary>
		/// Retrieves a collection of custom attributes that are applied to a specified member, and optionally inspects the
		/// ancestors of that member.
		/// </summary>
		/// <param name="element">The member to inspect.</param>
		/// <returns>
		/// A collection of the custom attributes that are applied to element that match the specified criteria, or an empty
		/// collection if no such attributes exist.
		/// </returns>
		public static IEnumerable<Attribute> GetCustomAttributes(this MemberInfo element) =>
			Attribute.GetCustomAttributes(element);

		/// <summary>
		/// Retrieves a collection of custom attributes that are applied to a specified member, and optionally inspects the
		/// ancestors of that member.
		/// </summary>
		/// <param name="element">The member to inspect.</param>
		/// <returns>
		/// A collection of the custom attributes that are applied to element that match the specified criteria, or an empty
		/// collection if no such attributes exist.
		/// </returns>
		public static IEnumerable<Attribute> GetCustomAttributes(this ParameterInfo element) =>
			Attribute.GetCustomAttributes(element);

		/// <summary>
		/// Retrieves a collection of custom attributes that are applied to a specified member, and optionally inspects the
		/// ancestors of that member.
		/// </summary>
		/// <param name="element">The member to inspect.</param>
		/// <param name="inherit">true to inspect the ancestors of element; otherwise, false.</param>
		/// <returns>
		/// A collection of the custom attributes that are applied to element that match the specified criteria, or an empty
		/// collection if no such attributes exist.
		/// </returns>
		public static IEnumerable<Attribute> GetCustomAttributes(this MemberInfo element, bool inherit) =>
			Attribute.GetCustomAttributes(element, inherit);

		/// <summary>
		/// Retrieves a collection of custom attributes that are applied to a specified member, and optionally inspects the
		/// ancestors of that member.
		/// </summary>
		/// <param name="element">The member to inspect.</param>
		/// <param name="inherit">true to inspect the ancestors of element; otherwise, false.</param>
		/// <returns>
		/// A collection of the custom attributes that are applied to element that match the specified criteria, or an empty
		/// collection if no such attributes exist.
		/// </returns>
		public static IEnumerable<Attribute> GetCustomAttributes(this ParameterInfo element, bool inherit) =>
			Attribute.GetCustomAttributes(element, inherit);
#endregion

#region APIs that return all attributes of a particular type
		/// <summary>
		/// Retrieves a collection of custom attributes that are applied to a specified member, and optionally inspects the
		/// ancestors of that member.
		/// </summary>
		/// <param name="element">The member to inspect.</param>
		/// <param name="attributeType">The type of attribute to search for.</param>
		/// <returns>
		/// A collection of the custom attributes that are applied to element that match the specified criteria, or an empty
		/// collection if no such attributes exist.
		/// </returns>
		public static IEnumerable<Attribute> GetCustomAttributes(this Assembly element, Type attributeType) =>
			Attribute.GetCustomAttributes(element, attributeType);

		/// <summary>
		/// Retrieves a collection of custom attributes that are applied to a specified member, and optionally inspects the
		/// ancestors of that member.
		/// </summary>
		/// <param name="element">The member to inspect.</param>
		/// <param name="attributeType">The type of attribute to search for.</param>
		/// <returns>
		/// A collection of the custom attributes that are applied to element that match the specified criteria, or an empty
		/// collection if no such attributes exist.
		/// </returns>
		public static IEnumerable<Attribute> GetCustomAttributes(this Module element, Type attributeType) =>
			Attribute.GetCustomAttributes(element, attributeType);

		/// <summary>
		/// Retrieves a collection of custom attributes that are applied to a specified member, and optionally inspects the
		/// ancestors of that member.
		/// </summary>
		/// <param name="element">The member to inspect.</param>
		/// <param name="attributeType">The type of attribute to search for.</param>
		/// <returns>
		/// A collection of the custom attributes that are applied to element that match the specified criteria, or an empty
		/// collection if no such attributes exist.
		/// </returns>
		public static IEnumerable<Attribute> GetCustomAttributes(this MemberInfo element, Type attributeType) =>
			Attribute.GetCustomAttributes(element, attributeType);

		/// <summary>
		/// Retrieves a collection of custom attributes that are applied to a specified member, and optionally inspects the
		/// ancestors of that member.
		/// </summary>
		/// <param name="element">The member to inspect.</param>
		/// <param name="attributeType">The type of attribute to search for.</param>
		/// <returns>
		/// A collection of the custom attributes that are applied to element that match the specified criteria, or an empty
		/// collection if no such attributes exist.
		/// </returns>
		public static IEnumerable<Attribute> GetCustomAttributes(this ParameterInfo element, Type attributeType) =>
			Attribute.GetCustomAttributes(element, attributeType);

		/// <summary>
		/// Retrieves a collection of custom attributes that are applied to a specified member, and optionally inspects the
		/// ancestors of that member.
		/// </summary>
		/// <param name="element">The member to inspect.</param>
		/// <typeparam name="T">The type of attribute to search for.</typeparam>
		/// <returns>
		/// A collection of the custom attributes that are applied to element that match the specified criteria, or an empty
		/// collection if no such attributes exist.
		/// </returns>
		public static IEnumerable<T> GetCustomAttributes<T>(this Assembly element) where T : Attribute =>
			(IEnumerable<T>)GetCustomAttributes(element, typeof(T));

		/// <summary>
		/// Retrieves a collection of custom attributes that are applied to a specified member, and optionally inspects the
		/// ancestors of that member.
		/// </summary>
		/// <param name="element">The member to inspect.</param>
		/// <typeparam name="T">The type of attribute to search for.</typeparam>
		/// <returns>
		/// A collection of the custom attributes that are applied to element that match the specified criteria, or an empty
		/// collection if no such attributes exist.
		/// </returns>
		public static IEnumerable<T> GetCustomAttributes<T>(this Module element) where T : Attribute =>
			(IEnumerable<T>)GetCustomAttributes(element, typeof(T));

		/// <summary>
		/// Retrieves a collection of custom attributes that are applied to a specified member, and optionally inspects the
		/// ancestors of that member.
		/// </summary>
		/// <param name="element">The member to inspect.</param>
		/// <typeparam name="T">The type of attribute to search for.</typeparam>
		/// <returns>
		/// A collection of the custom attributes that are applied to element that match the specified criteria, or an empty
		/// collection if no such attributes exist.
		/// </returns>
		public static IEnumerable<T> GetCustomAttributes<T>(this MemberInfo element) where T : Attribute =>
			(IEnumerable<T>)GetCustomAttributes(element, typeof(T));

		/// <summary>
		/// Retrieves a collection of custom attributes that are applied to a specified member, and optionally inspects the
		/// ancestors of that member.
		/// </summary>
		/// <param name="element">The member to inspect.</param>
		/// <typeparam name="T">The type of attribute to search for.</typeparam>
		/// <returns>
		/// A collection of the custom attributes that are applied to element that match the specified criteria, or an empty
		/// collection if no such attributes exist.
		/// </returns>
		public static IEnumerable<T> GetCustomAttributes<T>(this ParameterInfo element) where T : Attribute =>
			(IEnumerable<T>)GetCustomAttributes(element, typeof(T));

		/// <summary>
		/// Retrieves a collection of custom attributes that are applied to a specified member, and optionally inspects the
		/// ancestors of that member.
		/// </summary>
		/// <param name="element">The member to inspect.</param>
		/// <param name="attributeType">The type of attribute to search for.</param>
		/// <param name="inherit">true to inspect the ancestors of element; otherwise, false.</param>
		/// <returns>
		/// A collection of the custom attributes that are applied to element that match the specified criteria, or an empty
		/// collection if no such attributes exist.
		/// </returns>
		public static IEnumerable<Attribute> GetCustomAttributes(
				this MemberInfo element,
				Type attributeType,
				bool inherit) =>
			Attribute.GetCustomAttributes(element, attributeType, inherit);

		/// <summary>
		/// Retrieves a collection of custom attributes that are applied to a specified member, and optionally inspects the
		/// ancestors of that member.
		/// </summary>
		/// <param name="element">The member to inspect.</param>
		/// <param name="attributeType">The type of attribute to search for.</param>
		/// <param name="inherit">true to inspect the ancestors of element; otherwise, false.</param>
		/// <returns>
		/// A collection of the custom attributes that are applied to element that match the specified criteria, or an empty
		/// collection if no such attributes exist.
		/// </returns>
		public static IEnumerable<Attribute> GetCustomAttributes(
				this ParameterInfo element,
				Type attributeType,
				bool inherit) =>
			Attribute.GetCustomAttributes(element, attributeType, inherit);

		/// <summary>
		/// Retrieves a collection of custom attributes that are applied to a specified member, and optionally inspects the
		/// ancestors of that member.
		/// </summary>
		/// <param name="element">The member to inspect.</param>
		/// <param name="inherit">true to inspect the ancestors of element; otherwise, false.</param>
		/// <typeparam name="T">The type of attribute to search for.</typeparam>
		/// <returns>
		/// A collection of the custom attributes that are applied to element that match the specified criteria, or an empty
		/// collection if no such attributes exist.
		/// </returns>
		public static IEnumerable<T> GetCustomAttributes<T>(this MemberInfo element, bool inherit) where T : Attribute =>
			(IEnumerable<T>)GetCustomAttributes(element, typeof(T), inherit);

		/// <summary>
		/// Retrieves a collection of custom attributes that are applied to a specified member, and optionally inspects the
		/// ancestors of that member.
		/// </summary>
		/// <param name="element">The member to inspect.</param>
		/// <param name="inherit">true to inspect the ancestors of element; otherwise, false.</param>
		/// <typeparam name="T">The type of attribute to search for.</typeparam>
		/// <returns>
		/// A collection of the custom attributes that are applied to element that match the specified criteria, or an empty
		/// collection if no such attributes exist.
		/// </returns>
		public static IEnumerable<T> GetCustomAttributes<T>(this ParameterInfo element, bool inherit) where T : Attribute =>
			(IEnumerable<T>)GetCustomAttributes(element, typeof(T), inherit);
#endregion

#region IsDefined
		/// <summary>
		/// Indicates whether custom attributes of a specified type are applied to a specified member, and, optionally,
		/// applied to its ancestors.
		/// </summary>
		/// <param name="element">The member to inspect.</param>
		/// <param name="attributeType">The type of the attribute to search for.</param>
		/// <returns>true if an attribute of the specified type is applied to element; otherwise, false.</returns>
		public static bool IsDefined(this Assembly element, Type attributeType) =>
			Attribute.IsDefined(element, attributeType);

		/// <summary>
		/// Indicates whether custom attributes of a specified type are applied to a specified member, and, optionally,
		/// applied to its ancestors.
		/// </summary>
		/// <param name="element">The member to inspect.</param>
		/// <param name="attributeType">The type of the attribute to search for.</param>
		/// <returns>true if an attribute of the specified type is applied to element; otherwise, false.</returns>
		public static bool IsDefined(this Module element, Type attributeType) => Attribute.IsDefined(element, attributeType);

		/// <summary>
		/// Indicates whether custom attributes of a specified type are applied to a specified member, and, optionally,
		/// applied to its ancestors.
		/// </summary>
		/// <param name="element">The member to inspect.</param>
		/// <param name="attributeType">The type of the attribute to search for.</param>
		/// <returns>true if an attribute of the specified type is applied to element; otherwise, false.</returns>
		public static bool IsDefined(this MemberInfo element, Type attributeType) =>
			Attribute.IsDefined(element, attributeType);

		/// <summary>
		/// Indicates whether custom attributes of a specified type are applied to a specified member, and, optionally,
		/// applied to its ancestors.
		/// </summary>
		/// <param name="element">The member to inspect.</param>
		/// <param name="attributeType">The type of the attribute to search for.</param>
		/// <returns>true if an attribute of the specified type is applied to element; otherwise, false.</returns>
		public static bool IsDefined(this ParameterInfo element, Type attributeType) =>
			Attribute.IsDefined(element, attributeType);

		/// <summary>
		/// Indicates whether custom attributes of a specified type are applied to a specified member, and, optionally,
		/// applied to its ancestors.
		/// </summary>
		/// <param name="element">The member to inspect.</param>
		/// <param name="attributeType">The type of the attribute to search for.</param>
		/// <param name="inherit">true to inspect the ancestors of element; otherwise, false.</param>
		/// <returns>true if an attribute of the specified type is applied to element; otherwise, false.</returns>
		public static bool IsDefined(this MemberInfo element, Type attributeType, bool inherit) =>
			Attribute.IsDefined(element, attributeType, inherit);

		/// <summary>
		/// Indicates whether custom attributes of a specified type are applied to a specified member, and, optionally,
		/// applied to its ancestors.
		/// </summary>
		/// <param name="element">The member to inspect.</param>
		/// <param name="attributeType">The type of the attribute to search for.</param>
		/// <param name="inherit">true to inspect the ancestors of element; otherwise, false.</param>
		/// <returns>true if an attribute of the specified type is applied to element; otherwise, false.</returns>
		public static bool IsDefined(this ParameterInfo element, Type attributeType, bool inherit) =>
			Attribute.IsDefined(element, attributeType, inherit);
#endregion
	}
}
#endif