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
		[JetBrains.Annotations.NotNull]
		public static readonly ConvertInfo Default = new();

		public class LambdaInfo
		{
			public LambdaInfo(
				[JetBrains.Annotations.NotNull] LambdaExpression checkNullLambda,
				[AllowNull] LambdaExpression lambda,
				[AllowNull] Delegate @delegate,
				bool isSchemaSpecific)
			{
				CheckNullLambda = checkNullLambda;
				Lambda = lambda ?? checkNullLambda;
				Delegate = @delegate;
				IsSchemaSpecific = isSchemaSpecific;
			}

			[JetBrains.Annotations.NotNull] public readonly LambdaExpression Lambda;
			[JetBrains.Annotations.NotNull] public readonly LambdaExpression CheckNullLambda;
			[AllowNull] public readonly Delegate Delegate;
			public readonly bool IsSchemaSpecific;
		}

		[JetBrains.Annotations.NotNull]
		private readonly ConcurrentDictionary<Type, ConcurrentDictionary<Type, LambdaInfo>> _expressions =
			new();

		public void Set([JetBrains.Annotations.NotNull] Type from, [JetBrains.Annotations.NotNull] Type to, [JetBrains.Annotations.NotNull] LambdaInfo expr) => Set(_expressions, from, to, expr);

		private static void Set(
			[JetBrains.Annotations.NotNull] IDictionary<Type, ConcurrentDictionary<Type, LambdaInfo>> expressions,
			[JetBrains.Annotations.NotNull] Type from,
			[JetBrains.Annotations.NotNull] Type to,
			[JetBrains.Annotations.NotNull] LambdaInfo expr)
		{
			if (!expressions.TryGetValue(from, out var dic))
				expressions[from] = dic = new ConcurrentDictionary<Type, LambdaInfo>();

			dic[to] = expr;
		}

		[return: MaybeNull]
		public LambdaInfo Get([JetBrains.Annotations.NotNull] Type from, [JetBrains.Annotations.NotNull] Type to) =>
			_expressions.TryGetValue(from, out var dic) && dic.TryGetValue(to, out var li) ? li : null;

		[JetBrains.Annotations.NotNull]
		public LambdaInfo Create([AllowNull] MappingSchema mappingSchema, [JetBrains.Annotations.NotNull] Type from, [JetBrains.Annotations.NotNull] Type to)
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