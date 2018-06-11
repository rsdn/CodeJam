using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using CodeJam.Collections;

using JetBrains.Annotations;

namespace CodeJam.Reflection
{
	public partial class ReflectionExtensions
	{
		private static bool IsOptional(this ParameterInfo prm) =>
#if LESSTHAN_NET45
			(prm.Attributes & ParameterAttributes.HasDefault) == ParameterAttributes.HasDefault
#else
			prm.HasDefaultValue
#endif
			;

		private static bool IsCtorSuitable(ConstructorInfo ctor, ParamInfo[] parameters)
		{
			var ctorPrms = ctor.GetParameters();
			var ctorMap = ctorPrms.ToDictionary(p => p.Name);
			foreach (var prm in parameters)
			{
				if (!prm.Required)
					continue;
				if (!ctorMap.ContainsKey(prm.Name))
					return false;
				if (prm.Value != null && !ctorMap[prm.Name].ParameterType.IsInstanceOfType(prm.Value))
					return false;
			}

			var argMap = parameters.Select(p => p.Name).ToHashSet();
			foreach (var prm in ctorPrms)
			{
				if (prm.IsOptional())
					continue;
				if (!argMap.Contains(prm.Name))
					return false;
			}

			return true;
		}

		/// <summary>
		/// Creates instance of <paramref name="type"/> with specified <paramref name="parameters"/>.
		/// </summary>
		/// <param name="type">Type to create instance.</param>
		/// <param name="parameters">Constructor parameters</param>
		/// <returns>Instance of type</returns>
		/// <exception cref="ArgumentNullException"><paramref name="type"/> is null</exception>
		/// <exception cref="ArgumentException">No suitable constructors found</exception>
		[NotNull]
		[Pure]
		public static object CreateInstance([NotNull] this Type type, params ParamInfo[] parameters)
		{
			if (type == null) throw new ArgumentNullException(nameof(type));
			var constructors = type.GetConstructors(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
			var ctor = constructors.FirstOrDefault(c => IsCtorSuitable(c, parameters));
			if (ctor == null)
				throw new ArgumentException("No suitable constructors found", nameof(type));
			var prmsMap = parameters.ToDictionary(p => p.Name, p => p.Value);
			var values =
				ctor
					.GetParameters()
					.Select(p => prmsMap.GetValueOrDefault(p.Name, k => p.DefaultValue))
					.ToArray();
			return ctor.Invoke(values);
		}
	}
}