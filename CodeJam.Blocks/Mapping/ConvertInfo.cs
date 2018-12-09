#if !LESSTHAN_NET40
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq.Expressions;

using JetBrains.Annotations;

namespace CodeJam.Mapping
{
	internal class ConvertInfo
	{
		[NotNull]
		public static readonly ConvertInfo Default = new ConvertInfo();

		public class LambdaInfo
		{
			public LambdaInfo(
				[NotNull] LambdaExpression checkNullLambda,
				[CanBeNull] LambdaExpression lambda,
				[NotNull] Delegate @delegate,
				bool isSchemaSpecific)
			{
				CheckNullLambda = checkNullLambda;
				Lambda = lambda ?? checkNullLambda;
				Delegate = @delegate;
				IsSchemaSpecific = isSchemaSpecific;
			}

			[NotNull] public readonly LambdaExpression Lambda;
			[NotNull] public readonly LambdaExpression CheckNullLambda;
			[NotNull] public readonly Delegate Delegate;
			public readonly bool IsSchemaSpecific;
		}

		[NotNull]
		private readonly ConcurrentDictionary<Type, ConcurrentDictionary<Type, LambdaInfo>> _expressions =
			new ConcurrentDictionary<Type, ConcurrentDictionary<Type, LambdaInfo>>();

		public void Set([NotNull] Type from, [NotNull] Type to, [NotNull] LambdaInfo expr) => Set(_expressions, from, to, expr);

		private static void Set(
			[NotNull] IDictionary<Type, ConcurrentDictionary<Type, LambdaInfo>> expressions,
			[NotNull] Type from,
			[NotNull] Type to,
			[NotNull] LambdaInfo expr)
		{
			if (!expressions.TryGetValue(from, out var dic))
				expressions[from] = dic = new ConcurrentDictionary<Type, LambdaInfo>();

			dic[to] = expr;
		}

		[CanBeNull]
		public LambdaInfo Get([NotNull] Type from, [NotNull] Type to) =>
			_expressions.TryGetValue(from, out var dic) && dic.TryGetValue(to, out var li) ? li : null;

		[NotNull]
		public LambdaInfo Create([NotNull] MappingSchema mappingSchema, [NotNull] Type from, [NotNull] Type to)
		{
			var ex = ConvertBuilder.GetConverter(mappingSchema, from, to);
			var lm = ex.Item1.Compile();
			var ret = new LambdaInfo(ex.Item1, ex.Item2, lm, ex.Item3);

			Set(_expressions, from, to, ret);

			return ret;
		}
	}
}
#endif