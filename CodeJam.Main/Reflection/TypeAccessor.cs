#if NET40_OR_GREATER || TARGETS_NETSTANDARD || TARGETS_NETCOREAPP // PUBLIC_API_CHANGES. TODO: update after fixes in Theraot.Core
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;

using JetBrains.Annotations;

namespace CodeJam.Reflection
{
	/// <summary>
	/// Provides fast access to type and its members.
	/// </summary>
	[DebuggerDisplay("Type = {" + nameof(Type) + "}")]
	[PublicAPI]
	public abstract class TypeAccessor
	{
		#region Protected emit helpers

		/// <summary>
		/// Adds <see cref="MemberAccessor"/>.
		/// </summary>
		/// <param name="member">Instance of <see cref="MemberAccessor"/>.</param>
		protected void AddMember([NotNull] MemberAccessor member)
		{
			Code.NotNull(member, nameof(member));

			Members.Add(member);

			_membersByName[member.MemberInfo.Name] = member;
		}

		#endregion

		#region CreateInstance

		/// <summary>
		/// Creates an instance of the accessed type.
		/// </summary>
		/// <returns>An instance of the accessed type.</returns>
		[NotNull, Pure]
		[DebuggerStepThrough]
		public virtual object CreateInstance() =>
			throw new InvalidOperationException($"The '{Type.Name}' type must have public default or init constructor.");
		#endregion

		#region Public Members

		/// <summary>
		/// Type to access.
		/// </summary>
		[NotNull]
		public abstract Type Type { get; }

		/// <summary>
		/// Type members.
		/// </summary>
		[NotNull, ItemNotNull]
		public List<MemberAccessor> Members { get; } = new List<MemberAccessor>();

		#endregion

		#region Items
		[NotNull]
		private readonly ConcurrentDictionary<string, MemberAccessor> _membersByName =
			new ConcurrentDictionary<string, MemberAccessor>();

		/// <summary>
		/// Returns <see cref="MemberAccessor"/> by its name.
		/// </summary>
		/// <param name="memberName">Member name.</param>
		/// <returns>Instance of <see cref="MemberAccessor"/>.</returns>
		[NotNull]
		public MemberAccessor this[[NotNull] string memberName] =>
			_membersByName.GetOrAdd(memberName, name =>
			{
				var ma = new MemberAccessor(this, name);
				Members.Add(ma);
				return ma;
			});

		/// <summary>
		/// Returns <see cref="MemberAccessor"/> by index.
		/// </summary>
		/// <param name="index">Member index.</param>
		/// <returns>Instance of <see cref="MemberAccessor"/>.</returns>
		[NotNull]
		public MemberAccessor this[int index] => Members[index];

		#endregion

		#region Static Members
		[NotNull]
		private static readonly ConcurrentDictionary<Type, TypeAccessor> _accessors =
			new ConcurrentDictionary<Type, TypeAccessor>();

		/// <summary>
		/// Creates an instance of <see cref="TypeAccessor"/>.
		/// </summary>
		/// <param name="type">Type to access.</param>
		/// <returns>Instance of <see cref="TypeAccessor"/>.</returns>
		[NotNull, Pure]
		public static TypeAccessor GetAccessor([NotNull] Type type)
		{
			Code.NotNull(type, nameof(type));
			if (_accessors.TryGetValue(type, out var accessor))
				return accessor;

			var accessorType = typeof(TypeAccessor<>).MakeGenericType(type);

			accessor = (TypeAccessor)Activator.CreateInstance(accessorType, true);

			_accessors[type] = accessor;

			return accessor;
		}

		/// <summary>
		/// Creates an instance of <see cref="TypeAccessor"/>.
		/// </summary>
		/// <typeparam name="T">Type to access.</typeparam>
		/// <returns>Instance of <see cref="TypeAccessor"/>.</returns>
		[NotNull, Pure]
		public static TypeAccessor<T> GetAccessor<T>()
		{
			if (_accessors.TryGetValue(typeof(T), out var accessor))
				return (TypeAccessor<T>)accessor;

			return (TypeAccessor<T>)(_accessors[typeof(T)] = new TypeAccessor<T>());
		}

		#endregion
	}
}
#endif