using System.Collections.Generic;

namespace CodeJam
{
	public static class ArgumentAssertionExtensions
	{
		public static ArgumentAssertion<T> NotNull<T>(this ArgumentAssertion<T> arg) where T : class
		{
			Code.NotNull(arg.Argument, arg.ArgumentName);
			return arg;
		}

		public static ArgumentAssertion<IEnumerable<T>> ItemNotNull<T>(this ArgumentAssertion<IEnumerable<T>> arg) where T : class
		{
			Code.ItemNotNull(arg.Argument, arg.ArgumentName);
			return arg;
		}

		public static ArgumentAssertion<T?> NotNull<T>(this ArgumentAssertion<T?> arg) where T : struct
		{
			Code.NotNull(arg.Argument, arg.ArgumentName);
			return arg;
		}
	}
}