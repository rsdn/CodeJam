using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using CodeJam.Targeting;

using JetBrains.Annotations;

namespace CodeJam
{
	public static class TestTools
	{
		[NotNull]
		public static Random GetTestRandom() =>
			GetTestRandom(new Random().Next());

		[NotNull]
		public static Random GetTestRandom(int seed)
		{
			var currentMethod =
#if !LESSTHAN_NETSTANDARD20 && !LESSTHAN_NETCOREAPP20
				MethodBase.GetCurrentMethod().Name;
#else
				GetCurrentMethodName();
#endif
			Console.WriteLine(
				$"{currentMethod}: Rnd seed: {seed} (use the seed to reproduce test results).");
			return new Random(seed);
		}

#if LESSTHAN_NETSTANDARD20 || LESSTHAN_NETCOREAPP20
		private static string GetCurrentMethodName([System.Runtime.CompilerServices.CallerMemberName] string memberName = "") => memberName;
#endif

		[NotNull, LinqTunnel]
		public static IEnumerable<T> Shuffle<T>([NotNull] this IEnumerable<T> source, [NotNull] Random rnd) =>
			source.OrderBy(i => rnd.Next());

		[NotNull, LinqTunnel]
		public static IEnumerable<Holder<T>> Wrap<T>([NotNull] this IEnumerable<T> source) =>
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

		private static void PrintProps([NotNull] string typeName)
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