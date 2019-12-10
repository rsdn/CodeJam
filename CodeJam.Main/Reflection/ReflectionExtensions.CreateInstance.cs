using System;
using System.Linq;
using System.Reflection;

#if (LESSTHAN_NET472 && !LESSTHAN_NET46) || (LESSTHAN_NETSTANDARD21 && !LESSTHAN_NETSTANDARD16) || LESSTHAN_NETCOREAPP10
using CodeJam.Collections.Backported;
#endif

using JetBrains.Annotations;

namespace CodeJam.Reflection
{
	public partial class ReflectionExtensions
	{
		private static bool GetHasDefaultValue([NotNull] this ParameterInfo prm) =>
#if LESSTHAN_NET45 || LESSTHAN_NETSTANDARD10 || LESSTHAN_NETCOREAPP10
			(prm.Attributes & ParameterAttributes.HasDefault) == ParameterAttributes.HasDefault;
#else
			prm.HasDefaultValue;
#endif

		private static bool IsCtorSuitable([NotNull] ConstructorInfo ctor, [NotNull, ItemNotNull] ParamInfo[] parameters)
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
				if (prm.GetHasDefaultValue())
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
		public static object CreateInstance([NotNull] this Type type, [NotNull, ItemNotNull] params ParamInfo[] parameters)
		{
			Code.NotNull(type, nameof(type));

			var constructors = type.GetConstructors(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
			var ctor = constructors.FirstOrDefault(c => IsCtorSuitable(c, parameters));

			if (ctor == null)
				throw new ArgumentException("No suitable constructors found", nameof(type));

			var prmsMap = parameters.ToDictionary(p => p.Name, p => p.Value);
			var values =
				ctor
					.GetParameters()
					.Select(p => prmsMap.TryGetValue(p.Name, out var result) ? result : p.DefaultValue)
					.ToArray();
			// ReSharper disable once AssignNullToNotNullAttribute
			return ctor.Invoke(values);
		}
	}
}