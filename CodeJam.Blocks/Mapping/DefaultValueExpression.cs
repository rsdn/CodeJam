#if LESSTHAN_NET40 || LESSTHAN_NETSTANDARD10 || LESSTHAN_NETCOREAPP10 // PUBLIC_API_CHANGES. TODO: update after fixes in Theraot.Core
// Some expression types are missing if targeting to these frameworks
#else
using System;
using System.Linq.Expressions;

using JetBrains.Annotations;

namespace CodeJam.Mapping
{
	internal class DefaultValueExpression : Expression
	{
		public DefaultValueExpression([CanBeNull] MappingSchema mappingSchema, [NotNull] Type type)
		{
			_mappingSchema = mappingSchema;
			Type          = type;
		}

		private readonly MappingSchema _mappingSchema;

		public override Type           Type { get; }
		public override ExpressionType NodeType => ExpressionType.Extension;
		public override bool           CanReduce => true;

		public override Expression Reduce()
			=> Constant(
				_mappingSchema == null ?
					DefaultValue.GetValue(Type) :
					_mappingSchema.GetDefaultValue(Type),
				Type);
	}
}
#endif