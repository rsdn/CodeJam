#if !SUPPORTS_NET35
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace CodeJam.Mapping
{
	internal class ConvertInfo
	{
		public static readonly ConvertInfo Default = new ConvertInfo();

		public class LambdaInfo
		{
			public LambdaInfo(
				LambdaExpression checkNullLambda,
				LambdaExpression lambda,
				Delegate @delegate,
				bool isSchemaSpecific)
			{
				CheckNullLambda = checkNullLambda;
				Lambda = lambda ?? checkNullLambda;
				Delegate = @delegate;
				IsSchemaSpecific = isSchemaSpecific;
			}

			public readonly LambdaExpression Lambda;
			public readonly LambdaExpression CheckNullLambda;
			public readonly Delegate Delegate;
			public readonly bool IsSchemaSpecific;
		}

		private readonly ConcurrentDictionary<Type, ConcurrentDictionary<Type, LambdaInfo>> _expressions =
			new ConcurrentDictionary<Type, ConcurrentDictionary<Type, LambdaInfo>>();

		public void Set(Type from, Type to, LambdaInfo expr) => Set(_expressions, from, to, expr);

		private static void Set(
			IDictionary<Type, ConcurrentDictionary<Type, LambdaInfo>> expressions,
			Type from,
			Type to,
			LambdaInfo expr)
		{
			ConcurrentDictionary<Type, LambdaInfo> dic;

			if (!expressions.TryGetValue(from, out dic))
				expressions[from] = dic = new ConcurrentDictionary<Type, LambdaInfo>();

			dic[to] = expr;
		}

		public LambdaInfo Get(Type from, Type to)
		{
			ConcurrentDictionary<Type, LambdaInfo> dic;
			LambdaInfo li;

			return _expressions.TryGetValue(from, out dic) && dic.TryGetValue(to, out li) ? li : null;
		}

		public LambdaInfo Create(MappingSchema mappingSchema, Type from, Type to)
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