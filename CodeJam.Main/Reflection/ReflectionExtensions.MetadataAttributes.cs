#if TARGETS_NET || NETSTANDARD15_OR_GREATER || TARGETS_NETCOREAPP // PUBLIC_API_CHANGES
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using CodeJam.Collections;
using CodeJam.Targeting;

using JetBrains.Annotations;

// ReSharper disable once CheckNamespace

namespace CodeJam.Reflection
{
	/// <summary>
	/// Reflection extension methods.
	/// </summary>
	public static partial class ReflectionExtensions
	{
		private sealed class TypeHandleComparer : IEqualityComparer<Type>
		{
			public static TypeHandleComparer Default { get; } = new();

			public bool Equals(Type? x, Type? y) =>
				x is null ? y is null : y != null && x.TypeHandle.Equals(y.TypeHandle);

			public int GetHashCode(Type obj) => obj.TypeHandle.GetHashCode();
		}

		private sealed class MethodMethodHandleComparer : IEqualityComparer<MethodInfo>
		{
			public static MethodMethodHandleComparer Default { get; } = new();

			public bool Equals(MethodInfo? x, MethodInfo? y)
			{
				if (x is null) return y is null;
				if (y == null) return false;

#if TARGETS_NET || NETSTANDARD20_OR_GREATER || NETCOREAPP20_OR_GREATER
				return x.MethodHandle.Equals(y.MethodHandle);
#else
				return TypeHandleComparer.Default.Equals(x.DeclaringType, y.DeclaringType)
					&& x.GetMetadataToken() == y.GetMetadataToken();
#endif
			}

			public int GetHashCode(MethodInfo obj) =>
#if TARGETS_NET || NETSTANDARD20_OR_GREATER || NETCOREAPP20_OR_GREATER
				obj.MethodHandle.GetHashCode();
#else
				HashCode.Combine(
					TypeHandleComparer.Default.GetHashCode(obj.DeclaringType),
					obj.GetMetadataToken());
#endif
		}

		// DONTTOUCH: Direct compare may result in false negative.
		// See http://stackoverflow.com/questions/27645408 for explanation.
		[NotNull]
		private static readonly IEqualityComparer<Type> _typeComparer = TypeHandleComparer.Default;

		[NotNull]
		private static readonly IEqualityComparer<MethodInfo> _methodComparer = MethodMethodHandleComparer.Default;

		#region GetMetadataAttributes
#pragma warning disable 1574, 1584, 1581, 1580 // cref could not be resolved
		/// <summary>
		/// Performs search for metadata attribute.
		/// The search is performed in the following order
		/// * member attributes, base implementation attributes (if the <paramref name="attributeProvider"/> is member of the type)
		/// * type attributes, base type attributes (if the <paramref name="attributeProvider"/> is type or member of the type)
		/// * container type attributes (if the type is nested type)
		/// * assembly attributes.
		/// </summary>
		/// <remarks>
		/// Search logic for each level matches to the
		/// <see cref="Attribute.GetCustomAttributes(MemberInfo,Type,bool)"/> method (inherit = <c>true</c>).
		/// including checks of <see cref="AttributeUsageAttribute"/>.
		/// Ordering of attributes at each level is undefined and depends on runtime implementation.
		/// </remarks>
		/// <typeparam name="TAttribute">Type of the attribute or type of the interface implemented by the attributes.</typeparam>
		/// <param name="attributeProvider">Metadata attribute source.</param>
		/// <returns>First attribute found.</returns>
		public static TAttribute? TryGetMetadataAttribute<TAttribute>(
			[NotNull] this ICustomAttributeProvider attributeProvider)
			where TAttribute : class =>
				TryGetMetadataAttribute<TAttribute>(attributeProvider, false);

		/// <summary>
		/// Performs search for metadata attribute.
		/// If the <paramref name="thisLevelOnly"/> is <c>true</c>, the search is performed in the following order:
		/// * member attributes, base implementation attributes (if the <paramref name="attributeProvider"/> is member of the type)
		/// * type attributes, base type attributes (if the <paramref name="attributeProvider"/> is type or member of the type)
		/// * container type attributes (if the type is nested type)
		/// * assembly attributes.
		/// </summary>
		/// <remarks>
		/// Search logic for each level matches to the
		/// <see cref="Attribute.GetCustomAttributes(MemberInfo,Type,bool)"/> method (inherit = <c>true</c>).
		/// including checks of <see cref="AttributeUsageAttribute"/>.
		/// Ordering of attributes at each level is undefined and depends on runtime implementation.
		/// </remarks>
		/// <typeparam name="TAttribute">Type of the attribute or type of the interface implemented by the attributes.</typeparam>
		/// <param name="attributeProvider">Metadata attribute source.</param>
		/// <param name="thisLevelOnly">Do not check containers for the attributes.</param>
		/// <returns>First attribute found.</returns>
		public static TAttribute? TryGetMetadataAttribute<TAttribute>(
			[NotNull] this ICustomAttributeProvider attributeProvider,
			bool thisLevelOnly)
			where TAttribute : class =>
				GetMetadataAttributes<TAttribute>(attributeProvider, thisLevelOnly)
					.FirstOrDefault();

		/// <summary>
		/// Performs search for metadata attributes.
		/// The search is performed in the following order:
		/// * member attributes, base implementation attributes (if the <paramref name="attributeProvider"/> is member of the type)
		/// * type attributes, base type attributes (if the <paramref name="attributeProvider"/> is type or member of the type)
		/// * container type attributes (if the type is nested type)
		/// * assembly attributes.
		/// </summary>
		/// <remarks>
		/// Search logic for each level matches to the
		/// <see cref="Attribute.GetCustomAttributes(MemberInfo,Type,bool)"/> method (inherit = <c>true</c>).
		/// including checks of <see cref="AttributeUsageAttribute"/>.
		/// Ordering of attributes at each level is undefined and depends on runtime implementation.
		/// </remarks>
		/// <typeparam name="TAttribute">Type of the attribute or type of the interface implemented by the attributes.</typeparam>
		/// <param name="attributeProvider">Metadata attribute source.</param>
		/// <returns>Metadata attributes.</returns>
		[NotNull]
		public static IEnumerable<TAttribute> GetMetadataAttributes<TAttribute>(
			[NotNull] this ICustomAttributeProvider attributeProvider)
			where TAttribute : class =>
				GetMetadataAttributes<TAttribute>(attributeProvider, false);

		/// <summary>
		/// Performs search for metadata attributes.
		/// If the <paramref name="thisLevelOnly"/> is <c>true</c>, the search is performed in the following order:
		/// * member attributes, base implementation attributes (if the <paramref name="attributeProvider" /> is member of the type)
		/// * type attributes, base type attributes (if the <paramref name="attributeProvider" /> is type or member of the type)
		/// * container type attributes (if the type is nested type)
		/// * assembly attributes.
		/// </summary>
		/// <typeparam name="TAttribute">Type of the attribute or type of the interface implemented by the attributes.</typeparam>
		/// <param name="attributeProvider">Metadata attribute source.</param>
		/// <param name="thisLevelOnly">Do not check containers for the attributes.</param>
		/// <returns>Metadata attributes.</returns>
		/// <remarks>
		/// Search logic for each level matches to the
		/// <see cref="Attribute.GetCustomAttributes(MemberInfo,Type,bool)" /> method (inherit = <c>true</c>).
		/// including checks of <see cref="AttributeUsageAttribute" />.
		/// Ordering of attributes at each level is undefined and depends on runtime implementation.
		/// </remarks>
		[NotNull, ItemNotNull]
		public static IEnumerable<TAttribute> GetMetadataAttributes<TAttribute>(
			[NotNull] this ICustomAttributeProvider attributeProvider,
			bool thisLevelOnly)
			where TAttribute : class
		{
			Code.NotNull(attributeProvider, nameof(attributeProvider));

			return
				attributeProvider switch
				{
					Type type => type.GetAttributesForType<TAttribute>(thisLevelOnly),
					MemberInfo member => member.GetAttributesForMember<TAttribute>(thisLevelOnly),
					_ => GetAttributesFromCandidates<TAttribute>(true, attributeProvider)
				};
		}
#pragma warning restore 1574, 1584, 1581, 1580

		[NotNull, ItemNotNull]
		private static IEnumerable<TAttribute> GetAttributesForType<TAttribute>(
			this Type type, bool thisLevelOnly)
			where TAttribute : class =>
				thisLevelOnly
					? GetAttributesForTypeSingleLevel<TAttribute>(type)
					: GetAttributesForTypeWithNesting<TAttribute>(type);

		[NotNull, ItemNotNull]
		private static IEnumerable<TAttribute> GetAttributesForTypeSingleLevel<TAttribute>(
			this Type type)
			where TAttribute : class
		{
			var inheritanceTypes = Sequence.CreateWhileNotNull(type, t => t.GetBaseType())
				.ToArray();

			// ReSharper disable once CoVariantArrayConversion
			var attributes = GetAttributesFromCandidates<TAttribute>(false, inheritanceTypes);
			foreach (var attribute in attributes)
			{
				yield return attribute;
			}
		}

		[ItemNotNull]
		private static IEnumerable<TAttribute> GetAttributesForTypeWithNesting<TAttribute>(
			this Type type)
			where TAttribute : class
		{
			var visited = new HashSet<Type>(_typeComparer);
			var typesToCheck = Sequence.CreateWhileNotNull(type, t => t.DeclaringType);
			foreach (var typeToCheck in typesToCheck)
			{
				var inheritanceTypes = Sequence.Create(
					typeToCheck,
					t => t != null && !visited.Contains(t),
					t => t?.GetBaseType())
					.ToArray();

				if (inheritanceTypes.Length == 0)
					continue;

				visited.AddRange(inheritanceTypes!); // Always contains no nulls due to predicate

				// ReSharper disable once CoVariantArrayConversion
				var attributes = GetAttributesFromCandidates<TAttribute>(false, inheritanceTypes!);
				foreach (var attribute in attributes)
					yield return attribute;
			}

			foreach (var attribute in GetAttributesFromCandidates<TAttribute>(true, type.GetAssembly()))
				yield return attribute;
		}

		[NotNull, ItemNotNull]
		private static IEnumerable<TAttribute> GetAttributesForMember<TAttribute>(
			[NotNull] this MemberInfo member, bool thisLevelOnly)
			where TAttribute : class
		{
			// ReSharper disable once CoVariantArrayConversion
			var attributes = GetAttributesFromCandidates<TAttribute>(false, member.GetOverrideChain());
			foreach (var attribute in attributes)
			{
				yield return attribute;
			}

			if (!thisLevelOnly && member.DeclaringType != null)
				foreach (var attribute in member.DeclaringType.GetAttributesForTypeWithNesting<TAttribute>())
					yield return attribute;
		}
		#endregion

		#region Get override chain
		[NotNull, ItemNotNull]
		private static MemberInfo[] GetOverrideChain(this MemberInfo member) =>
			IsOverriden(member) ? _getOverrideChainCache(member) : new[] { member };

		private static bool IsOverriden(MemberInfo member)
		{
			return member switch
			{
#if NET45_OR_GREATER || TARGETS_NETSTANDARD || TARGETS_NETCOREAPP
				TypeInfo => throw CodeExceptions.Argument(nameof(member), "Member should not be a type."),
#endif
#if TARGETS_NET || NETSTANDARD20_OR_GREATER || NETCOREAPP20_OR_GREATER
				Type => throw CodeExceptions.Argument(nameof(member), "Member should not be a type."),
#endif
				MethodInfo method => IsOverriden(method),
				PropertyInfo property => IsOverriden(property.GetAccessors(true)[0]),
				EventInfo eventInfo => IsOverriden(eventInfo.GetAddMethod(true)),
				_ => false
			};
		}

		private static bool IsOverriden(MethodInfo? method) =>
			method != null &&
				!method.IsStatic &&
				method.IsVirtual &&
				!_methodComparer.Equals(method, method.GetBaseDefinition());

		[NotNull]
		private static Func<MemberInfo, MemberInfo[]> _getOverrideChainCache = Algorithms.Memoize(
			(MemberInfo m) => GetOverrideChainDispatch(m), true);

		private const BindingFlags _thisTypeMembers =
			BindingFlags.Instance | BindingFlags.Static |
				BindingFlags.Public | BindingFlags.NonPublic |
				BindingFlags.DeclaredOnly;

		[NotNull, ItemNotNull]
		private static MemberInfo[] GetOverrideChainDispatch(MemberInfo member) =>
			member switch
			{
				MethodInfo method => GetOverrideChainCore(
					method,
					m => m,
					t => t.GetMethods(_thisTypeMembers)),
				PropertyInfo property => GetOverrideChainCore(
					property,
					p => p.GetAccessors(true)[0],
					t => t.GetProperties(_thisTypeMembers)),
				EventInfo eventInfo => GetOverrideChainCore(
					eventInfo,
					e => e.GetAddMethod(true)
						?? throw new InvalidOperationException("Event has no add accessor."),
					t => t.GetEvents(_thisTypeMembers)),
				_ => new[] { member }
			};

		[NotNull, ItemNotNull]
		private static MemberInfo[] GetOverrideChainCore<TMember>(
			[NotNull] TMember member,
			[NotNull] Func<TMember, MethodInfo> accessorGetter,
			[NotNull] Func<Type, IEnumerable<TMember>> membersGetter)
			where TMember : MemberInfo
		{
			var result = new List<MemberInfo>();
			var implMethod = accessorGetter(member);
			var baseDefinition = implMethod.GetBaseDefinition();
			var typesToCheck = Sequence.CreateWhileNotNull(implMethod.DeclaringType, t => t?.GetBaseType());
			foreach (var type in typesToCheck)
			{
				foreach (var candidate in membersGetter(type))
				{
					var candidateMethod = accessorGetter(candidate);
					if (_methodComparer.Equals(candidateMethod.GetBaseDefinition(), baseDefinition))
					{
						result.Add(candidate);
						if (_methodComparer.Equals(candidateMethod, baseDefinition))
							return result.ToArray();
					}
				}
			}

			return result.ToArray();
		}
		#endregion

		#region GetAttributesFromCandidates
		private static readonly AttributeUsageAttribute _defaultUsage = new(AttributeTargets.All);

		[NotNull]
		private static readonly Func<Type, AttributeUsageAttribute> _attributeUsages = Algorithms.Memoize(
			(Type t) => t.GetTypeInfo().GetCustomAttribute<AttributeUsageAttribute>(true) ?? _defaultUsage,
			true);

		// BASEDON: https://github.com/dotnet/coreclr/blob/master/src/mscorlib/src/System/Attribute.cs#L28
		// Behavior matches the Attribute.InternalGetCustomAttributes() method.
		[NotNull, ItemNotNull]
		private static TAttribute[] GetAttributesFromCandidates<TAttribute>(
			bool inherit,
			[NotNull, ItemNotNull] params ICustomAttributeProvider[] traversePath)
			where TAttribute : class
		{
			var result = new List<TAttribute>();
			var visitedAttributes = new HashSet<Type>(_typeComparer);
			var checkInherited = false;
			foreach (var attributeProvider in traversePath)
			{
				var attributeCandidates = attributeProvider
					.GetCustomAttributesWithInterfaceSupport<TAttribute>(inherit);
				foreach (var attribute in attributeCandidates)
				{
					var attributeType = attribute.GetType();

					var visited = !visitedAttributes.Add(attributeType);
					var usage = _attributeUsages(attributeType);

					if (checkInherited && !usage.Inherited)
						continue;
					if (visited && !usage.AllowMultiple)
						continue;

					result.Add(attribute);
				}

				checkInherited = true;
			}
			return result.ToArray();
		}

		// BASEDON: https://github.com/dotnet/runtime/blob/master/src/coreclr/src/System.Private.CoreLib/src/System/Attribute.CoreCLR.cs#L16
		// Behavior matches the Attribute.InternalGetCustomAttributes() method.
		[NotNull, ItemNotNull]
		private static TAttribute[] GetAttributesFromCandidates<TAttribute>(
			bool inherit,
			[NotNull, ItemNotNull] params Type[] traversePath)
			where TAttribute : class
		{
			var result = new List<TAttribute>();
			var visitedAttributes = new HashSet<Type>(_typeComparer);
			var checkInherited = false;
			foreach (var attributeProvider in traversePath)
			{
				var attributeCandidates = attributeProvider
					.GetCustomAttributesWithInterfaceSupport<TAttribute>(inherit);
				foreach (var attribute in attributeCandidates)
				{
					var attributeType = attribute.GetType();

					var visited = !visitedAttributes.Add(attributeType);
					var usage = _attributeUsages(attributeType);

					if (checkInherited && !usage.Inherited)
						continue;
					if (visited && !usage.AllowMultiple)
						continue;

					result.Add(attribute);
				}

				checkInherited = true;
			}
			return result.ToArray();
		}
		#endregion
	}
}

#endif