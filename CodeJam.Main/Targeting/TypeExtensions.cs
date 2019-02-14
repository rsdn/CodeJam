using System;
using System.Reflection;
using System.Runtime.CompilerServices;

#if LESSTHAN_NETSTANDARD20 || LESSTHAN_NETCOREAPP20
using System.Collections.Generic;
using System.Linq;
#endif

using static CodeJam.Targeting.MethodImplOptionsExt;

using JetBrains.Annotations;

namespace CodeJam.Targeting
{
	/// <summary>
	/// Extension methods for <see cref="System.Type"/>
	/// </summary>
	internal static class TypeExtensions
	{
		[NotNull]
		[MethodImpl(AggressiveInlining)]
		public static Assembly GetAssembly([NotNull] this Type type)
		{
#if LESSTHAN_NETSTANDARD20 || LESSTHAN_NETCOREAPP20
			return type.GetTypeInfo().Assembly;
#else
			return type.Assembly;
#endif
		}

		[MethodImpl(AggressiveInlining)]
		public static bool GetIsSealed([NotNull] this Type type)
		{
#if LESSTHAN_NETSTANDARD20 || LESSTHAN_NETCOREAPP20
			return type.GetTypeInfo().IsSealed;
#else
			return type.IsSealed;
#endif
		}

		[MethodImpl(AggressiveInlining)]
		public static bool GetIsAbstract([NotNull] this Type type)
		{
#if LESSTHAN_NETSTANDARD20 || LESSTHAN_NETCOREAPP20
			return type.GetTypeInfo().IsAbstract;
#else
			return type.IsAbstract;
#endif
		}

		[MethodImpl(AggressiveInlining)]
		public static bool GetIsEnum([NotNull] this Type type)
		{
#if LESSTHAN_NETSTANDARD20 || LESSTHAN_NETCOREAPP20
			return type.GetTypeInfo().IsEnum;
#else
			return type.IsEnum;
#endif
		}

		[MethodImpl(AggressiveInlining)]
		public static bool GetIsClass([NotNull] this Type type)
		{
#if LESSTHAN_NETSTANDARD20 || LESSTHAN_NETCOREAPP20
			return type.GetTypeInfo().IsClass;
#else
			return type.IsClass;
#endif
		}

		[MethodImpl(AggressiveInlining)]
		public static bool GetIsPrimitive([NotNull] this Type type)
		{
#if LESSTHAN_NETSTANDARD20 || LESSTHAN_NETCOREAPP20
			return type.GetTypeInfo().IsPrimitive;
#else
			return type.IsPrimitive;
#endif
		}

		[MethodImpl(AggressiveInlining)]
		public static bool GetIsPublic([NotNull] this Type type)
		{
#if LESSTHAN_NETSTANDARD20 || LESSTHAN_NETCOREAPP20
			return type.GetTypeInfo().IsPublic;
#else
			return type.IsPublic;
#endif
		}

		[MethodImpl(AggressiveInlining)]
		public static bool GetIsNestedPublic([NotNull] this Type type)
		{
#if LESSTHAN_NETSTANDARD20 || LESSTHAN_NETCOREAPP20
			return type.GetTypeInfo().IsNestedPublic;
#else
			return type.IsNestedPublic;
#endif
		}

		[MethodImpl(AggressiveInlining)]
		public static bool GetIsFromLocalAssembly([NotNull] this Type type)
		{
#if SILVERLIGHT
			string assemblyName = type.GetAssembly().FullName;
#else
			var assemblyName = type.GetAssembly().GetName().Name;
#endif

			try
			{
#if LESSTHAN_NETSTANDARD20 || LESSTHAN_NETCOREAPP20
				Assembly.Load(new AssemblyName { Name = assemblyName });
#else
				Assembly.Load(assemblyName);
#endif
				return true;
			}
			catch
			{
				return false;
			}
		}

		[MethodImpl(AggressiveInlining)]
		public static bool GetIsGenericType([NotNull] this Type type)
		{
#if LESSTHAN_NETSTANDARD20 || LESSTHAN_NETCOREAPP20
			return type.GetTypeInfo().IsGenericType;
#else
			return type.IsGenericType;
#endif
		}

		[MethodImpl(AggressiveInlining)]
		public static bool GetIsGenericTypeDefinition([NotNull] this Type type)
		{
#if LESSTHAN_NETSTANDARD20 || LESSTHAN_NETCOREAPP20
			return type.GetTypeInfo().IsGenericTypeDefinition;
#else
			return type.IsGenericTypeDefinition;
#endif
		}

		[MethodImpl(AggressiveInlining)]
		public static bool GetIsInterface([NotNull] this Type type)
		{
#if LESSTHAN_NETSTANDARD20 || LESSTHAN_NETCOREAPP20
			return type.GetTypeInfo().IsInterface;
#else
			return type.IsInterface;
#endif
		}

		[CanBeNull]
		[MethodImpl(AggressiveInlining)]
		public static Type GetBaseType([NotNull] this Type type)
		{
#if LESSTHAN_NETSTANDARD20 || LESSTHAN_NETCOREAPP20
			return type.GetTypeInfo().BaseType;
#else
			return type.BaseType;
#endif
		}

		[MethodImpl(AggressiveInlining)]
		public static bool GetIsValueType([NotNull] this Type type)
		{
#if LESSTHAN_NETSTANDARD20 || LESSTHAN_NETCOREAPP20
			return type.GetTypeInfo().IsValueType;
#else
			return type.IsValueType;
#endif
		}

		[MethodImpl(AggressiveInlining)]
		public static bool GetContainsGenericParameters([NotNull] this Type type)
		{
#if LESSTHAN_NETSTANDARD20 || LESSTHAN_NETCOREAPP20
			return type.GetTypeInfo().ContainsGenericParameters;
#else
			return type.ContainsGenericParameters;
#endif
		}

		[MethodImpl(AggressiveInlining)]
		public static bool GetIsDefined([NotNull] this Type type, [NotNull] Type attributeType)
		{
#if LESSTHAN_NETSTANDARD20 || LESSTHAN_NETCOREAPP20
			return type.GetTypeInfo().IsDefined(attributeType);
#else
			return Attribute.IsDefined(type, attributeType);
#endif
		}

		[MethodImpl(AggressiveInlining)]
		public static bool GetIsDefined([NotNull] this Type type, [NotNull] Type attributeType, bool inherit)
		{
#if LESSTHAN_NETSTANDARD20 || LESSTHAN_NETCOREAPP20
			return type.GetTypeInfo().IsDefined(attributeType, inherit);
#else
			return Attribute.IsDefined(type, attributeType, inherit);
#endif
		}

		[MethodImpl(AggressiveInlining)]
		public static bool GetIsArray([NotNull] this Type type)
		{
#if LESSTHAN_NETSTANDARD20 || LESSTHAN_NETCOREAPP20
			return type.GetTypeInfo().IsArray;
#else
			return type.IsArray;
#endif
		}

		[MethodImpl(AggressiveInlining)]
		public static PropertyInfo GetGetProperty([NotNull] this Type type, [NotNull] string propertyName)
		{
#if LESSTHAN_NETSTANDARD20 || LESSTHAN_NETCOREAPP20
			return type.GetTypeInfo().GetProperty(propertyName);
#else
			return type.GetProperty(propertyName);
#endif
		}

		[MethodImpl(AggressiveInlining)]
		public static T GetPropertyValue<T>([NotNull] this Type type, [NotNull] string propertyName, object target)
		{
#if LESSTHAN_NETSTANDARD20 || LESSTHAN_NETCOREAPP20
			var property = type.GetTypeInfo().GetDeclaredProperty(propertyName);
			return (T)property.GetValue(target);
#else
			return (T)type.InvokeMember(propertyName, BindingFlags.GetProperty, null, target, null);
#endif
		}

		[MethodImpl(AggressiveInlining)]
		public static void SetPropertyValue(
			[NotNull] this Type type, [NotNull] string propertyName, object target, object value)
		{
#if LESSTHAN_NETSTANDARD20 || LESSTHAN_NETCOREAPP20
			var property = type.GetTypeInfo().GetDeclaredProperty(propertyName);
			property.SetValue(target, value);
#else
			type.InvokeMember(propertyName, BindingFlags.SetProperty, null, target, new object[] { value });
#endif
		}

		[MethodImpl(AggressiveInlining)]
		public static T GetFieldValue<T>([NotNull] this Type type, [NotNull] string fieldName, object target)
		{
#if LESSTHAN_NETSTANDARD20 || LESSTHAN_NETCOREAPP20
			var field = type.GetTypeInfo().GetDeclaredField(fieldName);
			return (T)field.GetValue(target);
#else
			return (T)type.InvokeMember(fieldName, BindingFlags.GetField | BindingFlags.GetProperty, null, target, null);
#endif
		}

		[MethodImpl(AggressiveInlining)]
		public static void SetFieldValue([NotNull] this Type type, [NotNull] string fieldName, object target, object value)
		{
#if LESSTHAN_NETSTANDARD20 || LESSTHAN_NETCOREAPP20
			var field = type.GetTypeInfo().GetDeclaredField(fieldName);
			if (field != null)
			{
				field.SetValue(target, value);
			}
			else
			{
				type.SetPropertyValue(fieldName, target, value);
			}
#else
			type.InvokeMember(fieldName, BindingFlags.SetField | BindingFlags.SetProperty, null, target, new object[] { value });
#endif
		}

		[MethodImpl(AggressiveInlining)]
		public static void InvokeMethod<T>([NotNull] this Type type, [NotNull] string methodName, object target, T value)
		{
#if LESSTHAN_NETSTANDARD20 || LESSTHAN_NETCOREAPP20
			var method = type.GetTypeInfo().GetDeclaredMethod(methodName);
			method.Invoke(target, new object[] { value });
#else
			type.InvokeMember(methodName, BindingFlags.InvokeMethod, null, target, new object[] { value });
#endif
		}


#if LESSTHAN_NETSTANDARD20 || LESSTHAN_NETCOREAPP20

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

			if (bindingAttr == BindingFlags.Default) bindingAttr = DefaultLookup;

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

			if (bindingAttr == BindingFlags.Default) bindingAttr = DefaultLookup;

			return type.GetConstructors(bindingAttr).TryFindParametersTypesMatch(types);
		}

		[CanBeNull]
		private static T TryFindParametersTypesMatch<T>(
			[NotNull, ItemNotNull] this IEnumerable<T> methods,
			[NotNull, ItemNotNull] Type[] types)
			where T : MethodBase
			=> methods.Where(m =>
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

		[NotNull]
		[MethodImpl(AggressiveInlining)]
		public static InterfaceMapping GetInterfaceMap([NotNull] this Type type, Type interfaceType)
			=> type.GetTypeInfo().GetRuntimeInterfaceMap(interfaceType);

#endif
	}
}