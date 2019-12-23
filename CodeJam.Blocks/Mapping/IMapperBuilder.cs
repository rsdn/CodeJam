#if NET40_OR_GREATER || TARGETS_NETSTANDARD || TARGETS_NETCOREAPP // PUBLIC_API_CHANGES. TODO: update after fixes in Theraot.Core
using System;
using System.Collections.Generic;
using System.Linq.Expressions;

using CodeJam.Reflection;

using JetBrains.Annotations;

namespace CodeJam.Mapping
{
	/// <summary>
	/// Builds a mapper that maps an object of <i>TFrom</i> type to an object of <i>TTo</i> type.
	/// </summary>
	[PublicAPI]
	public interface IMapperBuilder
	{
		/// <summary>
		/// Mapping schema.
		/// </summary>
		[NotNull]
		MappingSchema MappingSchema { get; set; }

		/// <summary>
		/// Returns a mapper expression to map an object of <i>TFrom</i> type to an object of <i>TTo</i> type.
		/// Returned expression is compatible to IQueriable.
		/// </summary>
		/// <returns>Mapping expression.</returns>
		[Pure]
		LambdaExpression GetMapperLambdaExpressionEx();

		/// <summary>
		/// Returns a mapper expression to map an object of <i>TFrom</i> type to an object of <i>TTo</i> type.
		/// </summary>
		/// <returns>Mapping expression.</returns>
		[Pure]
		LambdaExpression GetMapperLambdaExpression();

		/// <summary>
		/// Filters target members to map.
		/// </summary>
		Func<MemberAccessor,bool> MemberFilter { get; set; }

		/// <summary>
		/// Defines member name mapping for source types.
		/// </summary>
		Dictionary<Type,Dictionary<string,string>> FromMappingDictionary { get; set; }

		/// <summary>
		/// Defines member name mapping for destination types.
		/// </summary>
		Dictionary<Type,Dictionary<string,string>> ToMappingDictionary { get; set; }

		/// <summary>
		/// Member mappers.
		/// </summary>
		List<MemberMapper> MemberMappers { get; set; }

		/// <summary>
		/// If true, processes object cross references.
		/// if default (null), the <see cref="GetMapperLambdaExpressionEx"/> method does not process cross references,
		/// however the <see cref="GetMapperLambdaExpression"/> method does.
		/// </summary>
		bool? ProcessCrossReferences { get; set; }

		/// <summary>
		/// If true, performs deep copy.
		/// if default (null), the <see cref="GetMapperLambdaExpressionEx"/> method does not do deep copy,
		/// however the <see cref="GetMapperLambdaExpression"/> method does.
		/// </summary>
		bool? DeepCopy { get; set; }

		/// <summary>
		/// Type to map from.
		/// </summary>
		[NotNull] Type FromType { get; }

		/// <summary>
		/// Type to map to.
		/// </summary>
		[NotNull] Type ToType { get; }
	}

	/// <summary>Member Mapper</summary>
	public struct MemberMapper
	{
		/// <summary>Constructor</summary>
		/// <param name="from">From.</param>
		/// <param name="to">To.</param>
		public MemberMapper(LambdaExpression from, LambdaExpression to)
		{
			From = from;
			To = to;
		}

		/// <summary>From</summary>
		public readonly LambdaExpression From;

		/// <summary>To</summary>
		public readonly LambdaExpression To;
    }
}
#endif
