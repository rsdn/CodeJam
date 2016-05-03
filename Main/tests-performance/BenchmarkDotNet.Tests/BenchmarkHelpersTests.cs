using System;
using System.Collections.Generic;

using NUnit.Framework;

// ReSharper disable CheckNamespace

namespace BenchmarkDotNet.Helpers.Tests
{
	internal class BenchmarkHelpersTests
	{
		#region Percentile
		/// <summary>
		///  Some pseudo-random values
		/// </summary>
		private static readonly double[] _data = { 1, 2, 3, 4, 1, 2, 3, 1, 3, 45, 12, 43, 11 };

		/// <summary>
		/// Output from Excel PERCENTILE()
		/// </summary>
		private static readonly Dictionary<double, double> _expectedPercentiles = new Dictionary<double, double>
		{
			{ 0.0, 1 },
			{ 0.1, 1 },
			{ 0.2, 1.4 },
			{ 0.3, 2 },
			{ 0.4, 2.8 },
			{ 0.5, 3 },
			{ 0.6, 3.2 },
			{ 0.7, 6.8 },
			{ 0.8, 11.6 },
			{ 0.9, 36.8 },
			{ 1.0, 45 }
		};

		private const double Delta = 1e-7;

		[Test]
		public void TestPercentile()
		{
			Assert.Throws<ArgumentNullException>(() => BenchmarkHelpers.Percentile(null, 0));
			Assert.Throws<ArgumentOutOfRangeException>(() => BenchmarkHelpers.Percentile(_data, -0.1));
			Assert.Throws<ArgumentOutOfRangeException>(() => BenchmarkHelpers.Percentile(_data, 1.1));

			Assert.AreEqual(BenchmarkHelpers.Percentile(new double[0], 0), 0);
			Assert.AreEqual(BenchmarkHelpers.Percentile(new double[0], 0.5), 0);
			Assert.AreEqual(BenchmarkHelpers.Percentile(new double[0], 1), 0);

			foreach (var pair in _expectedPercentiles)
			{
				Assert.AreEqual(BenchmarkHelpers.Percentile(_data, pair.Key), pair.Value, Delta);
			}
		}
		#endregion
	}
}