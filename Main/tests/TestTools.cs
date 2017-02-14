using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

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

		public static IEnumerable<T> Shuffle<T>(this IEnumerable<T> source, Random rnd) =>
			source.OrderBy(i => rnd.Next());
	}
}