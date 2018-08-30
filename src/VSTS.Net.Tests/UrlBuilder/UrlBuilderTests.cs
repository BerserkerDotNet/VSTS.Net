using System;
using FluentAssertions;
using NUnit.Framework;
using VSTS.Net.Types;

namespace VSTS.Net.Tests.UrlBuilder
{
    [TestFixture]
    public class UrlBuilderTests
    {
        [Test]
        public void ComposeOnlineUrlWithInstanceName()
        {
            var result = VstsUrlBuilder.Create("foo").Build();
            result.Should().Be($"https://foo.visualstudio.com?api-version={Constants.CurrentWorkItemsApiVersion}");
        }

        [Test]
        public void ComposeOnlineUrlWihSubdomainAndInstanceName()
        {
            var result = VstsUrlBuilder.Create(instance: "foo", subDomain: "bar.buzz").Build();
            result.Should().Be($"https://foo.bar.buzz.visualstudio.com?api-version={Constants.CurrentWorkItemsApiVersion}");
        }

        [TestCase("http://foo.mycompany.com")]
        [TestCase("https://foo.mycompany.com")]
        [TestCase("https://foo.mycompany.buzz.com")]
        [TestCase("http://localhost:45789")]
        [TestCase("http://localhost/")]
        public void ComposeUrlWithCustomBaseAddress(string baseAddress)
        {
            var result = VstsUrlBuilder.Create(new Uri(baseAddress)).Build();
            result.Should().Be($"{baseAddress.Trim('/')}?api-version={Constants.CurrentWorkItemsApiVersion}");
        }
    }
}
