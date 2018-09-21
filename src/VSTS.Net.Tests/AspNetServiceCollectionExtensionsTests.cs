using System;
using System.Linq;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using NUnit.Framework;
using VSTS.Net.Extensions;
using VSTS.Net.Interfaces;
using VSTS.Net.Tests.Types;
using VSTS.Net.Types;

namespace VSTS.Net.Tests
{
    [TestFixture]
    public class AspNetServiceCollectionExtensionsTests
    {
        private const string InstanceName = "Foo";
        private const string Token = "Bla";
        private IServiceCollection _services;

        [SetUp]
        public void SetUp()
        {
            _services = new FakeServiceCollection();
        }

        [Test]
        public void ShouldRegisterIHttpClientImplementation()
        {
            _services.AddAzureDevOpsServices(InstanceName, Token);
            _services.Count.Should().BeGreaterThan(0);
            _services.Should().ContainSingle(d => d.ServiceType == typeof(IHttpClient));

            var httpClientRegistration = _services.Single(d => d.ServiceType == typeof(IHttpClient));
        }

        [Test]
        public void ShouldRegisterAllVstsClientInterfaces()
        {
            _services.AddAzureDevOpsServices(InstanceName, Token);
            TestClientRegistration<IVstsWorkItemsMetadataClient>();
            TestClientRegistration<IVstsPullRequestsClient>();
            TestClientRegistration<IVstsWorkItemsClient>();
            TestClientRegistration<IVstsClient>();
        }

        [Test]
        public void ShouldRegisterDefaultConfiguration()
        {
            _services.AddAzureDevOpsServices(InstanceName, Token);
            var configRegistration = _services.Single(d => d.ServiceType == typeof(VstsClientConfiguration));
            var config = configRegistration.ImplementationInstance as VstsClientConfiguration;

            config.Should().NotBeNull();
            config.Should().NotBeNull();
            config.Should().BeEquivalentTo(VstsClientConfiguration.Default);
        }

        [Test]
        public void ShouldRegisterDefaultConfigurationWithOnPrem()
        {
            _services.AddAzureDevOpsServices(new Uri("https://foo.com"), Token);
            var configRegistration = _services.Single(d => d.ServiceType == typeof(VstsClientConfiguration));
            var config = configRegistration.ImplementationInstance as VstsClientConfiguration;

            config.Should().NotBeNull();
            config.Should().NotBeNull();
            config.Should().BeEquivalentTo(VstsClientConfiguration.Default);
        }

        [Test]
        public void ShouldRegisterCustomConfiguration()
        {
            const string expectedPRApiVersion = "5.1";
            const string expecetdWIAPIVersion = "3.0";
            const int expectedWorkitemsBatchSize = 210;

            _services.AddAzureDevOpsServices(InstanceName, Token, cfg =>
            {
                cfg.WorkItemsApiVersion = expecetdWIAPIVersion;
                cfg.PullRequestsApiVersion = expectedPRApiVersion;
                cfg.WorkitemsBatchSize = expectedWorkitemsBatchSize;
            });

            var configRegistration = _services.Single(d => d.ServiceType == typeof(VstsClientConfiguration));
            var config = configRegistration.ImplementationInstance as VstsClientConfiguration;

            config.Should().NotBeNull();
            config.WorkItemsApiVersion.Should().Be(expecetdWIAPIVersion);
            config.PullRequestsApiVersion.Should().Be(expectedPRApiVersion);
            config.WorkitemsBatchSize.Should().Be(expectedWorkitemsBatchSize);
        }

        [Test]
        public void ShouldRegisterCustomConfigurationWithOnPrem()
        {
            const string expectedPRApiVersion = "5.2";
            const string expecetdWIAPIVersion = "3.2";
            const int expectedWorkitemsBatchSize = 123;

            _services.AddAzureDevOpsServices(new Uri("https://foo.com"), Token, cfg =>
            {
                cfg.WorkItemsApiVersion = expecetdWIAPIVersion;
                cfg.PullRequestsApiVersion = expectedPRApiVersion;
                cfg.WorkitemsBatchSize = expectedWorkitemsBatchSize;
            });

            var configRegistration = _services.Single(d => d.ServiceType == typeof(VstsClientConfiguration));
            var config = configRegistration.ImplementationInstance as VstsClientConfiguration;

            config.Should().NotBeNull();
            config.WorkItemsApiVersion.Should().Be(expecetdWIAPIVersion);
            config.PullRequestsApiVersion.Should().Be(expectedPRApiVersion);
            config.WorkitemsBatchSize.Should().Be(expectedWorkitemsBatchSize);
        }

        [Test]
        public void ShouldRegisterOnlineUrlFactoryBuilder()
        {
            _services.AddAzureDevOpsServices(InstanceName, Token);
            _services.Count.Should().BeGreaterThan(0);
            _services.Should().ContainSingle(d => d.ServiceType == typeof(IVstsUrlBuilderFactory) && d.Lifetime == ServiceLifetime.Singleton);

            var urlFactoryRegistration = _services.Single(d => d.ServiceType == typeof(IVstsUrlBuilderFactory));
            var urlFactory = urlFactoryRegistration.ImplementationFactory(Mock.Of<IServiceProvider>()) as OnlineUrlBuilderFactory;

            urlFactory.Should().NotBeNull();
        }

        [Test]
        public void ShouldRegisterOnPremUrlFactoryBuilder()
        {
            const string baseUrl = "https://foo.com";

            _services.AddAzureDevOpsServices(new Uri(baseUrl), Token);
            _services.Count.Should().BeGreaterThan(0);
            _services.Should().ContainSingle(d => d.ServiceType == typeof(IVstsUrlBuilderFactory) && d.Lifetime == ServiceLifetime.Singleton);

            var urlFactoryRegistration = _services.Single(d => d.ServiceType == typeof(IVstsUrlBuilderFactory));
            var urlFactory = urlFactoryRegistration.ImplementationFactory(Mock.Of<IServiceProvider>()) as OnPremUrlBuilderFactory;

            urlFactory.Should().NotBeNull();
        }

        private void TestClientRegistration<T>()
        {
            _services.Count.Should().BeGreaterThan(0);
            _services.Should().ContainSingle(d => d.ServiceType == typeof(T) && d.Lifetime == ServiceLifetime.Singleton);

            var clientRegistration = _services.Single(d => d.ServiceType == typeof(T));
            clientRegistration.ImplementationType.Should().Be(typeof(VstsClient));
        }
    }
}
