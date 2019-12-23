#if NET40_OR_GREATER || TARGETS_NETSTANDARD || TARGETS_NETCOREAPP // PUBLIC_API_CHANGES. TODO: update after fixes in Theraot.Core
using System;
using System.Reflection;

namespace CodeJam.Metadata
{
	/// <summary>
	/// Metadata reader interface.
	/// </summary>
	public interface IMetadataReader
	{
		/// <summary>
		/// Returns custom attributes applied to provided type.
		/// </summary>
		/// <param name="type">Object type</param>
		/// <param name="inherit"><b>true</b> to search this member's inheritance chain to find the attributes; otherwise, <b>false</b>.</param>
		/// <typeparam name="T">The type of attribute to search for. Only attributes that are assignable to this type are returned.</typeparam>
		/// <returns>Array of custom attributes.</returns>
		T[] GetAttributes<T>(Type type, bool inherit = true) where T : Attribute;

		/// <summary>
		/// Returns custom attributes applied to provided type member.
		/// </summary>
		/// <param name="memberInfo">Type member.</param>
		/// <param name="inherit"><b>true</b> to search this member's inheritance chain to find the attributes; otherwise, <b>false</b>.</param>
		/// <typeparam name="T">The type of attribute to search for. Only attributes that are assignable to this member are returned.</typeparam>
		/// <returns>Array of custom attributes.</returns>
		T[] GetAttributes<T>(MemberInfo memberInfo, bool inherit = true) where T : Attribute;
	}
}
#endif
