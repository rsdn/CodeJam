#if !FW35
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace CodeJam.Metadata
{
	using Mapping;
	using Reflection;

	internal class AttributeInfo
	{
		public AttributeInfo(string name, Dictionary<string,object> values)
		{
			Name   = name;
			Values = values;
		}

		public readonly string                    Name;
		public readonly Dictionary<string,object> Values;

		private Func<Attribute> _func;

		public Attribute MakeAttribute(Type type)
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
								Values.Select(k =>
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