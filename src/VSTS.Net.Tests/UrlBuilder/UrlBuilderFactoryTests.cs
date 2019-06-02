using System;
using System.Collections.Generic;
using System.Text;
using FluentAssertions;
using NUnit.Framework;
using VSTS.Net.Types;

namespace VSTS.Net.Tests.UrlBuilder
{
    [TestFixture]
    public class UrlBuilderFactoryTests
    {
        [TestCase("bar", "", "https://foo.bar.visualstudio.com")]
        [TestCase("bar", null, "https://foo.bar.visualstudio.com")]
        [TestCase("bar", "baz", "https://foo.baz.visualstudio.com")]
        [TestCase("", "", "https://foo.visualstudio.com")]
        [TestCase(null, null, "https://foo.visualstudio.com")]
        [TestCase("", "baz", "https://foo.baz.visualstudio.com")]
        [TestCase(null, "baz", "https://foo.baz.visualstudio.com")]
        public void OnlineUrlBuilderFactoryTest(string subDomain, string subDomainOverride, string expected)
        {
            var factory = new OnlineUrlBuilderFactory(instance: "foo", subDomain: subDomain);
            var result = factory.Create(subDomainOverride).Build();

            result.Should().Be($"{expected}?api-version={Constants.CurrentWorkItemsApiVersion}");
        }

        [Test]
        public void OnPremUrlBuilderFactoryTest()
        {
            var factory = new OnPremUrlBuilderFactory(new Uri("https://foo.com"));
            var result = factory.Create().Build();

            result.Should().Be($"https://foo.com?api-version={Constants.CurrentWorkItemsApiVersion}");
        }
    }
}
