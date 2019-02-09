#if !LESSTHAN_NET40
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Reflection;
using System.Linq;

using CodeJam.Expressions;

using JetBrains.Annotations;

namespace CodeJam.Reflection
{
	/// <summary>
	/// Provides fast access to a type member.
	/// </summary>
	[DebuggerDisplay("Name = {Name}, Type = {Type}")]
	[PublicAPI]
	public class MemberAccessor
	{
		internal MemberAccessor([NotNull] TypeAccessor typeAccessor, [NotNull] string memberName)
		{
			TypeAccessor = typeAccessor;

			if (memberName.IndexOf('.') < 0)
			{
				SetSimple(Expression.PropertyOrField(Expression.Constant(null, typeAccessor.Type), memberName).Member);
			}
			else
			{
				IsComplex = true;
				HasGetter = true;
				HasSetter = true;

				var members  = memberName.Split('.');
				var objParam = Expression.Parameter(TypeAccessor.Type, "obj");
				var expr     = (Expression)objParam;
				var infos    = members.Select(m =>
				{
					expr = Expression.PropertyOrField(expr, m);
					return new
					{
						member = ((MemberExpression)expr).Member,
						type   = expr.Type
					};
				}).ToArray();

				var lastInfo = infos[infos.Length - 1];

				MemberInfo = lastInfo.member;
				Type       = lastInfo.type;

				var checkNull = infos.Take(infos.Length - 1).Any(info => info.type.GetIsClass() || info.type.IsNullable());

				// Build getter.
				//
				{
					if (checkNull)
					{
						var ret = Expression.Variable(Type, "ret");

						Expression MakeGetter(Expression ex, int i)
						{
							while (true)
							{
								var info = infos[i];
								var next = Expression.MakeMemberAccess(ex, info.member);

								if (i == infos.Length - 1)
									return Expression.Assign(ret, next);

								if (next.Type.IsClass || next.Type.IsNullable())
								{
									var local = Expression.Variable(next.Type);

									return Expression.Block(new[] { local }, Expression.Assign(local, next) as Expression, Expression.IfThen(Expression.NotEqual(local, Expression.Constant(null)), MakeGetter(local, i + 1)));
								}

								ex = next;
								i = i + 1;
							}
						}

						expr = Expression.Block(
							new[] { ret },
							Expression.Assign(ret, Expression.Constant(GetDefaultValue(Type), Type)),
							MakeGetter(objParam, 0),
							ret);
					}
					else
					{
						expr = objParam;
						foreach (var info in infos)
							expr = Expression.MakeMemberAccess(expr, info.member);
					}

					GetterExpression = Expression.Lambda(expr, objParam);
				}

				// Build setter.
				//
				{
					HasSetter = !infos.Any(info => info.member is PropertyInfo propertyInfo && propertyInfo.GetSetMethod(true) == null);

					var valueParam = Expression.Parameter(Type, "value");

					if (HasSetter)
					{
						if (checkNull)
						{
							var vars  = new List<ParameterExpression>();
							var exprs = new List<Expression>();

							void MakeSetter(Expression ex, int i)
							{
								while (true)
								{
									var info = infos[i];
									var next = Expression.MakeMemberAccess(ex, info.member);

									if (i == infos.Length - 1)
									{
										exprs.Add(Expression.Assign(next, valueParam));
									}
									else
									{
										if (next.Type.IsClass || next.Type.IsNullable())
										{
											var local = Expression.Variable(next.Type);

											vars.Add(local);

											exprs.Add(Expression.Assign(local, next));
											exprs.Add(Expression.IfThen(Expression.Equal(local, Expression.Constant(null)), Expression.Block(Expression.Assign(local, Expression.New(local.Type)), Expression.Assign(next, local))));

											ex = local;
											i = i + 1;
											continue;
										}
										ex = next;
										i = i + 1;
										continue;
									}
									break;
								}
							}

							MakeSetter(objParam, 0);

							expr = Expression.Block(vars, exprs);
						}
						else
						{
							expr = objParam;
							foreach (var info in infos)
								expr = Expression.MakeMemberAccess(expr, info.member);
							expr = Expression.Assign(expr, valueParam);
						}

						SetterExpression = Expression.Lambda(expr, objParam, valueParam);
					}
					else
					{
						var fakeParam = Expression.Parameter(typeof(int));

						SetterExpression = Expression.Lambda(
							Expression.Block(
								new[] { fakeParam }, Expression.Assign(fakeParam, Expression.Constant(0))),
							objParam,
							valueParam);
					}
				}
			}

			SetExpressions();
		}

		internal MemberAccessor([NotNull] TypeAccessor typeAccessor, [NotNull] MemberInfo memberInfo)
		{
			TypeAccessor = typeAccessor;

			SetSimple(memberInfo);
			SetExpressions();
		}

		private void SetSimple([NotNull] MemberInfo memberInfo)
		{
			MemberInfo = memberInfo;
			var propertyInfo = MemberInfo as PropertyInfo;
			Type = propertyInfo?.PropertyType ?? ((FieldInfo)MemberInfo).FieldType;

			propertyInfo = memberInfo as PropertyInfo;
			if (propertyInfo != null)
			{
				HasGetter = propertyInfo.GetGetMethod(true) != null;
				HasSetter = propertyInfo.GetSetMethod(true) != null;
			}
			else
			{
				HasGetter = true;
				HasSetter = !((FieldInfo)memberInfo).IsInitOnly;
			}

			var objParam   = Expression.Parameter(TypeAccessor.Type, "obj");
			var valueParam = Expression.Parameter(Type, "value");

			GetterExpression =
				HasGetter
					? Expression.Lambda(Expression.MakeMemberAccess(objParam, memberInfo), objParam)
					: Expression.Lambda(Expression.Constant(GetDefaultValue(Type), Type), objParam);

			if (HasSetter)
			{
				SetterExpression = Expression.Lambda(
					Expression.Assign(Expression.MakeMemberAccess(objParam, memberInfo), valueParam),
					objParam,
					valueParam);
			}
			else
			{
				var fakeParam = Expression.Parameter(typeof(int));

				SetterExpression = Expression.Lambda(
					Expression.Block(new[] { fakeParam }, Expression.Assign(fakeParam, Expression.Constant(0))),
					objParam,
					valueParam);
			}
		}

		private void SetExpressions()
		{
			var objParam   = Expression.Parameter(typeof(object), "obj");
			var getterExpr = GetterExpression.ReplaceParameters(Expression.Convert(objParam, TypeAccessor.Type));
			var getter     = Expression.Lambda<Func<object,object>>(Expression.Convert(getterExpr, typeof(object)), objParam);

			Getter = getter.Compile();

			var valueParam = Expression.Parameter(typeof(object), "value");
			var setterExpr = SetterExpression.ReplaceParameters(
				Expression.Convert(objParam,   TypeAccessor.Type),
				Expression.Convert(valueParam, Type));
			var setter = Expression.Lambda<Action<object,object>>(setterExpr, objParam, valueParam);

			Setter = setter.Compile();
		}

		private const FieldAttributes _enumField =
			FieldAttributes.Public | FieldAttributes.Static | FieldAttributes.Literal;

		[NotNull]
		private static readonly ConcurrentDictionary<Type,object> _defaultValues = new ConcurrentDictionary<Type,object>();

		private object GetDefaultValue([NotNull] Type type)
		{
			if (_defaultValues.TryGetValue(type, out var value))
				return value;

			if (!type.IsClass && !type.IsNullable())
			{
				var mi = InfoOf.Method(() => GetDefaultValue<int>());

				value =
					Expression.Lambda<Func<object>>(
						Expression.Convert(
							Expression.Call(mi.GetGenericMethodDefinition().MakeGenericMethod(type)),
							typeof(object)))
						.Compile()();
			}

			_defaultValues[type] = value;

			return value;
		}

		private static T GetDefaultValue<T>()
		{
			if (_defaultValues.TryGetValue(typeof(T), out var value))
				return (T)value;

			_defaultValues[typeof(T)] = default(T);

			return default;
		}

		#region Public Properties

		/// <summary>
		/// Member <see cref="MemberInfo"/>.
		/// </summary>
		[NotNull]
		public MemberInfo MemberInfo { get; private set; }

		/// <summary>
		/// Parent <see cref="TypeAccessor"/>.
		/// </summary>
		[NotNull]
		public TypeAccessor TypeAccessor { get; private set; }

		/// <summary>
		/// True, if the member has getter.
		/// </summary>
		public bool HasGetter { get; private set; }

		/// <summary>
		/// True, if the member has setter.
		/// </summary>
		public bool HasSetter { get; private set; }

		/// <summary>
		/// Member <see cref="Type"/>.
		/// </summary>
		[NotNull] public Type Type { get; private set; }

		/// <summary>
		/// True, if the member is complex.
		/// </summary>
		public bool IsComplex { get; private set; }

		/// <summary>
		/// Getter expression of the member.
		/// </summary>
		[NotNull] public LambdaExpression GetterExpression { get; private set; }

		/// <summary>
		/// Setter expression of the member.
		/// </summary>
		[NotNull] public LambdaExpression SetterExpression { get; private set; }

		/// <summary>
		/// Member getter function.
		/// </summary>
		[NotNull] public Func<object,object> Getter { get; private set; }

		/// <summary>
		/// Member setter action.
		/// </summary>
		[NotNull] public Action<object,object> Setter { get; private set; }

		/// <summary>
		/// Member name.
		/// </summary>
		public string Name
			=> MemberInfo.Name;

		/// <summary>
		/// Gets member value for provided object.
		/// </summary>
		/// <param name="o">Object to access.</param>
		/// <returns>Member value.</returns>
		public object GetValue(object o)
			=> Getter(o);

		/// <summary>
		/// Sets member value for provided object.
		/// </summary>
		/// <param name="o">Object to access.</param>
		/// <param name="value">Value to set.</param>
		public void SetValue(object o, object value)
			=> Setter(o, value);

		#endregion
	}
}
#endif