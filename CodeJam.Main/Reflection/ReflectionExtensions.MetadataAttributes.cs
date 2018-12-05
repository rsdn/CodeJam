using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using CodeJam.Collections;

using JetBrains.Annotations;

// ReSharper disable once CheckNamespace

namespace CodeJam.Reflection
{
	/// <summary>
	/// Reflection extension methods.
	/// </summary>
	public static partial class ReflectionExtensions
	{
		private sealed class TypeTypeHandleComparer : IEqualityComparer<Type>
		{
			public bool Equals(Type x, Type y) =>
				x is null ? y is null : (object)y != null && x.TypeHandle.Equals(y.TypeHandle);

			public int GetHashCode(Type obj) => obj.TypeHandle.GetHashCode();
		}

		private sealed class MethodMethodHandleComparer : IEqualityComparer<MethodInfo>
		{
			public bool Equals(MethodInfo x, MethodInfo y) =>
				x is null ? y is null : (object)y != null && x.MethodHandle.Equals(y.MethodHandle);

			public int GetHashCode(MethodInfo obj) => obj.MethodHandle.GetHashCode();
		}

		// DONTTOUCH: Direct compare may result in false negative.
		// See http://stackoverflow.com/questions/27645408 for explanation.
		[NotNull]
		private static readonly IEqualityComparer<Type> _typeComparer = new TypeTypeHandleComparer();
		[NotNull]
		private static readonly IEqualityComparer<MethodInfo> _methodComparer = new MethodMethodHandleComparer();

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
		[CanBeNull]
		public static TAttribute TryGetMetadataAttribute<TAttribute>(
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
		[CanBeNull]
		public static TAttribute TryGetMetadataAttribute<TAttribute>(
			[NotNull] this ICustomAttributeProvider attributeProvider,
			bool thisLevelOnly)
			where TAttribute : class =>
				GetMetadataAttributes<TAttribute>(attributeProvider, thisLevelOnly).FirstOrDefault();

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

			switch (attributeProvider) {
				case Type type:
					return type.GetAttributesForType<TAttribute>(thisLevelOnly);
				case MemberInfo member:
					return member.GetAttributesForMember<TAttribute>(thisLevelOnly);
				default:
					return GetAttributesFromCandidates<TAttribute>(true, attributeProvider);
			}
		}

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
			var inheritanceTypes = Sequence.CreateWhileNotNull(type, t => t.BaseType)
				.ToArray();

			// ReSharper disable once CoVariantArrayConversion
			var attributes = GetAttributesFromCandidates<TAttribute>(false, inheritanceTypes);
			foreach (var attribute in attributes)
			{
				yield return attribute;
			}
		}

		[NotNull, ItemNotNull]
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
					t => t.BaseType)
					.ToArray();

				if (inheritanceTypes.Length == 0)
					continue;

				visited.AddRange(inheritanceTypes);

				// ReSharper disable once CoVariantArrayConversion
				var attributes = GetAttributesFromCandidates<TAttribute>(false, inheritanceTypes);
				foreach (var attribute in attributes)
				{
					yield return attribute;
				}
			}

			foreach (var attribute in GetAttributesFromCandidates<TAttribute>(true, type.Assembly))
			{
				yield return attribute;
			}
		}

		[NotNull, ItemNotNull]
		private static IEnumerable<TAttribute> GetAttributesForMember<TAttribute>([NotNull] this MemberInfo member, bool thisLevelOnly)
			where TAttribute : class
		{
			// ReSharper disable once CoVariantArrayConversion
			var attributes = GetAttributesFromCandidates<TAttribute>(false, member.GetOverrideChain());
			foreach (var attribute in attributes)
			{
				yield return attribute;
			}

			if (!thisLevelOnly)
			{
				foreach (var attribute in member.DeclaringType.GetAttributesForTypeWithNesting<TAttribute>())
				{
					yield return attribute;
				}
			}
		}

		#region Get override chain
		[NotNull, ItemNotNull]
		private static MemberInfo[] GetOverrideChain(this MemberInfo member) =>
			IsOverriden(member) ? _getOverrideChainCache(member) : new[] { member };

		private static bool IsOverriden(MemberInfo member)
		{
			switch (member) {
				case Type _:
					throw CodeExceptions.Argument(nameof(member), "Member should not be a type.");
				case MethodInfo method:
					return IsOverriden(method);
				case PropertyInfo property:
					return IsOverriden(property.GetAccessors(true)[0]);
				case EventInfo eventInfo:
					return IsOverriden(eventInfo.GetAddMethod(true));
				default:
					return false;
			}
		}

		private static bool IsOverriden([CanBeNull] MethodInfo method) =>
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
		private static MemberInfo[] GetOverrideChainDispatch(MemberInfo member)
		{
			// TODO: Use GetParentDefinition after https://github.com/dotnet/coreclr/issues/7135
			switch (member) {
				case MethodInfo method:
					return GetOverrideChainCore(method, m => m, t => t.GetMethods(_thisTypeMembers));
				case PropertyInfo property:
					return GetOverrideChainCore(property, p => p.GetAccessors(true)[0], t => t.GetProperties(_thisTypeMembers));
				case EventInfo eventInfo:
					return GetOverrideChainCore(eventInfo, e => e.GetAddMethod(true), t => t.GetEvents(_thisTypeMembers));
				default:
					return new[] { member };
			}
		}

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
			var typesToCheck = Sequence.CreateWhileNotNull(implMethod.DeclaringType, t => t.BaseType);
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
		private static readonly AttributeUsageAttribute _defaultUsage = new AttributeUsageAttribute(AttributeTargets.All);

		[NotNull]
		private static readonly Func<Type, AttributeUsageAttribute> _attributeUsages = Algorithms.Memoize(
			(Type t) => t.GetCustomAttribute<AttributeUsageAttribute>(true) ?? _defaultUsage,
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
					.GetCustomAttributes(typeof(TAttribute), inherit)
					.Cast<TAttribute>();
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