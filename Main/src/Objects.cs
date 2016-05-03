using System;

namespace CodeJam
{
	static class Objects
	{
		public static readonly Random Random = new Random((int)DateTime.Now.Ticks);
	}
}
