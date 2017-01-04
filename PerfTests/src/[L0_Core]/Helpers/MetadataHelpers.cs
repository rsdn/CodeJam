using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using CodeJam;
using CodeJam.Collections;

using JetBrains.Annotations;

// ReSharper disable once CheckNamespace

namespace BenchmarkDotNet.Helpers
{
	/// <summary>
	/// Helper methods for metadata attributes.
	/// </summary>
	[PublicAPI]
	public static class MetadataHelpers
	{
		// DONTOUCH: Direct compare will include ReflectedType comparison.
		private static readonly IEqualityComparer<Type> _typeComparer = ComparerBuilder<Type>.GetEqualityComparer(t => t.TypeHandle);
		private static readonly IEqualityComparer<MethodInfo> _methodComparer = ComparerBuilder<MethodInfo>.GetEqualityComparer(m => m.MethodHandle);

		#region Reflection
		/// <summary>
		/// Performs search for metadata attribute in the following order:
		/// * member attributes (if the <paramref name="attributeProvider"/> is member of the type)
		/// * type attributes (if the <paramref name="attributeProvider"/> is type or member of the type)
		/// * container type attributes (if the type is nested type)
		/// * assembly attributes.
		/// </summary>
		/// <typeparam name="TAttribute">Type of the attribute or type of interface implemented by the attribute.</typeparam>
		/// <param name="attributeProvider">Metadata attribute source.</param>
		/// <returns>Metadata attributes.</returns>
		public static TAttribute TryGetMetadataAttribute<TAttribute>(
			[NotNull] this ICustomAttributeProvider attributeProvider)
			where TAttribute : class =>
				GetMetadataAttributes<TAttribute>(attributeProvider).FirstOrDefault();

		/// <summary>
		/// Returns metadata attributes in the following order:
		/// * member attributes (if the <paramref name="attributeProvider"/> is member of the type)
		/// * type attributes (if the <paramref name="attributeProvider"/> is type or member of the type)
		/// * container type attributes (if the type is nested type)
		/// * assembly attributes.
		/// </summary>
		/// <typeparam name="TAttribute">Type of the attribute or type of interface implemented by the attribute.</typeparam>
		/// <param name="attributeProvider">Metadata attribute source.</param>
		/// <returns>Metadata attributes.</returns>
		public static IEnumerable<TAttribute> GetMetadataAttributes<TAttribute>(
			[NotNull] this ICustomAttributeProvider attributeProvider)
			where TAttribute : class
		{
			if (attributeProvider == null)
				throw new ArgumentNullException(nameof(attributeProvider));

			if (attributeProvider == null)
				throw new ArgumentNullException(nameof(attributeProvider));

			if (attributeProvider is Type type)
				return type.GetAttributesForType<TAttribute>();

			if (attributeProvider is MemberInfo member)
				return member.GetAttributesForMember<TAttribute>();

			return GetAttributesFromCandidates<TAttribute>(attributeProvider);
		}

		private static IEnumerable<TAttribute> GetAttributesForType<TAttribute>(
			this Type type)
			where TAttribute : class
		{
			var visited = new HashSet<ICustomAttributeProvider>();
			var typesToCheck = Sequence.CreateWhileNotNull(type, t => t.DeclaringType);
			foreach (var typeToCheck in typesToCheck)
			{
				var inheritanceTypes = Sequence.Create(
						typeToCheck,
						t => t != null && !visited.Contains(t),
						t => t.BaseType)
					.ToArray<ICustomAttributeProvider>();

				if (inheritanceTypes.Length == 0)
					continue;

				visited.AddRange(inheritanceTypes);

				foreach (var attribute in GetAttributesFromCandidates<TAttribute>(inheritanceTypes))
				{
					yield return attribute;
				}
			}

			foreach (var attribute in GetAttributesFromCandidates<TAttribute>(type.Assembly))
			{
				yield return attribute;
			}
		}

		private static IEnumerable<TAttribute> GetAttributesForMember<TAttribute>(
			this MemberInfo member)
			where TAttribute : class
		{
			// ReSharper disable once CoVariantArrayConversion
			var attributes = GetAttributesFromCandidates<TAttribute>(member.GetOverrideChain());

			foreach (var attribute in attributes)
			{
				yield return attribute;
			}

			foreach (var attribute in member.DeclaringType.GetAttributesForType<TAttribute>())
			{
				yield return attribute;
			}
		}

		#region Get override chain
		private static MemberInfo[] GetOverrideChain(this MemberInfo member)
		{
			if (member is Type)
				throw new ArgumentException("Member should not be a type", nameof(member));

			if (member.DeclaringType?.BaseType == null || member.DeclaringType.IsValueType)
				return new[] { member };

			return _getOverrideChainCache(member);
		}

		private static Func<MemberInfo, MemberInfo[]> _getOverrideChainCache = Algorithms.Memoize(
			(MemberInfo m) => GetOverrideChainDispatch(m), true);

		private static MemberInfo[] GetOverrideChainDispatch(MemberInfo member)
		{
			// TODO: Use GetParentDefinition after https://github.com/dotnet/coreclr/issues/7135
			if (member is MethodInfo method)
				return GetOverrideChainCore(method, m => m, t => t.GetRuntimeMethods());
			if (member is PropertyInfo property)
				return GetOverrideChainCore(property, p => p.GetAccessors(true)[0], t => t.GetRuntimeProperties());
			if (member is EventInfo eventInfo)
				return GetOverrideChainCore(eventInfo, e => e.GetAddMethod(true), t => t.GetRuntimeEvents());

			return new[] { member };
		}

		private static MemberInfo[] GetOverrideChainCore<TMember>(
			TMember property, Func<TMember, MethodInfo> accessorGetter, Func<Type, IEnumerable<TMember>> membersGetter)
			where TMember : MemberInfo
		{
			var propertyMethod = accessorGetter(property);
			if (propertyMethod.IsStatic || !propertyMethod.IsVirtual || propertyMethod.DeclaringType?.BaseType == null)
				return new MemberInfo[] { property };

			var result = new List<MemberInfo>();
			var baseDefinition = propertyMethod.GetBaseDefinition();
			var typesToCheck = Sequence.CreateWhileNotNull(propertyMethod.DeclaringType, t => t.BaseType);
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
		private static readonly Func<Type, AttributeUsageAttribute> _attributeUsages = Algorithms.Memoize(
			(Type t) => t.GetTypeInfo().GetCustomAttribute<AttributeUsageAttribute>(true) ?? _defaultUsage);

		private static TAttribute[] GetAttributesFromCandidates<TAttribute>(
			params ICustomAttributeProvider[] traversePath)
			where TAttribute : class
		{
			var result = new List<TAttribute>();
			var visitedAttributes = new HashSet<Type>();
			var checkInherited = false;
			foreach (var attributeProvider in traversePath)
			{
				var attributeCandidates = attributeProvider
					.GetCustomAttributes(typeof(TAttribute), false)
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
		#endregion
	}
}