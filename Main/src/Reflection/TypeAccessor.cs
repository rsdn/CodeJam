#if !FW35
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
		#region Protected Emit Helpers

		/// <summary>
		/// Adds <see cref="MemberAccessor"/>.
		/// </summary>
		/// <param name="member">Instance of <see cref="MemberAccessor"/>.</param>
		protected void AddMember([NotNull] MemberAccessor member)
		{
			if (member == null) throw new ArgumentNullException(nameof(member));

			Members.Add(member);

			_membersByName[member.MemberInfo.Name] = member;
		}

		#endregion

		#region CreateInstance

		/// <summary>
		/// Creates an instance of the accessed type.
		/// </summary>
		/// <returns>An instance of the accessed type.</returns>
		[DebuggerStepThrough]
		public virtual object CreateInstance() =>
			throw new InvalidOperationException($"The '{Type.Name}' type must have public default or init constructor.");
		#endregion

		#region Public Members

		/// <summary>
		/// Type to access.
		/// </summary>
		public abstract Type Type { get; }

		/// <summary>
		/// Type members.
		/// </summary>
		public List<MemberAccessor> Members { get; } = new List<MemberAccessor>();

		#endregion

		#region Items
		private readonly ConcurrentDictionary<string,MemberAccessor> _membersByName =
			new ConcurrentDictionary<string,MemberAccessor>();

		/// <summary>
		/// Returns <see cref="MemberAccessor"/> by its name.
		/// </summary>
		/// <param name="memberName">Member name.</param>
		/// <returns>Instance of <see cref="MemberAccessor"/>.</returns>
		public MemberAccessor this[string memberName] =>
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
		public MemberAccessor this[int index]
			=> Members[index];

		#endregion

		#region Static Members
		private static readonly ConcurrentDictionary<Type,TypeAccessor> _accessors =
			new ConcurrentDictionary<Type,TypeAccessor>();

		/// <summary>
		/// Creates an instance of <see cref="TypeAccessor"/>.
		/// </summary>
		/// <param name="type">Type to access.</param>
		/// <returns>Instance of <see cref="TypeAccessor"/>.</returns>
		public static TypeAccessor GetAccessor([NotNull] Type type)
		{
			if (type == null) throw new ArgumentNullException(nameof(type));

			TypeAccessor accessor;

			if (_accessors.TryGetValue(type, out accessor))
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
		public static TypeAccessor<T> GetAccessor<T>()
		{
			TypeAccessor accessor;

			if (_accessors.TryGetValue(typeof(T), out accessor))
				return (TypeAccessor<T>)accessor;

			return (TypeAccessor<T>)(_accessors[typeof(T)] = new TypeAccessor<T>());
		}

		#endregion
	}
}
#endif