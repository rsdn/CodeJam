using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

using CodeJam.Strings;
using CodeJam.Targeting;

using JetBrains.Annotations;

namespace CodeJam
{
	public static class TestTools
	{
		public static Random GetTestRandom([CallerMemberName] string memberName = "") =>
			GetTestRandom(new Random().Next(), memberName);

				[MethodImpl(MethodImplOptions.NoInlining)]
		public static Random GetTestRandom(int seed, [CallerMemberName] string? memberName = "")
		{
#if TARGETS_NET || NETCOREAPP20_OR_GREATER
			// ReSharper disable RedundantSuppressNullableWarningExpression
			if (memberName.IsNullOrEmpty())
				memberName = new StackTrace(1).GetFrame(0)!.GetMethod()!.Name;
			// ReSharper restore RedundantSuppressNullableWarningExpression
#endif
			Console.WriteLine(
				$"{memberName}: Rnd seed: {seed} (use the seed to reproduce test results).");

			return new Random(seed);
		}

		[LinqTunnel]
		public static IEnumerable<T> Shuffle<T>(this IEnumerable<T> source, Random rnd) =>
			source.OrderBy(i => rnd.Next());

		[LinqTunnel]
		public static IEnumerable<Holder<T>> Wrap<T>(this IEnumerable<T> source) =>
			source.Select(i => new Holder<T>(i));

		public static void PrintQuirks()
		{
			var assembly = typeof(int).GetAssembly();

			Console.WriteLine($"{PlatformHelper.TargetPlatform}. Running on {assembly}");
			Console.WriteLine();
			PrintProps("System.Runtime.Versioning.BinaryCompatibility");
			Console.WriteLine();
			PrintProps("System.CompatibilitySwitches");
			Console.WriteLine();
			PrintProps("System.AppContextSwitches");
		}

		private static void PrintProps(string typeName)
		{
			var type = typeof(int).GetAssembly().GetType(typeName);
			if (type == null)
			{
				Console.WriteLine($"No type {typeName} found.");
				return;
			}

			Console.WriteLine(type.Name);
			var bf = BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;
			foreach (var prop in type.GetProperties(bf))
			{
				Console.WriteLine($"\t * {prop.Name}: {prop.GetValue(null, null)}");
			}
		}

		public static void WaitForResult(this Task source)
		{
			Code.NotNull(source, nameof(source));

#if NET45_OR_GREATER || TARGETS_NETCOREAPP
			source.GetAwaiter().GetResult();
#else
			// Workaround for Theraot cancellation logic
			try
			{
				source.GetAwaiter().GetResult();
			}
			catch (TaskCanceledException ex)
			{
				throw new OperationCanceledException(ex.Message, ex);
			}
#endif
		}

		public static T WaitForResult<T>(this Task<T> source)
		{
			Code.NotNull(source, nameof(source));

#if NET45_OR_GREATER || TARGETS_NETCOREAPP
			return source.GetAwaiter().GetResult();
#else
			// Workaround for Theraot cancellation logic
			try
			{
				return source.GetAwaiter().GetResult();
			}
			catch (TaskCanceledException ex)
			{
				throw new OperationCanceledException(ex.Message, ex);
			}
#endif
		}
	}

	public class Holder<T>
	{
		public Holder(T item)
		{
			Value = item;
		}

		public T Value { get; }
	}
}