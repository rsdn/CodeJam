using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using JetBrains.Annotations;

namespace CodeJam
{
	public static class TestTools
	{
		public static Random GetTestRandom() =>
			GetTestRandom(new Random().Next());

		public static Random GetTestRandom(int seed)
		{
			Console.WriteLine(
				$"{MethodBase.GetCurrentMethod().Name}: Rnd seed: {seed} (use the seed to reproduce test results).");
			return new Random(seed);
		}

		public static IEnumerable<T> Shuffle<T>([NotNull] this IEnumerable<T> source, [NotNull] Random rnd) =>
			source.OrderBy(i => rnd.Next());

		public static IEnumerable<Holder<T>> Wrap<T>([NotNull] this IEnumerable<T> source) =>
			source.Select(i => new Holder<T>(i));

		public static void PrintQuirks()
		{
			var assembly = typeof(int).Assembly;

			Console.WriteLine($"{PlatformDependent.TargetPlatform}. Running on {assembly}");
			Console.WriteLine();
			PrintProps("System.Runtime.Versioning.BinaryCompatibility");
			Console.WriteLine();
			PrintProps("System.CompatibilitySwitches");
			Console.WriteLine();
			PrintProps("System.AppContextSwitches");
		}

		private static void PrintProps([NotNull] string typeName)
		{
			var type = typeof(int).Assembly.GetType(typeName);
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