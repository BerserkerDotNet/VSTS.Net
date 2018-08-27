using FluentAssertions;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Text;
using VSTS.Net.Types;

namespace VSTS.Net.Tests.UrlBuilder
{
    [TestFixture]
    public class UrlBuilderFactoryTests
    {
        [Test]
        public void OnlineUrlBuilderFactoryTest()
        {
            var factory = new OnlineUrlBuilderFactory(instance: "foo", subDomain: "bar");
            var result = factory.Create().Build();

            result.Should().Be($"https://foo.bar.visualstudio.com?api-version={Constants.CurrentWorkItemsApiVersion}");
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
