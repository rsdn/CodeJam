using System;

using JetBrains.Annotations;

// ReSharper disable once CheckNamespace

namespace BenchmarkDotNet.Toolchains
{
	/// <summary>Common API to run the target/idle/setup/cleanup methods</summary>
	[PublicAPI]
	public abstract class BenchmarkAction
	{
		/// <summary>Gets or sets the invoke single callback.</summary>
		/// <value>The invoke single callback.</value>
		public Action InvokeSingle { get; protected set; }
		/// <summary>Gets or sets the invoke multiple times callback.</summary>
		/// <value>The invoke multiple times callback.</value>
		public Action<long> InvokeMultiple { get; protected set; }

		/// <summary>Gets the last run result.</summary>
		/// <value>The last run result.</value>
		public virtual object LastRunResult => null;
	}
}