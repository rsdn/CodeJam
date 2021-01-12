using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;

using CodeJam.Strings;
using CodeJam.Targeting;

using JetBrains.Annotations;

namespace CodeJam.Reflection
{
	/// <summary>
	/// Reflection extension methods.
	/// </summary>
	[PublicAPI]
	public static partial class ReflectionExtensions
	{
#if TARGETS_NET || NETSTANDARD15_OR_GREATER || TARGETS_NETCOREAPP // PUBLIC_API_CHANGES
		/// <summary>
		/// Returns path to the <paramref name="module"/> file.
		/// </summary>
		/// <param name="module">Assembly.</param>
		/// <returns>Path to <paramref name="module"/>.</returns>
		[JetBrains.Annotations.NotNull]
		[Pure]
		public static string GetModulePath([JetBrains.Annotations.NotNull] this Module module)
		{
			Code.NotNull(module, nameof(module));

			var assemblyPath = module.Assembly.GetAssemblyPath();

			return Equals(module, module.Assembly.ManifestModule)
				? assemblyPath
				: System.IO.Path.Combine(
					// ReSharper disable once AssignNullToNotNullAttribute
					System.IO.Path.GetDirectoryName(assemblyPath) ?? "",
					module.Name);
		}
#endif

		/// <summary>
		/// Gets the short form of assembly qualified type name (without assembly version or assembly key).
		/// </summary>
		/// <example>
		/// <code>
		/// // result is "CodeJam.Reflection.ReflectionExtensions, CodeJam";
		/// var shortNameWithAssembly = typeof(ReflectionExtensions).GetShortAssemblyQualifiedName();
		/// </code>
		/// </example>
		/// <param name="type">The type to get the name for.</param>
		/// <returns>The short form of assembly qualified type name.</returns>
		[JetBrains.Annotations.NotNull]
		[Pure]
		public static string GetShortAssemblyQualifiedName([JetBrains.Annotations.NotNull] this Type type)
		{
			Code.NotNull(type, nameof(type));

			static void WriteAssemblyName(StringBuilder sb, Type t)
			{
				sb.Append(", ");

				var index = -1;

				var assemblyFullName = t.GetAssembly().FullName ?? throw new InvalidOperationException("Assembly has no name.");

				while (true)
				{
					index = assemblyFullName.IndexOf(',', index + 1);
					DebugCode.BugIf(index == 0, "Invalid assembly name");

					if (index < 0)
					{
						sb.Append(assemblyFullName);
						return;
					}

					if (assemblyFullName[index - 1] != '\\')
					{
						sb.Append(assemblyFullName, 0, index);
						return;
					}
				}
			}

			void WriteGenericArguments(StringBuilder sb, Type t)
			{
				DebugCode.AssertState(t.GetIsGenericType() && !t.GetIsGenericTypeDefinition(), "Invalid type");

				sb.Append('[');

				var arguments = t.GetGenericArguments();
				for (var i = 0; i < arguments.Length; i++)
				{
					if (i != 0)
						sb.Append(',');

					sb.Append('[');
					WriteFull(sb, arguments[i]);
					sb.Append(']');
				}

				sb.Append(']');
			}

			void WriteElementType(StringBuilder sb, Type t)
			{
				DebugCode.AssertState(t.IsArray || t.IsPointer || t.IsByRef, "Invalid type");

				Write(sb, t.GetElementType()!); // Always not null for array, pointer or byref

				if (t.IsArray)
				{
					sb.Append('[');
					sb.Append(',', t.GetArrayRank() - 1);
					sb.Append(']');
				}
				else
				{
					sb.Append(t.IsPointer ? '*' : '&');
				}
			}

			static void WriteType(StringBuilder sb, Type t)
			{
				if (t.DeclaringType != null)
				{
					WriteType(sb, t.DeclaringType);
					sb.Append('+');
				}

				sb.Append(t.Name);
			}

			void Write(StringBuilder sb, Type t)
			{
				if (t.GetIsGenericType() && !t.GetIsGenericTypeDefinition())
				{
					WriteType(sb, t);
					WriteGenericArguments(sb, t);
				}
				else if (t.IsArray || t.IsPointer || t.IsByRef)
				{
					WriteElementType(sb, t);
				}
				else
				{
					WriteType(sb, t);
				}
			}

			void WriteFull(StringBuilder sb, Type t)
			{
				if (t.Namespace.NotNullNorEmpty())
				{
					sb.Append(t.Namespace);
					sb.Append('.');
				}

				Write(sb, t);
				WriteAssemblyName(sb, t);
			}

			var builder = new StringBuilder();
			WriteFull(builder, type);
			return builder.ToString();
		}

		/// <summary>
		/// Gets a value indicating whether the <paramref name="type"/> can be instantiated.
		/// </summary>
		/// <param name="type">The <see cref="Type"/> to test.</param>
		/// <returns>
		/// A value indicating whether the <paramref name="type"/> can be instantiated.
		/// </returns>
		[Pure]
		public static bool IsInstantiable([JetBrains.Annotations.NotNull] this Type type)
		{
			Code.NotNull(type, nameof(type));
			return !(type.GetIsAbstract() || type.GetIsInterface() || type.IsArray || type.GetContainsGenericParameters());
		}

		/// <summary>
		/// Gets a value indicating whether the <paramref name="type"/> is declared static.
		/// </summary>
		/// <param name="type">The <see cref="Type"/> to test.</param>
		/// <returns>
		/// A value indicating whether the <paramref name="type"/> is declared static.
		/// </returns>
		[Pure]
		public static bool IsStatic([JetBrains.Annotations.NotNull] this Type type)
		{
			Code.NotNull(type, nameof(type));
			return type.GetIsClass() && type.GetIsAbstract() && type.GetIsSealed();
		}

		/// <summary>
		/// Gets a value indicating whether the <paramref name="type"/> is Nullable&#60;&#62; type.
		/// </summary>
		/// <param name="type">The <see cref="Type"/> to test.</param>
		/// <returns>
		/// A value indicating whether the <paramref name="type"/> is Nullable&#60;&#62;.
		/// </returns>
		[Pure]
		public static bool IsNullable([JetBrains.Annotations.NotNull] this Type type)
		{
			Code.NotNull(type, nameof(type));
			return type.GetIsGenericType() && type.GetGenericTypeDefinition() == typeof(Nullable<>);
		}

		/// <summary>
		/// Checks if <paramref name="type"/> is numeric type.
		/// </summary>
		/// <param name="type">Type to check.</param>
		/// <returns>True, if <paramref name="type"/> is numeric.</returns>
		[Pure]
		public static bool IsNumeric([JetBrains.Annotations.NotNull] this Type type)
		{
			Code.NotNull(type, nameof(type));
			while (true) // tail recursion expanded
#if TARGETS_NET || NETSTANDARD15_OR_GREATER || TARGETS_NETCOREAPP
				switch (Type.GetTypeCode(type))
				{
					case TypeCode.SByte:
					case TypeCode.Byte:
					case TypeCode.Int16:
					case TypeCode.UInt16:
					case TypeCode.Int32:
					case TypeCode.UInt32:
					case TypeCode.Int64:
					case TypeCode.UInt64:
					case TypeCode.Decimal:
					case TypeCode.Single:
					case TypeCode.Double:
						return true;
					case TypeCode.Object:
						var underlyingType = Nullable.GetUnderlyingType(type);
						if (underlyingType != null)
						{
							type = underlyingType;
							continue;
						}
						return false;
					default:
						return false;
				}
#else
				switch (type)
				{
					case Type t when t == typeof(SByte):
					case Type t2 when t2 == typeof(Byte):
					case Type t3 when t3 == typeof(Int16):
					case Type t4 when t4 == typeof(UInt16):
					case Type t5 when t5 == typeof(Int32):
					case Type t6 when t6 == typeof(UInt32):
					case Type t7 when t7 == typeof(Int64):
					case Type t8 when t8 == typeof(UInt64):
					case Type t9 when t9 == typeof(Decimal):
					case Type t10 when t10 == typeof(Single):
					case Type t11 when t11 == typeof(Double):
						return true;
					case Type t12 when Nullable.GetUnderlyingType(t12) is Type nullableType:
						type = nullableType;
						continue;
					default:
						return false;
				}
#endif
		}

		/// <summary>
		/// Checks if <paramref name="type"/> is integer type.
		/// </summary>
		/// <param name="type">Type to check.</param>
		/// <returns>True, if <paramref name="type"/> is integer type.</returns>
		[Pure]
		public static bool IsInteger([JetBrains.Annotations.NotNull] this Type type)
		{
			Code.NotNull(type, nameof(type));
			while (true) // tail recursion expanded
#if TARGETS_NET || NETSTANDARD15_OR_GREATER || TARGETS_NETCOREAPP
				switch (Type.GetTypeCode(type))
				{
					case TypeCode.SByte:
					case TypeCode.Byte:
					case TypeCode.Int16:
					case TypeCode.UInt16:
					case TypeCode.Int32:
					case TypeCode.UInt32:
					case TypeCode.Int64:
					case TypeCode.UInt64:
						return true;
					case TypeCode.Object:
						var underlyingType = Nullable.GetUnderlyingType(type);
						if (underlyingType != null)
						{
							type = underlyingType;
							continue;
						}
						return false;
					default:
						return false;
				}
#else
				switch (type)
				{
					case Type t when t == typeof(SByte):
					case Type t2 when t2 == typeof(Byte):
					case Type t3 when t3 == typeof(Int16):
					case Type t4 when t4 == typeof(UInt16):
					case Type t5 when t5 == typeof(Int32):
					case Type t6 when t6 == typeof(UInt32):
					case Type t7 when t7 == typeof(Int64):
					case Type t8 when t8 == typeof(UInt64):
						return true;
					case Type t12 when Nullable.GetUnderlyingType(t12) is Type nullableType:
						type = nullableType;
						continue;
					default:
						return false;
				}
#endif
		}

		/// <summary>
		/// Checks if <paramref name="type" /> is nullable numeric type.
		/// </summary>
		/// <param name="type">Type to check.</param>
		/// <returns>True, if <paramref name="type" /> is nullable numeric.</returns>
		[Pure]
		public static bool IsNullableNumeric([JetBrains.Annotations.NotNull] this Type type)
		{
			var arg = Nullable.GetUnderlyingType(type);
			return arg != null && arg.IsNumeric();
		}

		/// <summary>
		/// Checks if <paramref name="type"/> is nullable integer type.
		/// </summary>
		/// <param name="type">Type to check.</param>
		/// <returns>True, if <paramref name="type"/> is nullable integer type.</returns>
		[Pure]
		public static bool IsNullableInteger([JetBrains.Annotations.NotNull] this Type type)
		{
			var arg = Nullable.GetUnderlyingType(type);
			return arg != null && arg.IsInteger();
		}

		/// <summary>
		/// Checks if <paramref name="type"/> is nullable enum type.
		/// </summary>
		/// <param name="type">Type to check.</param>
		/// <returns>True, if <paramref name="type"/> is nullable enum type.</returns>
		[Pure]
		public static bool IsNullableEnum([JetBrains.Annotations.NotNull] this Type type)
		{
			var arg = Nullable.GetUnderlyingType(type);
			return arg != null && arg.GetIsEnum();
		}

		/// <summary>
		/// Determines whether the <paramref name="type"/> derives from the specified <paramref name="check"/>.
		/// </summary>
		/// <remarks>
		/// This method also returns false if <paramref name="type"/> and the <paramref name="check"/> are equal.
		/// </remarks>
		/// <param name="type">The type to test.</param>
		/// <param name="check">The type to compare with. </param>
		/// <returns>
		/// true if the <paramref name="type"/> derives from <paramref name="check"/>; otherwise, false.
		/// </returns>
		[Pure]
		public static bool IsSubClass([JetBrains.Annotations.NotNull] this Type type, [JetBrains.Annotations.NotNull] Type check)
		{
			Code.NotNull(type, nameof(type));
			Code.NotNull(check, nameof(check));

			if (type == check)
				return false;

			while (true)
			{
				if (check.GetIsInterface())
					// ReSharper disable once LoopCanBeConvertedToQuery
					foreach (var interfaceType in type.GetInterfaces())
						if (interfaceType == check || interfaceType.IsSubClass(check))
							return true;

				if (type.GetIsGenericType() && !type.GetIsGenericTypeDefinition())
				{
					var definition = type.GetGenericTypeDefinition();
					DebugCode.BugIf(definition == null, "definition == null");
					if (definition == check || definition.IsSubClass(check))
						return true;
				}

				var baseType = type.GetBaseType();

				if (baseType == null)
					return false;

				if (baseType == check)
					return true;
			}
		}

		/// <summary>
		/// Returns delegate parameter infos.
		/// </summary>
		/// <param name="delegateType">Type of delegate</param>
		/// <returns>Array of <see cref="ParameterInfo"/>.</returns>
		[JetBrains.Annotations.NotNull]
		[Pure]
		public static ParameterInfo[] GetDelegateParams([JetBrains.Annotations.NotNull] Type delegateType)
		{
			Code.NotNull(delegateType, nameof(delegateType));

			var invoke = delegateType.GetMethod("Invoke");
			DebugCode.BugIf(invoke == null, "invoke == null");
			return invoke.GetParameters();
		}

		/// <summary>
		/// Returns the underlying type argument of the specified type.
		/// </summary>
		/// <param name="type">A <see cref="System.Type"/> instance. </param>
		/// <returns><list>
		/// <item>The type argument of the type parameter,
		/// if the type parameter is a closed generic nullable type.</item>
		/// <item>The underlying Type if the type parameter is an enum type.</item>
		/// <item>Otherwise, the type itself.</item>
		/// </list>
		/// </returns>
		[JetBrains.Annotations.NotNull]
		[Pure]
		public static Type ToUnderlying([JetBrains.Annotations.NotNull] this Type type)
		{
			Code.NotNull(type, nameof(type));

			type = Nullable.GetUnderlyingType(type) ?? type;
			return type.GetIsEnum() ? Enum.GetUnderlyingType(type) : type;
		}

		/// <summary>
		/// Returns the underlying type argument of the specified nullable type.
		/// </summary>
		/// <param name="type">A <see cref="System.Type"/> instance. </param>
		/// <returns><list>
		/// <item>The type argument of the type parameter,
		/// if the type parameter is a closed generic nullable type.</item>
		/// </list>
		/// </returns>
		[JetBrains.Annotations.NotNull]
		[Pure]
		public static Type ToNullableUnderlying([JetBrains.Annotations.NotNull] this Type type)
		{
			Code.NotNull(type, nameof(type));

			return Nullable.GetUnderlyingType(type) ?? type;
		}

		/// <summary>
		/// Returns the underlying type argument of the specified enum type.
		/// </summary>
		/// <param name="type">A <see cref="System.Type"/> instance. </param>
		/// <returns><list>
		/// <item>The underlying Type if the type parameter is an enum type.</item>
		/// <item>Otherwise, the type itself.</item>
		/// </list>
		/// </returns>
		[JetBrains.Annotations.NotNull]
		[Pure]
		public static Type ToEnumUnderlying([JetBrains.Annotations.NotNull] this Type type)
		{
			Code.NotNull(type, nameof(type));

			return type.GetIsEnum() ? Enum.GetUnderlyingType(type) : type;
		}

		/// <summary>
		/// Gets the type of this member.
		/// </summary>
		/// <param name="memberInfo">A <see cref="System.Reflection.MemberInfo"/> instance. </param>
		/// <returns>
		/// <list>
		/// <item>If the member is a property, returns <see cref="PropertyInfo.PropertyType"/>.</item>
		/// <item>If the member is a field, returns <see cref="FieldInfo.FieldType"/>.</item>
		/// <item>If the member is a method, returns <see cref="MethodInfo.ReturnType"/>.</item>
		/// <item>If the member is a constructor, returns <see cref="MemberInfo.DeclaringType"/>.</item>
		/// <item>If the member is an event, returns <see cref="EventInfo.EventHandlerType"/>.</item>
		/// </list>
		/// </returns>
		[Pure]
		[JetBrains.Annotations.NotNull]
		public static Type GetMemberType([JetBrains.Annotations.NotNull] this MemberInfo memberInfo)
		{
			Code.NotNull(memberInfo, nameof(memberInfo));

			// ReSharper disable once AssignNullToNotNullAttribute
			return memberInfo switch
			{
				PropertyInfo propertyInfo => propertyInfo.PropertyType,
				FieldInfo fieldInfo => fieldInfo.FieldType,
				MethodInfo methodInfo => methodInfo.ReturnType,
				ConstructorInfo constructorInfo => constructorInfo.DeclaringType,
				EventInfo eventInfo => eventInfo.EventHandlerType,
				_ => throw new InvalidOperationException("Invalid member type.")
			}
				?? throw new InvalidOperationException("Member has no type");
		}
		/// <summary>
		/// Returns default constructor.
		/// </summary>
		/// <param name="type">A <see cref="System.Type"/> instance. </param>
		/// <param name="exceptionIfNotExists">if true, throws an exception if type does not exists default constructor.
		/// Otherwise returns null.</param>
		/// <returns>Returns <see cref="ConstructorInfo"/> or null.</returns>
		[Pure]
		[ContractAnnotation("exceptionIfNotExists:true => notnull; exceptionIfNotExists:false => canbenull")]
		public static ConstructorInfo? GetDefaultConstructor(
			this Type type,
			bool exceptionIfNotExists = false)
		{
			Code.NotNull(type, nameof(type));
			var info = type.GetConstructor(
				BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public,
				null,
				Type.EmptyTypes,
				null);

			if (info == null && exceptionIfNotExists)
			{
				throw new InvalidOperationException($"The '{type.FullName}' type must have default or init constructor.");
			}

			return info;
		}

		/// <summary>
		/// Gets a value indicating whether the current <i>Type</i> encompasses or refers to another type;
		/// that is, whether the provided Type is an array, a pointer, or is passed by reference.
		/// </summary>
		/// <param name="type">Type to get item type.</param>
		/// <returns>Returns item type or null.</returns>
		[Pure]
		[ContractAnnotation("type:null => null")]
		public static Type? GetItemType(this Type? type)
		{
			while (true)
			{
				if (type == null)
					return null;

				if (type.HasElementType || type.IsArray)
					return type.GetElementType();

				if (type == typeof(object))
					return null;

				if (type.GetIsGenericType())
					foreach (var aType in type.GetGenericArguments())
						if (typeof(IEnumerable<>).MakeGenericType(aType).IsAssignableFrom(type))
							return aType;

				var interfaces = type.GetInterfaces();

				if (interfaces.Length > 0)
					foreach (var iType in interfaces)
					{
						var eType = iType.GetItemType();

						if (eType != null)
							return eType;
					}

				type = type.GetBaseType();
			}
		}

		/// <summary>
		/// Checks if <paramref name="type"/> is an anonymous type.
		/// </summary>
		/// <param name="type">Type to check.</param>
		/// <returns>True, if <paramref name="type"/> is an anonymous type.</returns>
		[Pure]
		public static bool IsAnonymous([JetBrains.Annotations.NotNull] this Type type)
		{
			Code.NotNull(type, nameof(type));

			return !type.GetIsPublic()
				&& type.GetIsGenericType()
				&& (type.Name.StartsWith("<>f__AnonymousType", StringComparison.Ordinal)
					|| type.Name.StartsWith("VB$AnonymousType", StringComparison.Ordinal))
				&& type.GetIsDefined(typeof(CompilerGeneratedAttribute), false);
		}

		/// <summary>
		/// Checks if <typeparamref name="TAttribute"/> is defined on <paramref name="type"/>.
		/// </summary>
		/// <param name="type">Type to check.</param>
		/// <returns>True, if <typeparamref name="TAttribute"/> is defined on <paramref name="type"/></returns>
		[Pure]
		public static bool IsDefined<TAttribute>([JetBrains.Annotations.NotNull] this Type type) where TAttribute : Attribute
		{
			Code.NotNull(type, nameof(type));

			return type.IsDefined(typeof(TAttribute));
		}

		/// <summary>
		/// Checks if <typeparamref name="TAttribute"/> is defined on <paramref name="member"/>.
		/// </summary>
		/// <param name="member">Member to check.</param>
		/// <returns>True, if <typeparamref name="TAttribute"/> is defined on <paramref name="member"/></returns>
		[Pure]
		public static bool IsDefined<TAttribute>([JetBrains.Annotations.NotNull] this MemberInfo member) where TAttribute : Attribute
		{
			Code.NotNull(member, nameof(member));

			return member.IsDefined(typeof(TAttribute));
		}

		/// <summary>
		/// Checks if <paramref name="type"/> is compiler generated type.
		/// </summary>
		/// <param name="type">Type to check.</param>
		/// <returns>True, if <paramref name="type"/> is generated by compiler.</returns>
		[Pure]
		public static bool IsCompilerGenerated([JetBrains.Annotations.NotNull] this Type type) => type.IsDefined<CompilerGeneratedAttribute>();

		/// <summary>
		/// Checks if <paramref name="member"/> is is compiler generated member.
		/// </summary>
		/// <param name="member">Member to check.</param>
		/// <returns>True, if <paramref name="member"/> is generated by compiler.</returns>
		[Pure]
		public static bool IsCompilerGenerated([JetBrains.Annotations.NotNull] this MemberInfo member) => member.IsDefined<CompilerGeneratedAttribute>();

#if NET35_OR_GREATER || TARGETS_NETSTANDARD || TARGETS_NETCOREAPP // PUBLIC_API_CHANGES
		/// <summary>
		/// Checks if <paramref name="member"/> is is compiler generated member.
		/// </summary>
		/// <param name="member">Member to check.</param>
		/// <returns>True, if <paramref name="member"/> is generated by compiler.</returns>
		[Pure]
		public static bool IsRequired([JetBrains.Annotations.NotNull] this MemberInfo member) => member.IsDefined<RequiredAttribute>();
#endif

		/// <summary>
		/// Checks if <paramref name="type"/> has no <see cref="BrowsableAttribute"/> defined
		/// -or-
		/// <see cref="BrowsableAttribute.Browsable"/> equals to <c>true</c>.
		/// </summary>
		/// <param name="type">Type to check.</param>
		/// <returns>True, if <paramref name="type"/> is browsable.</returns>
		[Pure]
		public static bool IsBrowsable([JetBrains.Annotations.NotNull] this Type type) =>
			type.GetCustomAttribute<BrowsableAttribute>()?.Browsable ?? true;

		/// <summary>
		/// Checks if <paramref name="member"/> has no <see cref="BrowsableAttribute"/> defined
		/// -or-
		/// <see cref="BrowsableAttribute.Browsable"/> equals to <c>true</c>.
		/// </summary>
		/// <param name="member">Member to check.</param>
		/// <returns>True, if <paramref name="member"/> is browsable.</returns>
		[Pure]
		public static bool IsBrowsable([JetBrains.Annotations.NotNull] this MemberInfo member) =>
			member.GetCustomAttribute<BrowsableAttribute>()?.Browsable ?? true;

		/// <summary>
		/// Determines whether instance of <typeparamref name="T"/> can be assigned to instance of current type.
		/// </summary>
		/// <typeparam name="T">Type to check</typeparam>
		/// <param name="type">Target type.</param>
		/// <returns>
		///   <c>true</c> if instance of <typeparamref name="T"/> can be assigned to instance of current type; otherwise, <c>false</c>.
		/// </returns>
		[Pure]
		public static bool IsAssignableFrom<T>([JetBrains.Annotations.NotNull] this Type type) => type.IsAssignableFrom(typeof(T));

		/// <summary>
		/// Determines whether instance of current type can be assigned to instance of <typeparamref name="T"/>.
		/// </summary>
		/// <typeparam name="T">Target type</typeparam>
		/// <param name="type">The type to check.</param>
		/// <returns>
		///   <c>true</c> if instance of current type can be assigned to instance of <typeparamref name="T"/>; otherwise, <c>false</c>.
		/// </returns>
		[Pure]
		public static bool IsAssignableTo<T>([JetBrains.Annotations.NotNull] this Type type) => typeof(T).IsAssignableFrom(type);

		/// <summary>
		/// Determines whether instance of current type can be assigned to instance of <paramref name="targetType"/>.
		/// </summary>
		/// <param name="type">The type to check.</param>
		/// <param name="targetType">Target type.</param>
		/// <returns>
		///   <c>true</c> if instance of current type can be assigned to instance of <paramref name="targetType"/>; otherwise, <c>false</c>.
		/// </returns>
		[Pure]
		public static bool IsAssignableTo(this Type type, Type targetType)
		{
			Code.NotNull(targetType, nameof(targetType));
			return targetType.IsAssignableFrom(type);
		}
	}
}