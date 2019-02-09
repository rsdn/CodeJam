#if LESSTHAN_NETSTANDARD20 || LESSTHAN_NETCOREAPP20

using System;
using System.Globalization;

namespace CodeJam
{
	internal class InvariantCultureStringComparer : StringComparer
	{
		public static readonly InvariantCultureStringComparer CompareCase = new InvariantCultureStringComparer(false);
		public static readonly InvariantCultureStringComparer IgnoreCase = new InvariantCultureStringComparer(true);

		private static readonly CompareInfo _invariantCulture = new CultureInfo("").CompareInfo;

		private readonly CompareOptions _compareOptions;

		private InvariantCultureStringComparer(bool ignoreCase) =>
			_compareOptions = ignoreCase ? CompareOptions.IgnoreCase : 0;

		#region Overrides of StringComparer

		public override int Compare(string x, string y)
		{
			if (object.ReferenceEquals(x, y)) return 0;
			if (object.ReferenceEquals(x, null)) return -1;
			if (object.ReferenceEquals(y, null)) return 1;

			return _invariantCulture.Compare(x, y, _compareOptions);
		}

		public override bool Equals(string x, string y)
		{
			if (object.ReferenceEquals(x ,y)) return true;
			if (object.ReferenceEquals(x, null) || object.ReferenceEquals(y, null)) return false;

			return _invariantCulture.Compare(x, y, _compareOptions) == 0;
		}

		public override int GetHashCode(string obj) => _invariantCulture.GetHashCode(obj, _compareOptions);

		#endregion
	}
}

#endif