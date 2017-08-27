using System;

using JetBrains.Annotations;

namespace CodeJam.Services
{
	/// <summary>
	/// <see cref="IServiceProvider"/> and <see cref="IServicePublisher"/> helper methods.
	/// </summary>
	[PublicAPI]
	public static class ServiceProviderHelper
	{
		/// <summary>
		/// Gets the service object of the specified type, or throws an exception if service not registered.
		/// </summary>
		/// <param name="provider">Instance of <see cref="IServiceProvider"/>.</param>
		/// <param name="serviceType">An object that specifies the type of service object to get.</param>
		/// <returns>
		/// A service object of type <paramref name="serviceType"/>.
		/// </returns>
		[NotNull]
		[Pure]
		public static object GetRequiredService(
			[NotNull] this IServiceProvider provider,
			[NotNull] Type serviceType)
		{
			Code.NotNull(provider, nameof(provider));
			Code.NotNull(serviceType, nameof(serviceType));

			var svc = provider.GetService(serviceType);
			if (svc == null)
				throw new ArgumentException($"Service '{serviceType}' is not registered in provider.");
			return svc;
		}

		/// <summary>
		/// Gets the service object of the specified type.
		/// </summary>
		/// <typeparam name="T">An object that specifies the type of service object to get.</typeparam>
		/// <param name="provider">Instance of <see cref="IServiceProvider"/>.</param>
		/// <returns>A service object of type serviceType.</returns>
		[CanBeNull]
		[Pure]
		public static T GetService<T>([NotNull] this IServiceProvider provider)
		{
			Code.NotNull(provider, nameof(provider));
			return (T)provider.GetService(typeof(T));
		}

		/// <summary>
		/// Gets the service object of the specified type, or throws an exception if service not registered.
		/// </summary>
		/// <param name="provider">Instance of <see cref="IServiceProvider"/>.</param>
		/// <typeparam name="T">An object that specifies the type of service object to get.</typeparam>
		/// <returns>
		/// A service object of type <typeparamref name="T"/>.
		/// </returns>
		[NotNull]
		[Pure]
		public static T GetRequiredService<T>([NotNull] this IServiceProvider provider) =>
			(T)provider.GetRequiredService(typeof(T));

		/// <summary>
		/// Publish service.
		/// </summary>
		/// <typeparam name="T">Type of service object to publish.</typeparam>
		/// <param name="publisher">Service publisher.</param>
		/// <param name="serviceInstance">Instance of service of type <typeparamref name="T"/></param>
		/// <returns>Disposable cookie to conceal published service</returns>
		[NotNull]
		public static IDisposable Publish<T>(
			[NotNull] this IServicePublisher publisher,
			[NotNull] T serviceInstance) where T : class
		{
			Code.NotNull(publisher, nameof(publisher));
			Code.NotNull(serviceInstance, nameof(serviceInstance));
			return publisher.Publish(typeof(T), serviceInstance);
		}

		/// <summary>
		/// Publish service.
		/// </summary>
		/// <typeparam name="T">Type of service object to publish.</typeparam>
		/// <param name="publisher">Service publisher.</param>
		/// <param name="instanceFactory">Factory to create service instance</param>
		/// <returns>Disposable cookie to conceal published service</returns>
		[NotNull]
		public static IDisposable Publish<T>(
			[NotNull] this IServicePublisher publisher,
			[NotNull] Func<IServicePublisher, T> instanceFactory) where T : class
		{
			Code.NotNull(publisher, nameof(publisher));
			Code.NotNull(instanceFactory, nameof(instanceFactory));
			// DONTTOUCH: for valid overload resolution (fails on net35).
#if SUPPORTS_NET35
			// Hack for CS0030 Cannot convert type 'System.Func<CodeJam.Services.IServicePublisher, T>' to 'System.Func<CodeJam.Services.IServicePublisher, object>'
			return publisher.Publish(typeof(T), new Func<IServicePublisher, object>(sp => instanceFactory(sp)));
#else
			// ReSharper disable once RedundantCast
			return publisher.Publish(typeof(T), (Func<IServicePublisher, object>)instanceFactory);
#endif
		}
	}
}