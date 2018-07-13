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
        }

        [Test]
        public void ShouldRegisterIHttpClientImplementation()
        {
            _services.AddVstsNet(InstanceName, Token);
            _services.Count.Should().BeGreaterThan(0);
            _services.Should().ContainSingle(d => d.ServiceType == typeof(IHttpClient) && d.Lifetime == ServiceLifetime.Singleton);

            var httpClientRegistration = _services.Single(d => d.ServiceType == typeof(IHttpClient));
            var httpClient = httpClientRegistration.ImplementationFactory(Mock.Of<IServiceProvider>()) as DefaultHttpClient;

            httpClient.Should().NotBeNull();
        }

        [Test]
        public void ShouldRegisterAllVstsClientInterfaces()
        {
            _services.AddVstsNet(InstanceName, Token);
            TestClientRegistration<IVstsPullRequestsClient>();
            TestClientRegistration<IVstsWorkItemsClient>();
            TestClientRegistration<IVstsClient>();
        }

        [Test]
        public void ShouldRegisterDefaultConfiguration()
        {
            _services.AddVstsNet(InstanceName, Token);
            var clientRegistration = _services.Single(d => d.ServiceType == typeof(IVstsClient));
            var mockServiceProvider = new Mock<IServiceProvider>();
            var client = clientRegistration.ImplementationFactory(mockServiceProvider.Object) as VstsClient;

            client.Configuration.Should().NotBeNull();
            client.Configuration.Should().BeEquivalentTo(VstsClientConfiguration.Default);
        }

        [Test]
        public void ShouldRegisterCustomConfiguration()
        {
            const string expectedPRApiVersion = "5.1";
            const string expecetdWIAPIVersion = "3.0";
            const int expectedWorkitemsBatchSize = 210;

            _services.AddVstsNet(InstanceName, Token, config => 
            {
                config.WorkItemsApiVersion = expecetdWIAPIVersion;
                config.PullRequestsApiVersion = expectedPRApiVersion;
                config.WorkitemsBatchSize = expectedWorkitemsBatchSize;
            });
            var clientRegistration = _services.Single(d => d.ServiceType == typeof(IVstsClient));
            var mockServiceProvider = new Mock<IServiceProvider>();
            var client = clientRegistration.ImplementationFactory(mockServiceProvider.Object) as VstsClient;

            client.Configuration.Should().NotBeNull();
            client.Configuration.WorkItemsApiVersion.Should().Be(expecetdWIAPIVersion);
            client.Configuration.PullRequestsApiVersion.Should().Be(expectedPRApiVersion);
            client.Configuration.WorkitemsBatchSize.Should().Be(expectedWorkitemsBatchSize);
        }

        private void TestClientRegistration<T>()
        {
            var mockServiceProvider = new Mock<IServiceProvider>();
            mockServiceProvider.Setup(p => p.GetService(It.Is<Type>(t => t == typeof(IVstsClient))))
                .Returns(_services.Single(d => d.ServiceType == typeof(IVstsClient)).ImplementationFactory(mockServiceProvider.Object));

            _services.Count.Should().BeGreaterThan(0);
            _services.Should().ContainSingle(d => d.ServiceType == typeof(T) && d.Lifetime == ServiceLifetime.Singleton);

            var clientRegistration = _services.Single(d => d.ServiceType == typeof(T));
            var client = clientRegistration.ImplementationFactory(mockServiceProvider.Object) as VstsClient;

            client.Should().NotBeNull();
        }
    }
}
