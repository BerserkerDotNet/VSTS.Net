using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using NUnit.Framework;
using System;
using System.Linq;
using VSTS.Net.Extensions;
using VSTS.Net.Interfaces;
using VSTS.Net.Tests.Types;
using VSTS.Net.Types;

namespace VSTS.Net.Tests
{
    [TestFixture]
    public class AspNetServiceCollectionExtensionsTests
    {
        const string InstanceName = "Foo";
        const string Token = "Bla";
        IServiceCollection _services;

        [SetUp]
        public void SetUp()
        {
            _services = new FakeServiceCollection();
            _services.AddVstsNet(InstanceName, Token);
        }

        [Test]
        public void ShouldRegisterIHttpClientImplementation()
        {
            _services.Count.Should().BeGreaterThan(0);
            _services.Should().ContainSingle(d => d.ServiceType == typeof(IHttpClient) && d.Lifetime == ServiceLifetime.Singleton);

            var httpClientRegistration = _services.Single(d => d.ServiceType == typeof(IHttpClient));
            var httpClient = httpClientRegistration.ImplementationFactory(Mock.Of<IServiceProvider>()) as DefaultHttpClient;

            httpClient.Should().NotBeNull();
        }

        [Test]
        public void ShouldRegisterAllVstsClientInterfaces()
        {
            TestClientRegistration<IVstsPullRequestsClient>();
            TestClientRegistration<IVstsWorkItemsClient>();
            TestClientRegistration<IVstsClient>();
        }

        private void TestClientRegistration<T>()
        {
            var mockServiceProvider = new Mock<IServiceProvider>();
            mockServiceProvider.Setup(p => p.GetService(It.Is<Type>(t => t == typeof(IVstsClient))))
                .Returns(new VstsClient("Foo", Mock.Of<IHttpClient>()));
            

            _services.Count.Should().BeGreaterThan(0);
            _services.Should().ContainSingle(d => d.ServiceType == typeof(T) && d.Lifetime == ServiceLifetime.Singleton);

            var clientRegistration = _services.Single(d => d.ServiceType == typeof(T));
            var client = clientRegistration.ImplementationFactory(mockServiceProvider.Object) as VstsClient;

            client.Should().NotBeNull();
        }
    }
}
