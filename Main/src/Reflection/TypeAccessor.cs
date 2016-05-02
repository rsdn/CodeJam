using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;

using JetBrains.Annotations;

namespace CodeJam.Reflection
{
	/// <summary>
	/// 
	/// </summary>
	[DebuggerDisplay("Type = {Type}")]
	[PublicAPI]
	public abstract class TypeAccessor
	{
		#region Protected Emit Helpers

		protected void AddMember([NotNull] MemberAccessor member)
		{
			if (member == null) throw new ArgumentNullException(nameof(member));

			Members.Add(member);

			_membersByName[member.MemberInfo.Name] = member;
		}

		#endregion

		#region CreateInstance

		[DebuggerStepThrough]
		public virtual object CreateInstance()
		{
			throw new InvalidOperationException($"The '{Type.Name}' type must have public default or init constructor.");
		}

		#endregion

		#region Public Members

		public abstract Type Type { get; }
		public List<MemberAccessor> Members { get; } = new List<MemberAccessor>();

		#endregion

		#region Items

		readonly ConcurrentDictionary<string,MemberAccessor> _membersByName = new ConcurrentDictionary<string,MemberAccessor>();

		public MemberAccessor this[string memberName] =>
			_membersByName.GetOrAdd(memberName, name =>
			{
				var ma = new MemberAccessor(this, name);
				Members.Add(ma);
				return ma;
			});

		public MemberAccessor this[int index]
			=> Members[index];

		#endregion

		#region Static Members

		static readonly ConcurrentDictionary<Type,TypeAccessor> _accessors = new ConcurrentDictionary<Type,TypeAccessor>();

		public static TypeAccessor GetAccessor([NotNull] Type type)
		{
			if (type == null) throw new ArgumentNullException(nameof(type));

			TypeAccessor accessor;

			if (_accessors.TryGetValue(type, out accessor))
				return accessor;

			var accessorType = typeof(TypeAccessor<>).MakeGenericType(type);

			accessor = (TypeAccessor)Activator.CreateInstance(accessorType);

			_accessors[type] = accessor;

			return accessor;
		}

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
