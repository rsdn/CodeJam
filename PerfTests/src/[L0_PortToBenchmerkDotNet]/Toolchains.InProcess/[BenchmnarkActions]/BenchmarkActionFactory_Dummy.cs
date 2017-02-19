using System;

using JetBrains.Annotations;

// ReSharper disable once CheckNamespace

namespace BenchmarkDotNet.Toolchains.InProcess
{
	/// <summary>Helper class that creates <see cref="BenchmarkAction"/> instances. </summary>
	public static partial class BenchmarkActionFactory
	{
		private class DummyInstance
		{
			[UsedImplicitly]
			private int _dummyField;

			public void Dummy()
			{
				// copy-pasted as emit is not supported across all platforms
				_dummyField++; // 0
				_dummyField++; // 1
				_dummyField++; // 2
				_dummyField++; // 3
				_dummyField++; // 4
				_dummyField++; // 5
				_dummyField++; // 6
				_dummyField++; // 7
				_dummyField++; // 8
				_dummyField++; // 9
				_dummyField++; // 10
				_dummyField++; // 11
				_dummyField++; // 12
				_dummyField++; // 13
				_dummyField++; // 14
				_dummyField++; // 15
				_dummyField++; // 16
				_dummyField++; // 17
				_dummyField++; // 18
				_dummyField++; // 19
				_dummyField++; // 20
				_dummyField++; // 21
				_dummyField++; // 22
				_dummyField++; // 23
				_dummyField++; // 24
				_dummyField++; // 25
				_dummyField++; // 26
				_dummyField++; // 27
				_dummyField++; // 28
				_dummyField++; // 29
				_dummyField++; // 30
				_dummyField++; // 31
				_dummyField++; // 32
				_dummyField++; // 33
				_dummyField++; // 34
				_dummyField++; // 35
				_dummyField++; // 36
				_dummyField++; // 37
				_dummyField++; // 38
				_dummyField++; // 39
				_dummyField++; // 40
				_dummyField++; // 41
				_dummyField++; // 42
				_dummyField++; // 43
				_dummyField++; // 44
				_dummyField++; // 45
				_dummyField++; // 46
				_dummyField++; // 47
				_dummyField++; // 48
				_dummyField++; // 49
				_dummyField++; // 50
				_dummyField++; // 51
				_dummyField++; // 52
				_dummyField++; // 53
				_dummyField++; // 54
				_dummyField++; // 55
				_dummyField++; // 56
				_dummyField++; // 57
				_dummyField++; // 58
				_dummyField++; // 59
				_dummyField++; // 60
				_dummyField++; // 61
				_dummyField++; // 62
				_dummyField++; // 63
			}
		}
	}
}