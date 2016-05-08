using System;
using System.Linq.Expressions;

using JetBrains.Annotations;

namespace CodeJam.Mapping
{
	[PublicAPI]
	public static class ConvertTo<TTo>
	{
		public static TTo From<TFrom>(TFrom o)
		{
			return Convert<TFrom,TTo>.From(o);
		}
	}
}
