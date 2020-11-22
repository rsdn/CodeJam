﻿using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Linq;

using CodeJam.Collections;

using JetBrains.Annotations;

#if TARGETS_NET || NETSTANDARD15_OR_GREATER || TARGETS_NETCOREAPP
using ParameterModifierEx = System.Reflection.ParameterModifier;
#else
using ParameterModifierEx = System.Object;
#endif

using static CodeJam.Targeting.MethodImplOptionsEx;

namespace CodeJam.Targeting
{
	/// <summary>
	/// Extension methods for <see cref="System.Type"/>
	/// </summary>
	internal static class TypeExtensions
	{
		[NotNull]
		[MethodImpl(AggressiveInlining)]
		public static Assembly GetAssembly([NotNull] this Type type) =>
#if TARGETS_NET || NETSTANDARD20_OR_GREATER || NETCOREAPP20_OR_GREATER
			type.Assembly;
#else
			type.GetTypeInfo().Assembly;
#endif

		[MethodImpl(AggressiveInlining)]
		public static bool GetIsSealed([NotNull] this Type type) =>
#if TARGETS_NET || NETSTANDARD20_OR_GREATER || NETCOREAPP20_OR_GREATER
			type.IsSealed;
#else
			type.GetTypeInfo().IsSealed;
#endif

		[MethodImpl(AggressiveInlining)]
		public static bool GetIsAbstract([NotNull] this Type type) =>
#if TARGETS_NET || NETSTANDARD20_OR_GREATER || NETCOREAPP20_OR_GREATER
			type.IsAbstract;
#else
			type.GetTypeInfo().IsAbstract;
#endif

		[MethodImpl(AggressiveInlining)]
		public static bool GetIsEnum([NotNull] this Type type) =>
#if TARGETS_NET || NETSTANDARD20_OR_GREATER || NETCOREAPP20_OR_GREATER
			type.IsEnum;
#else
			type.GetTypeInfo().IsEnum;
#endif

		[MethodImpl(AggressiveInlining)]
		public static bool GetIsClass([NotNull] this Type type) =>
#if TARGETS_NET || NETSTANDARD20_OR_GREATER || NETCOREAPP20_OR_GREATER
			type.IsClass;
#else
			type.GetTypeInfo().IsClass;
#endif

		[MethodImpl(AggressiveInlining)]
		public static bool GetIsPrimitive([NotNull] this Type type) =>
#if TARGETS_NET || NETSTANDARD20_OR_GREATER || NETCOREAPP20_OR_GREATER
			type.IsPrimitive;
#else
			type.GetTypeInfo().IsPrimitive;
#endif

		[MethodImpl(AggressiveInlining)]
		public static bool GetIsPublic([NotNull] this Type type) =>
#if TARGETS_NET || NETSTANDARD20_OR_GREATER || NETCOREAPP20_OR_GREATER
			type.IsPublic;
#else
			type.GetTypeInfo().IsPublic;
#endif

		[MethodImpl(AggressiveInlining)]
		public static bool GetIsGenericType([NotNull] this Type type) =>
#if TARGETS_NET || NETSTANDARD20_OR_GREATER || NETCOREAPP20_OR_GREATER
			type.IsGenericType;
#else
			type.GetTypeInfo().IsGenericType;
#endif

		[MethodImpl(AggressiveInlining)]
		public static bool GetIsGenericTypeDefinition([NotNull] this Type type) =>
#if TARGETS_NET || NETSTANDARD20_OR_GREATER || NETCOREAPP20_OR_GREATER
			type.IsGenericTypeDefinition;
#else
			type.GetTypeInfo().IsGenericTypeDefinition;
#endif

		[MethodImpl(AggressiveInlining)]
		public static bool GetIsInterface([NotNull] this Type type) =>
#if TARGETS_NET || NETSTANDARD20_OR_GREATER || NETCOREAPP20_OR_GREATER
			type.IsInterface;
#else
			type.GetTypeInfo().IsInterface;
#endif

		[CanBeNull]
		[MethodImpl(AggressiveInlining)]
		public static Type GetBaseType([NotNull] this Type type) =>
#if TARGETS_NET || NETSTANDARD20_OR_GREATER || NETCOREAPP20_OR_GREATER
			type.BaseType;
#else
			type.GetTypeInfo().BaseType;
#endif

		[MethodImpl(AggressiveInlining)]
		public static bool GetIsValueType([NotNull] this Type type) =>
#if TARGETS_NET || NETSTANDARD20_OR_GREATER || NETCOREAPP20_OR_GREATER
			type.IsValueType;
#else
			type.GetTypeInfo().IsValueType;
#endif

		[MethodImpl(AggressiveInlining)]
		public static bool GetContainsGenericParameters([NotNull] this Type type) =>
#if TARGETS_NET || NETSTANDARD20_OR_GREATER || NETCOREAPP20_OR_GREATER
			type.ContainsGenericParameters;
#else
			type.GetTypeInfo().ContainsGenericParameters;
#endif

		[MethodImpl(AggressiveInlining)]
		public static bool GetIsDefined([NotNull] this Type type, [NotNull] Type attributeType, bool inherit) =>
#if TARGETS_NET || NETSTANDARD20_OR_GREATER || NETCOREAPP20_OR_GREATER
			Attribute.IsDefined(type, attributeType, inherit);
#else
			type.GetTypeInfo().IsDefined(attributeType, inherit);
#endif

		[MethodImpl(AggressiveInlining)]
		public static bool GetIsArray([NotNull] this Type type) =>
#if TARGETS_NET || NETSTANDARD20_OR_GREATER || NETCOREAPP20_OR_GREATER
			type.IsArray;
#else
			type.GetTypeInfo().IsArray;
#endif

		[MethodImpl(AggressiveInlining)]
		private static object GetCustomAttributeWithInterfaceSupport(
			[NotNull] this Type type, Type attributeType, bool inherit)
		{
#if TARGETS_NET || NETSTANDARD20_OR_GREATER || NETCOREAPP20_OR_GREATER
			return type.GetCustomAttribute(attributeType, inherit);
#else
			if (attributeType.GetIsInterface())
			{
				try
				{
					return type
						.GetTypeInfo()
						.GetCustomAttributes(inherit)
						.SingleOrDefault(a => attributeType.IsInstanceOfType(a));
				}
				catch (InvalidOperationException)
				{
					throw new AmbiguousMatchException($"Ambiguous match found for attribute {attributeType} on {type}.");
				}
			}
			return type
				.GetTypeInfo()
				.GetCustomAttribute(attributeType, inherit);
#endif
		}

		[MethodImpl(AggressiveInlining)]
		public static T GetCustomAttributeWithInterfaceSupport<T>([NotNull] this Type type, bool inherit)
			where T : class =>
				(T)GetCustomAttributeWithInterfaceSupport(type, typeof(T), inherit);

		[MethodImpl(AggressiveInlining)]
		private static IEnumerable<Attribute> GetCustomAttributesWithInterfaceSupport(
			[NotNull] this Type type, Type attributeType, bool inherit)
		{
#if TARGETS_NET || NETSTANDARD20_OR_GREATER || NETCOREAPP20_OR_GREATER
			return type
				.GetCustomAttributes(attributeType, inherit)
				.Cast<Attribute>();
#else
			if (attributeType.GetIsInterface())
			{
				return type
					.GetTypeInfo()
					.GetCustomAttributes(inherit)
					.Where(a => attributeType.IsInstanceOfType(a));
			}

			return type
				.GetCustomAttributes(attributeType, inherit);
#endif
		}

		[MethodImpl(AggressiveInlining)]
		public static IEnumerable<T> GetCustomAttributesWithInterfaceSupport<T>([NotNull] this Type type, bool inherit)
			where T : class =>
				type.GetCustomAttributesWithInterfaceSupport(typeof(T), inherit)
					.Cast<T>();

#if TARGETS_NET || NETSTANDARD15_OR_GREATER || TARGETS_NETCOREAPP
		[MethodImpl(AggressiveInlining)]
		public static object[] GetCustomAttributesWithInterfaceSupport(
			[NotNull] this ICustomAttributeProvider attributeProvider, Type attributeType, bool inherit)
		{
#if TARGETS_NET || NETSTANDARD20_OR_GREATER || NETCOREAPP20_OR_GREATER
			return attributeProvider.GetCustomAttributes(attributeType, inherit);
#else
			if (attributeType.GetIsInterface())
			{
				return attributeProvider
					.GetCustomAttributes(inherit)
					.Where(a => attributeType.IsInstanceOfType(a))
					.ToArray();
			}

			return attributeProvider.GetCustomAttributes(attributeType, inherit);
#endif
		}

		[MethodImpl(AggressiveInlining)]
		public static T[] GetCustomAttributesWithInterfaceSupport<T>(
			[NotNull] this ICustomAttributeProvider attributeProvider, bool inherit)
			where T : class
		{
#if TARGETS_NET || NETSTANDARD20_OR_GREATER || NETCOREAPP20_OR_GREATER
			return attributeProvider.GetCustomAttributes(typeof(T), inherit).ConvertAll<T>();
#else
			if (typeof(T).GetIsInterface())
			{
				return attributeProvider
					.GetCustomAttributes(inherit)
					.OfType<T>()
					.ToArray();
			}
			return attributeProvider.GetCustomAttributes(typeof(T), inherit).ConvertAll<T>();
#endif
		}
#endif
	}
}