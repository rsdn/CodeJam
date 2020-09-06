using System;

namespace CodeJam
{
	internal static class Objects
	{
		[ThreadStatic]
		private static Random? _random;

		public static Random Random => _random ??= new Random(unchecked((int)DateTime.Now.Ticks));
	}
}
