using System;
using System.Threading.Tasks;
using FizzWare.NBuilder;
using FluentAssertions;
using NUnit.Framework;
using VSTS.Net.Models.Identity;
using VSTS.Net.Tests.Types;

namespace VSTS.Net.Tests.Identities
{
    [TestFixture]
    public class GetIdentitiesTests : BaseHttpClientTests
    {
        [Test]
        public void ThrowExceptionIfSearchFilterNullOrEmpty([Values(null, "")] string searchFilter)
        {
            Client.Awaiting(c => c.GetIdentitiesAsync("foo", searchFilter: searchFilter))
                .Should().Throw<ArgumentNullException>();
        }

        [Test]
        public void ThrowExceptionIfFilterValueNullOrEmpty([Values(null, "")] string filterValue)
        {
            Client.Awaiting(c => c.GetIdentitiesAsync(filterValue))
                .Should().Throw<ArgumentNullException>();
        }

        [Test]
        public async Task ReturnListOfIdentities()
        {
            const string expectedFilterValue = "foo";
            var identities = Builder<Identity>.CreateListOfSize(4).Build();
            SetupOnePageOf(identities, u => VerifyUrl(u, expectedFilterValue));

            var result = await Client.GetIdentitiesAsync(expectedFilterValue, cancellationToken: CancellationToken);

            result.Should().BeEquivalentTo(identities);
        }

        [Test]
        public async Task ReturnListOfActiveIdentities()
        {
            const string expectedFilterValue = "foo";
            var identities = Builder<Identity>.CreateListOfSize(4)
                .All()
                .With(i => i.IsActive = false)
                .TheLast(1)
                .With(i => i.IsActive = true)
                .Build();
            SetupOnePageOf(identities);

            var result = await Client.GetIdentitiesAsync(expectedFilterValue, onlyActive: true, cancellationToken: CancellationToken);

            result.Should().HaveCount(1);
            result.Should().OnlyContain(i => i.IsActive);
        }

        [Test]
        public async Task ReturnEmptyIfNoIdentities()
        {
            const string expectedFilterValue = "foo";
            SetupOnePageOf<Identity>(null);

            var result = await Client.GetIdentitiesAsync(expectedFilterValue, cancellationToken: CancellationToken);

            result.Should().BeEmpty();
        }

        private bool VerifyUrl(string url, string filterValue)
        {
            return url.Contains("/Identities?")
                && url.Contains("vssps")
                && url.Contains("searchFilter=General")
                && url.Contains($"filterValue={filterValue}");
        }
    }
}
