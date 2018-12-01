#if !LESSTHAN_NET40
using System;
using System.Linq.Expressions;

namespace CodeJam.Mapping
{
	internal class DefaultValueExpression : Expression
	{
		public DefaultValueExpression(MappingSchema mappingSchema, Type type)
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