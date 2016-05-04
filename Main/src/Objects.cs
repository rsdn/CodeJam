using System;

namespace CodeJam
{
	internal static class Objects
	{
		[ThreadStatic]
		static Random _random;

		public static Random Random
			=> _random ?? (_random = new Random(unchecked((int)DateTime.Now.Ticks)));
	}
}
