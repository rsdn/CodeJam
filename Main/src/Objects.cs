using System;

namespace CodeJam
{
	internal static class Objects
	{
		public static readonly Random Random = new Random(unchecked((int)DateTime.Now.Ticks));
	}
}
