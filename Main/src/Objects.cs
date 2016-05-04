using System;

namespace CodeJam
{
	static class Objects
	{
		public static readonly Random Random = new Random(unchecked((int)DateTime.Now.Ticks));
	}
}
