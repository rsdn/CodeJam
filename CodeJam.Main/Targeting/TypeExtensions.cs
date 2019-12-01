using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Linq;

using CodeJam.Collections;

using JetBrains.Annotations;

#if LESSTHAN_NET20 || LESSTHAN_NETSTANDARD15 || LESSTHAN_NETCOREAPP10
using ParameterModifier = System.Object;
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
#if LESSTHAN_NET20 || LESSTHAN_NETSTANDARD20 || LESSTHAN_NETCOREAPP20
			type.GetTypeInfo().Assembly;
#else
			type.Assembly;
#endif

		[MethodImpl(AggressiveInlining)]
		public static bool GetIsSealed([NotNull] this Type type) =>
#if LESSTHAN_NET20 || LESSTHAN_NETSTANDARD20 || LESSTHAN_NETCOREAPP20
			type.GetTypeInfo().IsSealed;
#else
			type.IsSealed;
#endif

		[MethodImpl(AggressiveInlining)]
		public static bool GetIsAbstract([NotNull] this Type type) =>
#if LESSTHAN_NET20 || LESSTHAN_NETSTANDARD20 || LESSTHAN_NETCOREAPP20
			type.GetTypeInfo().IsAbstract;
#else
			type.IsAbstract;
#endif

		[MethodImpl(AggressiveInlining)]
		public static bool GetIsEnum([NotNull] this Type type) =>
#if LESSTHAN_NET20 || LESSTHAN_NETSTANDARD20 || LESSTHAN_NETCOREAPP20
			type.GetTypeInfo().IsEnum;
#else
			type.IsEnum;
#endif

		[MethodImpl(AggressiveInlining)]
		public static bool GetIsClass([NotNull] this Type type) =>
#if LESSTHAN_NET20 || LESSTHAN_NETSTANDARD20 || LESSTHAN_NETCOREAPP20
			type.GetTypeInfo().IsClass;
#else
			type.IsClass;
#endif

		[MethodImpl(AggressiveInlining)]
		public static bool GetIsPrimitive([NotNull] this Type type) =>
#if LESSTHAN_NET20 || LESSTHAN_NETSTANDARD20 || LESSTHAN_NETCOREAPP20
			type.GetTypeInfo().IsPrimitive;
#else
			type.IsPrimitive;
#endif

		[MethodImpl(AggressiveInlining)]
		public static bool GetIsPublic([NotNull] this Type type) =>
#if LESSTHAN_NET20 || LESSTHAN_NETSTANDARD20 || LESSTHAN_NETCOREAPP20
			type.GetTypeInfo().IsPublic;
#else
			type.IsPublic;
#endif

		[MethodImpl(AggressiveInlining)]
		public static bool GetIsGenericType([NotNull] this Type type) =>
#if LESSTHAN_NET20 || LESSTHAN_NETSTANDARD20 || LESSTHAN_NETCOREAPP20
			type.GetTypeInfo().IsGenericType;
#else
			type.IsGenericType;
#endif

		[MethodImpl(AggressiveInlining)]
		public static bool GetIsGenericTypeDefinition([NotNull] this Type type) =>
#if LESSTHAN_NET20 || LESSTHAN_NETSTANDARD20 || LESSTHAN_NETCOREAPP20
			type.GetTypeInfo().IsGenericTypeDefinition;
#else
			type.IsGenericTypeDefinition;
#endif

		[MethodImpl(AggressiveInlining)]
		public static bool GetIsInterface([NotNull] this Type type) =>
#if LESSTHAN_NET20 || LESSTHAN_NETSTANDARD20 || LESSTHAN_NETCOREAPP20
			type.GetTypeInfo().IsInterface;
#else
			type.IsInterface;
#endif

		[CanBeNull]
		[MethodImpl(AggressiveInlining)]
		public static Type GetBaseType([NotNull] this Type type) =>
#if LESSTHAN_NET20 || LESSTHAN_NETSTANDARD20 || LESSTHAN_NETCOREAPP20
			type.GetTypeInfo().BaseType;
#else
			type.BaseType;
#endif

		[MethodImpl(AggressiveInlining)]
		public static bool GetIsValueType([NotNull] this Type type) =>
#if LESSTHAN_NET20 || LESSTHAN_NETSTANDARD20 || LESSTHAN_NETCOREAPP20
			type.GetTypeInfo().IsValueType;
#else
			type.IsValueType;
#endif

		[MethodImpl(AggressiveInlining)]
		public static bool GetContainsGenericParameters([NotNull] this Type type) =>
#if LESSTHAN_NET20 || LESSTHAN_NETSTANDARD20 || LESSTHAN_NETCOREAPP20
			type.GetTypeInfo().ContainsGenericParameters;
#else
			type.ContainsGenericParameters;
#endif

		[MethodImpl(AggressiveInlining)]
		public static bool GetIsDefined([NotNull] this Type type, [NotNull] Type attributeType, bool inherit) =>
#if LESSTHAN_NET20 || LESSTHAN_NETSTANDARD20 || LESSTHAN_NETCOREAPP20
			type.GetTypeInfo().IsDefined(attributeType, inherit);
#else
			Attribute.IsDefined(type, attributeType, inherit);
#endif

		[MethodImpl(AggressiveInlining)]
		public static bool GetIsArray([NotNull] this Type type) =>
#if LESSTHAN_NET20 || LESSTHAN_NETSTANDARD20 || LESSTHAN_NETCOREAPP20
			type.GetTypeInfo().IsArray;
#else
			type.IsArray;
#endif

#if LESSTHAN_NET20 || LESSTHAN_NETSTANDARD20 || LESSTHAN_NETCOREAPP20
		[NotNull]
		[MethodImpl(AggressiveInlining)]
		public static InterfaceMapping GetInterfaceMap([NotNull] this Type type, Type interfaceType) =>
			type.GetTypeInfo().GetRuntimeInterfaceMap(interfaceType);

		[MethodImpl(AggressiveInlining)]
		public static MethodInfo GetMethod(
			[NotNull] this Type type,
			[NotNull] string name,
			BindingFlags bindingAttr,
			[CanBeNull] object binder,
			[NotNull, ItemNotNull] Type[] types)
		{
			if (name == null) throw new ArgumentNullException(name, nameof(name));
			if (binder != null) throw new NotImplementedException();
			if (types.Length != 0) throw new NotImplementedException();

			return type.GetMethod(name, bindingAttr, binder, types, null);
		}

		[MethodImpl(AggressiveInlining)]
		public static MethodInfo GetMethod(
			[NotNull] this Type type,
			[NotNull] string name,
			BindingFlags bindingAttr,
			[CanBeNull] object binder,
			[NotNull, ItemNotNull] Type[] types,
			[CanBeNull] ParameterModifier[] modifiers)
		{
			if (name == null) throw new ArgumentNullException(nameof(name));
			if (binder != null) throw new NotImplementedException(nameof(binder));
			if (types == null) throw new ArgumentNullException(nameof(types));
			if (types.Any(t => t == null)) throw new ArgumentNullException(nameof(types));
			if (modifiers != null) throw new NotImplementedException(nameof(modifiers));

			if (bindingAttr == default) bindingAttr = DefaultLookup;

			return type.GetMethods(bindingAttr).Where(m => m.Name == name).TryFindParametersTypesMatch(types);
		}

		[NotNull]
		[MethodImpl(AggressiveInlining)]
		public static ConstructorInfo GetConstructor(
			[NotNull] this Type type,
			BindingFlags bindingAttr,
			[CanBeNull] object binder,
			[NotNull, ItemNotNull] Type[] types,
			[CanBeNull] ParameterModifier[] modifiers)
		{
			if (binder != null) throw new NotImplementedException(nameof(binder));
			if (types == null) throw new ArgumentNullException(nameof(types));
			if (types.Any(t => t == null)) throw new ArgumentNullException(nameof(types));
			if (modifiers != null) throw new NotImplementedException(nameof(modifiers));

			if (bindingAttr == default) bindingAttr = DefaultLookup;

			return type.GetConstructors(bindingAttr).TryFindParametersTypesMatch(types);
		}

		[CanBeNull]
		private static T TryFindParametersTypesMatch<T>(
			[NotNull, ItemNotNull] this IEnumerable<T> methods,
			[NotNull, ItemNotNull] Type[] types)
			where T : MethodBase
			=> methods.Where(
				m =>
				{
					var parameters = m.GetParameters();
					if (parameters.Length != types.Length) return false;

					for (var i = 0; i < parameters.Length; i++)
					{
						if (!parameters[i].ParameterType.Equals(types[i])) return false;
					}

					return true;
				})
				.FirstOrDefault();

		private const BindingFlags DefaultLookup = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public;

		[MethodImpl(AggressiveInlining)]
		public static bool GetIsAssignableFrom([NotNull] this Type type, [NotNull] Type otherType)
			=> type.GetTypeInfo().IsAssignableFrom(otherType.GetTypeInfo());

		[MethodImpl(AggressiveInlining)]
		public static bool GetIsSubclassOf([NotNull] this Type type, [NotNull] Type c) => type.GetTypeInfo().IsSubclassOf(c);

		[MethodImpl(AggressiveInlining)]
		public static T GetCustomAttribute<T>([NotNull] this Type type) where T : Attribute
			=> type.GetTypeInfo().GetCustomAttribute<T>();

		[MethodImpl(AggressiveInlining)]
		public static T GetCustomAttribute<T>([NotNull] this Type type, bool inherit) where T : Attribute
			=> type.GetTypeInfo().GetCustomAttribute<T>(inherit);

		[NotNull, ItemNotNull]
		[MethodImpl(AggressiveInlining)]
		public static Attribute[] GetCustomAttributes([NotNull] this Type type)
			=> type.GetTypeInfo().GetCustomAttributes().ToArray();

		[NotNull, ItemNotNull]
		[MethodImpl(AggressiveInlining)]
		public static Attribute[] GetCustomAttributes([NotNull] this Type type, Type attributeType, bool inherit)
			=> type.GetTypeInfo().GetCustomAttributes(attributeType, inherit).ToArray();
#endif

		[MethodImpl(AggressiveInlining)]
		public static object GetCustomAttributeWithInterfaceSupport(
			[NotNull] this Type type, Type attributeType, bool inherit)
		{
#if LESSTHAN_NET20 || LESSTHAN_NETSTANDARD20 || LESSTHAN_NETCOREAPP20
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
#else
			return type.GetCustomAttribute(attributeType, inherit);
#endif
		}

		[MethodImpl(AggressiveInlining)]
		public static T GetCustomAttributeWithInterfaceSupport<T>([NotNull] this Type type, bool inherit)
			where T : class =>
			(T)GetCustomAttributeWithInterfaceSupport(type, typeof(T), inherit);

		[MethodImpl(AggressiveInlining)]
		public static IEnumerable<Attribute> GetCustomAttributesWithInterfaceSupport(
			[NotNull] this Type type, Type attributeType, bool inherit)
		{
#if LESSTHAN_NET20 || LESSTHAN_NETSTANDARD20 || LESSTHAN_NETCOREAPP20
			if (attributeType.GetIsInterface())
			{
				return type
					.GetTypeInfo()
					.GetCustomAttributes(inherit)
					.Where(a => attributeType.IsInstanceOfType(a));
			}

			return type
				.GetCustomAttributes(attributeType, inherit);
#else
			return type
				.GetCustomAttributes(attributeType, inherit)
				.Cast<Attribute>();
#endif
		}

		[MethodImpl(AggressiveInlining)]
		public static IEnumerable<T> GetCustomAttributesWithInterfaceSupport<T>([NotNull] this Type type, bool inherit)
			where T : class =>
			type.GetCustomAttributesWithInterfaceSupport(typeof(T), inherit)
				.Cast<T>();

#if LESSTHAN_NET20 || LESSTHAN_NETSTANDARD15 || LESSTHAN_NETCOREAPP10 // PUBLIC_API_CHANGES
// ICustomAttributeProvider & .GetMetadataToken() are missing if targeting to these frameworks
#else
		[MethodImpl(AggressiveInlining)]
		public static object[] GetCustomAttributesWithInterfaceSupport(
			[NotNull] this ICustomAttributeProvider attributeProvider, Type attributeType, bool inherit)
		{
#if LESSTHAN_NET20 || LESSTHAN_NETSTANDARD20 || LESSTHAN_NETCOREAPP20
			if (attributeType.GetIsInterface())
			{
				return attributeProvider
					.GetCustomAttributes(inherit)
					.Where(a => attributeType.IsInstanceOfType(a))
					.ToArray();
			}
#endif

			return attributeProvider.GetCustomAttributes(attributeType, inherit);
		}

		[MethodImpl(AggressiveInlining)]
		public static T[] GetCustomAttributesWithInterfaceSupport<T>([NotNull] this ICustomAttributeProvider attributeProvider, bool inherit)
			where T : class
		{
#if LESSTHAN_NET20 || LESSTHAN_NETSTANDARD20 || LESSTHAN_NETCOREAPP20
			if (typeof(T).GetIsInterface())
			{
				return attributeProvider
					.GetCustomAttributes(inherit)
					.OfType<T>()
					.ToArray();
			}
#endif

			return attributeProvider.GetCustomAttributes(typeof(T), inherit).ConvertAll<T>();
		}
#endif
	}
}