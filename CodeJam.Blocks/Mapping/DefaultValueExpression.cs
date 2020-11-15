#if NET40_OR_GREATER || TARGETS_NETSTANDARD || TARGETS_NETCOREAPP // PUBLIC_API_CHANGES. TODO: update after fixes in Theraot.Core
using System;
using System.Linq.Expressions;

using JetBrains.Annotations;

namespace CodeJam.Mapping
{
	internal class DefaultValueExpression : Expression
	{
		public DefaultValueExpression([CanBeNull] MappingSchema? mappingSchema, [NotNull] Type type)
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