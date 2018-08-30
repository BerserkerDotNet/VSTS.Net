using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using VSTS.Net.Models.PullRequests;
using VSTS.Net.Models.Request;
using VSTS.Net.Models.Response;
using VSTS.Net.Tests.Types;

namespace VSTS.Net.Tests.PullRequests
{
    [TestFixture]
    public class GetPullRequestsTests : BaseHttpClientTests
    {
        [Test]
        [Combinatorial]
        public void ThrowsIfEmptyInput(
            [Values(null, "", ProjectName)]string project,
            [Values(null, "", RepositoryName)]string repository)
        {
            if (!string.IsNullOrEmpty(project) && !string.IsNullOrEmpty(repository))
            {
                return;
            }

            Client.Awaiting(c => c.GetPullRequestsAsync(project, repository, null, CancellationToken))
                .Should().Throw<ArgumentNullException>();
        }

        [Test]
        public void DoesNotThrowIfNullQuery()
        {
            Client.Awaiting(c => c.GetPullRequestsAsync(ProjectName, RepositoryName, null, CancellationToken))
                .Should().NotThrow<ArgumentNullException>();
        }

        [Test]
        public async Task QueriesPullRequestsFromServer()
        {
            var pullRequests = new[] { new PullRequest(), new PullRequest() };
            SetupOnePageOf(pullRequests);

            var result = await Client.GetPullRequestsAsync(ProjectName, RepositoryName, null, CancellationToken);

            result.Should().NotBeNull();
            result.Should().NotBeEmpty();
            result.Should().BeEquivalentTo(pullRequests);
        }

        [Test]
        public async Task ReturnsEmptyListIfNullResponse()
        {
            SetupGetCollectionOf<PullRequest>()
                .ReturnsAsync((CollectionResponse<PullRequest>)null);

            var result = await Client.GetPullRequestsAsync(ProjectName, RepositoryName, null, CancellationToken);

            result.Should().NotBeNull();
            result.Should().BeEmpty();
        }

        [Test]
        public async Task RequestsCorrectUrl()
        {
            var pullRequests = new[] { new PullRequest(), new PullRequest() };
            SetupOnePageOf(pullRequests, s => ValidateUrl(s));

            var result = await Client.GetPullRequestsAsync(ProjectName, RepositoryName, null, CancellationToken);

            HttpClientMock.VerifyAll();

            result.Should().NotBeNull();
            result.Should().NotBeEmpty();
            result.Should().BeEquivalentTo(pullRequests);
        }

        [Test]
        public async Task IncludesQueryParametersInUrl()
        {
            var query = new PullRequestQuery
            {
                CreatorId = Guid.NewGuid(),
                ReviewerId = Guid.NewGuid(),
                Status = "Active",
                CreatedAfter = DateTime.UtcNow.AddDays(-3)
            };
            var pullRequests = new[] { CreatePR(daysAgo: 2), CreatePR(daysAgo: 1) };
            SetupOnePageOf(pullRequests, s => ValidateUrlWithQuery(s, query));

            var result = await Client.GetPullRequestsAsync(ProjectName, RepositoryName, query, CancellationToken);

            HttpClientMock.VerifyAll();

            result.Should().NotBeNull();
            result.Should().NotBeEmpty();
            result.Should().BeEquivalentTo(pullRequests);
        }

        [Test]
        public async Task FetchAllPages()
        {
            var page1 = new[] { CreatePR(), CreatePR() };
            var page2 = new[] { CreatePR(), CreatePR() };
            var page3 = new[] { CreatePR() };

            int skipTotal = 0;
            SetupPagedGetCollectionOf<PullRequest>(u => ValidateUrlWithPaging(u, ref skipTotal))
                .ReturnsAsync(new CollectionResponse<PullRequest> { Value = page1 })
                .ReturnsAsync(new CollectionResponse<PullRequest> { Value = page2 })
                .ReturnsAsync(new CollectionResponse<PullRequest> { Value = page3 })
                .ReturnsAsync(new CollectionResponse<PullRequest> { Value = Enumerable.Empty<PullRequest>() });

            var result = await Client.GetPullRequestsAsync(ProjectName, RepositoryName, null, CancellationToken);

            skipTotal.Should().Be(11);
            result.Should().NotBeNull();
            result.Should().NotBeEmpty();
            result.Should().HaveCount(5);
            result.Should().BeEquivalentTo(page1.Union(page2).Union(page3));
        }

        [Test]
        public async Task CorrectlySetSkipCountWithQuery()
        {
            var page1 = new[] { CreatePR("PR 1"), CreatePR("PR 2 skip") };
            var page2 = new[] { CreatePR("PR 3 skip"), CreatePR("PR 4") };
            var page3 = new[] { CreatePR("PR 5") };

            int skipTotal = 0;
            SetupPagedGetCollectionOf<PullRequest>(u => ValidateUrlWithPaging(u, ref skipTotal))
                .ReturnsAsync(new CollectionResponse<PullRequest> { Value = page1 })
                .ReturnsAsync(new CollectionResponse<PullRequest> { Value = page2 })
                .ReturnsAsync(new CollectionResponse<PullRequest> { Value = page3 })
                .ReturnsAsync(new CollectionResponse<PullRequest> { Value = Enumerable.Empty<PullRequest>() });

            var query = new PullRequestQuery { CustomFilter = p => !p.Title.Contains("skip") };
            var result = await Client.GetPullRequestsAsync(ProjectName, RepositoryName, query, CancellationToken);

            result.Should().HaveCount(3);
            skipTotal.Should().Be(11);
        }

        [Test]
        public async Task DoNotFetchMorePullRequestsIfCreatedAfterIsSet()
        {
            var page1 = new[] { CreatePR("PR 1", daysAgo: 1), CreatePR("PR 2 skip", daysAgo: 1) };
            var page2 = new[] { CreatePR("PR 3 skip", daysAgo: 3), CreatePR("PR 4", daysAgo: 5) };
            var page3 = new[] { CreatePR("PR 5", daysAgo: 6) };

            SetupPagedGetCollectionOf<PullRequest>()
                .ReturnsAsync(new CollectionResponse<PullRequest> { Value = page1 })
                .ReturnsAsync(new CollectionResponse<PullRequest> { Value = page2 })
                .ReturnsAsync(new CollectionResponse<PullRequest> { Value = page3 })
                .ReturnsAsync(new CollectionResponse<PullRequest> { Value = Enumerable.Empty<PullRequest>() });

            var query = new PullRequestQuery { CreatedAfter = DateTime.UtcNow.AddDays(-4) };
            var result = await Client.GetPullRequestsAsync(ProjectName, RepositoryName, query, CancellationToken);

            result.Should().HaveCount(3);
            HttpClientMock.Verify(c => c.ExecuteGet<CollectionResponse<PullRequest>>(It.IsAny<string>(), CancellationToken), Times.Exactly(2));
        }

        [Test]
        public async Task FilterByDate()
        {
            var pullRequests = new[] { CreatePR(daysAgo: 5), CreatePR(daysAgo: 3), CreatePR(daysAgo: 2) };
            SetupOnePageOf(pullRequests);
            var query = new PullRequestQuery { CreatedAfter = DateTime.UtcNow.AddDays(-4) };

            var result = await Client.GetPullRequestsAsync(ProjectName, RepositoryName, query, CancellationToken);

            result.Should().HaveCount(2);
            result.Should().BeEquivalentTo(pullRequests.Skip(1));
        }

        [Test]
        public async Task StopRequestingIfFoundOlderPRsThanRequested()
        {
            var page1 = new[] { CreatePR(daysAgo: 1), CreatePR(daysAgo: 2) };
            var page2 = new[] { CreatePR(daysAgo: 3), CreatePR(daysAgo: 5) };
            var page3 = new[] { CreatePR(daysAgo: 6) };

            SetupPagedGetCollectionOf<PullRequest>()
                .ReturnsAsync(new CollectionResponse<PullRequest> { Value = page1 })
                .ReturnsAsync(new CollectionResponse<PullRequest> { Value = page2 })
                .ReturnsAsync(new CollectionResponse<PullRequest> { Value = page3 })
                .ReturnsAsync(new CollectionResponse<PullRequest> { Value = Enumerable.Empty<PullRequest>() });
            var query = new PullRequestQuery { CreatedAfter = DateTime.UtcNow.AddDays(-4) };

            var result = await Client.GetPullRequestsAsync(ProjectName, RepositoryName, query, CancellationToken);

            VerifyPagedRequests<PullRequest>(Times.Exactly(2));

            result.Should().HaveCount(3);
            result.Should().BeEquivalentTo(page1.Union(page2.Take(1)));
        }

        [Test]
        public async Task CustomFilter()
        {
            var pullRequests = new[] { CreatePR("Bug 1"), CreatePR("Task 1"), CreatePR("Bug 2") };
            SetupOnePageOf(pullRequests);
            var query = new PullRequestQuery { CustomFilter = p => p.Title.StartsWith("Bug") };

            var result = await Client.GetPullRequestsAsync(ProjectName, RepositoryName, query, CancellationToken);

            result.Should().HaveCount(2);
            result.Should().OnlyContain(p => query.CustomFilter(p));
        }

        [Test]
        public async Task CustomFilterWithDateFilter()
        {
            var pullRequests = new[] { CreatePR("Bug 1", daysAgo: 2), CreatePR("Task 1", daysAgo: 2), CreatePR("Bug 2", daysAgo: 4) };
            SetupOnePageOf(pullRequests);
            var query = new PullRequestQuery { CustomFilter = p => p.Title.StartsWith("Bug"), CreatedAfter = DateTime.UtcNow.AddDays(-3) };

            var result = await Client.GetPullRequestsAsync(ProjectName, RepositoryName, query, CancellationToken);

            result.Should().HaveCount(1);
            result.Should().BeEquivalentTo(pullRequests.Take(1));
        }

        [Test]
        public void DoesNotCatchExceptions()
        {
            var page1 = new[] { new PullRequest(), new PullRequest() };

            SetupPagedGetCollectionOf<PullRequest>()
                .ReturnsAsync(new CollectionResponse<PullRequest> { Value = page1 })
                .Throws<Exception>()
                .ReturnsAsync(new CollectionResponse<PullRequest> { Value = Enumerable.Empty<PullRequest>() });

            Client.Awaiting(c => c.GetPullRequestsAsync(ProjectName, RepositoryName, null, CancellationToken))
                .Should().Throw<Exception>();
        }

        private bool ValidateUrlWithPaging(string url, ref int skipTotal)
        {
            var match = Regex.Match(url, "\\$skip=(?<Skip>\\d)");
            var skip = int.Parse(match.Groups["Skip"].Value);
            skipTotal += skip;
            return skip == 0 || skip == 2 || skip == 4 || skip == 5;
        }

        private bool ValidateUrl(string url)
        {
            var expectedUrl = $"https://{InstanceName}.visualstudio.com/{ProjectName}/_apis/git/repositories/{RepositoryName}/pullrequests";
            return url.StartsWith(expectedUrl, StringComparison.OrdinalIgnoreCase);
        }

        private bool ValidateUrlWithQuery(string url, PullRequestQuery query)
        {
            var uri = new Uri(url);
            var queryString = uri.Query;
            return queryString.Contains($"searchCriteria.status={query.Status}") &&
                queryString.Contains($"searchCriteria.reviewerId={query.ReviewerId}") &&
                queryString.Contains($"searchCriteria.creatorId={query.CreatorId}");
        }

        private PullRequest CreatePR(DateTime? createdOn = null)
        {
            if (!createdOn.HasValue)
            {
                createdOn = DateTime.UtcNow;
            }

            return new PullRequest { CreationDate = createdOn.Value, PullRequestId = Random.Next(1, int.MaxValue) };
        }

        private PullRequest CreatePR(int daysAgo)
        {
            return CreatePR(DateTime.UtcNow.AddDays(-daysAgo));
        }

        private PullRequest CreatePR(string title, int daysAgo = 1)
        {
            return new PullRequest { PullRequestId = Random.Next(1, int.MaxValue), Title = title, CreationDate = DateTime.UtcNow.AddDays(-daysAgo) };
        }
    }
}
