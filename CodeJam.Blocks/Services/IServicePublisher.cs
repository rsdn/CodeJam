using System;

using JetBrains.Annotations;

namespace CodeJam.Services
{
	/// <summary>
	/// Service publisher interface.
	/// </summary>
	[PublicAPI]
	public interface IServicePublisher : IServiceProvider
	{
		/// <summary>
		/// Publish service.
		/// </summary>
		/// <param name="serviceType">Type of service object to publish.</param>
		/// <param name="serviceInstance">Instance of service of type <paramref name="serviceType"/>.</param>
		/// <returns>Disposable cookie to conceal published service</returns>
		IDisposable Publish(Type serviceType, object serviceInstance);

		/// <summary>
		/// Publish service.
		/// </summary>
		/// <param name="serviceType">Type of service object to publish.</param>
		/// <param name="instanceFactory">Factory to create service instance</param>
		/// <returns>Disposable cookie to conceal published service</returns>
		IDisposable Publish(
			Type serviceType,
			Func<IServicePublisher, object> instanceFactory);
	}
}