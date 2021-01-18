#if NET40_OR_GREATER || TARGETS_NETSTANDARD || TARGETS_NETCOREAPP // PUBLIC_API_CHANGES. TODO: update after fixes in Theraot.Core
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;

namespace CodeJam.Mapping
{
	internal class ConvertInfo
	{
		public static readonly ConvertInfo Default = new();

		public class LambdaInfo
		{
			public LambdaInfo(
				LambdaExpression checkNullLambda,
				[AllowNull] LambdaExpression lambda,
				[AllowNull] Delegate @delegate,
				bool isSchemaSpecific)
			{
				CheckNullLambda = checkNullLambda;
				Lambda = lambda ?? checkNullLambda;
				Delegate = @delegate;
				IsSchemaSpecific = isSchemaSpecific;
			}

			public readonly LambdaExpression Lambda;
			public readonly LambdaExpression CheckNullLambda;

			[AllowNull]
			public readonly Delegate Delegate;

			public readonly bool IsSchemaSpecific;
		}

		private readonly ConcurrentDictionary<Type, ConcurrentDictionary<Type, LambdaInfo>> _expressions =
			new();

		public void Set(Type from, Type to, LambdaInfo expr) => Set(_expressions, from, to, expr);

		private static void Set(
			IDictionary<Type, ConcurrentDictionary<Type, LambdaInfo>> expressions,
			Type from,
			Type to,
			LambdaInfo expr)
		{
			if (!expressions.TryGetValue(from, out var dic))
				expressions[from] = dic = new ConcurrentDictionary<Type, LambdaInfo>();

			dic[to] = expr;
		}

		[return: MaybeNull]
		public LambdaInfo Get(Type from, Type to) =>
			_expressions.TryGetValue(from, out var dic) && dic.TryGetValue(to, out var li) ? li : null;

		public LambdaInfo Create([AllowNull] MappingSchema mappingSchema, Type from, Type to)
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