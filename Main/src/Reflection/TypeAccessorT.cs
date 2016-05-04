using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace CodeJam.Reflection
{
	/// <summary>
	/// Provides fast access to type and its members.
	/// </summary>
	public class TypeAccessor<T> : TypeAccessor
	{
		static TypeAccessor()
		{
			// Create Instance.
			//
			var type = typeof(T);

			if (type.IsValueType)
			{
				_createInstance = () => default(T);
			}
			else
			{
				var ctor = type.IsAbstract ? null : type.GetDefaultConstructor();

				if (ctor == null)
				{
					Expression<Func<T>> mi;

					if (type.IsAbstract) mi = () => ThrowAbstractException();
					else                 mi = () => ThrowException();

					var body = Expression.Call(null, ((MethodCallExpression)mi.Body).Method);

					_createInstance = Expression.Lambda<Func<T>>(body).Compile();
				}
				else
				{
					_createInstance = Expression.Lambda<Func<T>>(Expression.New(ctor)).Compile();
				}
			}

			foreach (var memberInfo in type.GetMembers(BindingFlags.Instance | BindingFlags.Public))
			{
				if (memberInfo.MemberType == MemberTypes.Field || 
					memberInfo.MemberType == MemberTypes.Property && ((PropertyInfo)memberInfo).GetIndexParameters().Length == 0)
				{
					_members.Add(memberInfo);
				}
			}

			// Add explicit iterface implementation properties support
			// Or maybe we should support all private fields/properties?
			//
			var interfaceMethods = type.GetInterfaces().SelectMany(ti => type.GetInterfaceMap(ti).TargetMethods).ToList();

			if (interfaceMethods.Count > 0)
			{
				foreach (var pi in type.GetProperties(BindingFlags.NonPublic | BindingFlags.Instance))
				{
					if (pi.GetIndexParameters().Length == 0)
					{
						var getMethod = pi.GetGetMethod(true);
						var setMethod = pi.GetSetMethod(true);

						if ((getMethod == null || interfaceMethods.Contains(getMethod)) &&
							(setMethod == null || interfaceMethods.Contains(setMethod)))
						{
							_members.Add(pi);
						}
					}
				}
			}
		}

		private static T ThrowException()
		{
			throw new InvalidOperationException($"The '{typeof(T).FullName}' type must have default or init constructor.");
		}

		private static T ThrowAbstractException()
		{
			throw new InvalidOperationException($"Cant create an instance of abstract class '{typeof(T).FullName}'.");
		}

		// ReSharper disable once StaticMemberInGenericType
		private static readonly List<MemberInfo> _members = new List<MemberInfo>();

		internal TypeAccessor()
		{
			foreach (var member in _members)
				AddMember(new MemberAccessor(this, member));
		}

		private static readonly Func<T> _createInstance;

		/// <summary>
		/// Creates an instance of <see cref="TypeAccessor"/>.
		/// </summary>
		/// <returns>Instance of <see cref="TypeAccessor"/>.</returns>
		public override object CreateInstance() => _createInstance();

		/// <summary>
		/// Creates an instance of <see cref="TypeAccessor"/>.
		/// </summary>
		/// <returns>Instance of <see cref="TypeAccessor"/>.</returns>
		public T Create() => _createInstance();

		/// <summary>
		/// Type to access.
		/// </summary>
		public override Type Type => typeof(T);
	}
}
