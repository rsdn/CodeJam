using System;

using NUnit.Framework;

namespace CodeJam.Services
{
	[TestFixture]
	public class ServicesTests
	{
		[Test]
		public void Instance()
		{
			var container = new ServiceContainer();
			Assert.IsNull(container.GetService<ISampleSvc>(), "#A01");
			Assert.AreEqual(container, container.GetService<IServicePublisher>(), "#A08");

			var sampleSvc = new SampleSvc();

			using (container.Publish<ISampleSvc>(sampleSvc))
			{
				Assert.AreEqual(sampleSvc, container.GetService<ISampleSvc>(), "#A02");
				Assert.Throws<ArgumentException>(() => container.Publish<ISampleSvc>(sampleSvc));
			}
			Assert.IsNull(container.GetService<ISampleSvc>(), "#A03");
		}

		[Test]
		public void InstanceFactory()
		{
			var container = new ServiceContainer();
			var sampleSvc = new SampleSvc();
			var calls = 0;
			using (
				container.Publish<ISampleSvc>(
					// ReSharper disable ImplicitlyCapturedClosure
					sp =>
						// ReSharper restore ImplicitlyCapturedClosure
					{
						calls++;
						return sampleSvc;
					}))
			{
				Assert.AreEqual(0, calls, "#A01");
				Assert.AreEqual(sampleSvc, container.GetService<ISampleSvc>(), "#A02");
				Assert.AreEqual(1, calls, "#A03");
			}
			Assert.IsNull(container.GetService<ISampleSvc>(), "#A04");
		}

		[Test]
		public void ContainerChain()
		{
			var parent = new ServiceContainer();
			var child = new ServiceContainer(parent);
			var svc = new SampleSvc();
			using (parent.Publish<ISampleSvc>(svc))
				Assert.AreEqual(svc, child.GetService<ISampleSvc>(), "#A01");
			Assert.IsNull(child.GetService<ISampleSvc>(), "#A02");
		}

		[Test]
		public void SvcDispose()
		{
			var calls = 0;
			using (var container = new ServiceContainer())
			{
				using (container.Publish(sp => Disposable.Create(() => calls++)))
				{
					Assert.AreEqual(0, calls, "#A01");
					// ReSharper disable once ReturnValueOfPureMethodIsNotUsed
					container.GetService<IDisposable>();
					Assert.AreEqual(0, calls, "#A02");
				}
				Assert.AreEqual(1, calls, "#A03");
				container.Publish(sp => Disposable.Create(() => calls++));
				// ReSharper disable once ReturnValueOfPureMethodIsNotUsed
				container.GetService<IDisposable>();
				Assert.AreEqual(1, calls, "#A04");
			}
			Assert.AreEqual(2, calls, "#A05");
		}

		private interface ISampleSvc { }

		private class SampleSvc : ISampleSvc { }
	}
}