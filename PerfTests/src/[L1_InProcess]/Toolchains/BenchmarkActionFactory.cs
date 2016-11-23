using System;
using System.Reflection;
using System.Threading.Tasks;

using BenchmarkDotNet.Running;

// ReSharper disable once CheckNamespace

namespace BenchmarkDotNet.Toolchains
{
	/// <summary>Helper class that creates <see cref="BenchmarkAction"/> instances. </summary>
	public static partial class BenchmarkActionFactory
	{
		private static BenchmarkAction Create(Type actionType, object instance, MethodInfo method, int unrollFactor) =>
			// ReSharper disable once RedundantExplicitParamsArrayCreation
			(BenchmarkAction)Activator.CreateInstance(actionType, new[] { instance, method, unrollFactor });

		private static BenchmarkAction CreateCore(
			object instance,
			MethodInfo targetMethod,
			MethodInfo idleSignature,
			int unrollFactor)
		{
			var signature = targetMethod ?? idleSignature;
			if (signature == null)
				throw new ArgumentNullException(nameof(idleSignature), $"Either {nameof(targetMethod)} or  {nameof(idleSignature)} should be not null");

			var resutltType = signature.ReturnType;
			if (resutltType == typeof(void))
			{
				return new BenchmarkActionVoid(instance, targetMethod, unrollFactor);
			}
			if (resutltType == typeof(Task))
			{
				return new BenchmarkActionTask(instance, targetMethod, unrollFactor);
			}

			if (resutltType.IsGenericType)
			{
				var genericType = resutltType.GetGenericTypeDefinition();
				var argType = resutltType.GenericTypeArguments[0];
				if (typeof(Task<>) == genericType)
					return Create(typeof(BenchmarkActionTask<>).MakeGenericType(argType), instance, targetMethod, unrollFactor);

				if (typeof(ValueTask<>).IsAssignableFrom(genericType))
					return Create(typeof(BenchmarkActionValueTask<>).MakeGenericType(argType), instance, targetMethod, unrollFactor);
			}

			if (targetMethod == null && resutltType.IsValueType)
			{
				resutltType = typeof(int);
			}
			return Create(typeof(BenchmarkAction<>).MakeGenericType(resutltType), instance, targetMethod, unrollFactor);
		}

		private static void FallbackMethod() { }
		private static readonly MethodInfo _fallbackSignature = new Action(FallbackMethod).Method;

		/// <summary>Creates the run benchmark action.</summary>
		/// <param name="target">The target.</param>
		/// <param name="instance">The target instance.</param>
		/// <param name="unrollFactor">The unroll factor.</param>
		/// <returns>Run benchmark action</returns>
		public static BenchmarkAction CreateRun(Target target, object instance, int unrollFactor) =>
			CreateCore(instance, target.Method, null, unrollFactor);

		/// <summary>Creates the idle benchmark action.</summary>
		/// <param name="target">The target.</param>
		/// <param name="instance">The target instance.</param>
		/// <param name="unrollFactor">The unroll factor.</param>
		/// <returns>Idle benchmark action</returns>
		public static BenchmarkAction CreateIdle(Target target, object instance, int unrollFactor) =>
			CreateCore(instance, null, target.Method, unrollFactor);

		/// <summary>Creates the setup benchmark action.</summary>
		/// <param name="target">The target.</param>
		/// <param name="instance">The target instance.</param>
		/// <returns>Setup benchmark action</returns>
		public static BenchmarkAction CreateSetup(Target target, object instance) =>
			CreateCore(instance, target.SetupMethod, _fallbackSignature, 1);

		/// <summary>Creates the cleanup benchmark action.</summary>
		/// <param name="target">The target.</param>
		/// <param name="instance">The target instance.</param>
		/// <returns>Cleanup benchmark action</returns>
		public static BenchmarkAction CreateCleanup(Target target, object instance) =>
			CreateCore(instance, target.CleanupMethod, _fallbackSignature, 1);
	}
}