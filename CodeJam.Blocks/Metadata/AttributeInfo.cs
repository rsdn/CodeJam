#if LESSTHAN_NET40 || LESSTHAN_NETSTANDARD10 || LESSTHAN_NETCOREAPP10 // PUBLIC_API_CHANGES. TODO: update after fixes in Theraot.Core
// Some expression types are missing if targeting to these frameworks
#else
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

using JetBrains.Annotations;

namespace CodeJam.Metadata
{
	using Mapping;
	using Reflection;

	internal class AttributeInfo
	{
		public AttributeInfo([NotNull] string name, [NotNull] Dictionary<string,object> values)
		{
			Name    = name;
			_values = values;
		}

		[NotNull] public readonly string Name;

		[NotNull] private readonly Dictionary<string,object> _values;

		private Func<Attribute> _func;

		public Attribute MakeAttribute([NotNull] Type type)
		{
			if (_func == null)
			{
				var ctors = type.GetConstructors();
				var ctor  = ctors.FirstOrDefault(c => c.GetParameters().Length == 0);

				if (ctor != null)
				{
					var expr = Expression.Lambda<Func<Attribute>>(
						Expression.Convert(
							Expression.MemberInit(
								Expression.New(ctor),
								_values.Select(k =>
								{
									var member = type.GetMember(k.Key)[0];
									var mtype  = member.GetMemberType();

									return Expression.Bind(
										member,
										Expression.Constant(Converter.ChangeType(k.Value, mtype), mtype));
								})),
							typeof(Attribute)));

					_func = expr.Compile();
				}
				else
				{
					throw new NotImplementedException();
				}
			}

			return _func();
		}
	}
}
#endif