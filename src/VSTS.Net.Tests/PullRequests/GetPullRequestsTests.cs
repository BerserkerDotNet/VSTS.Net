using FluentAssertions;
using Moq;
using NUnit.Framework;
using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using VSTS.Net.Models.PullRequests;
using VSTS.Net.Models.Request;
using VSTS.Net.Models.Response;
using VSTS.Net.Tests.Types;

namespace VSTS.Net.Tests.PullRequests
{
    [TestFixture]
    public class GetPullRequestsTests : BaseHttpClientTests
    {
        [Test, Combinatorial]
        public void ThrowsIfEmptyInput(
            [Values(null, "", ProjectName)]string project,
            [Values(null, "", RepositoryName)]string repository)
        {
            if (!string.IsNullOrEmpty(project) && !string.IsNullOrEmpty(repository))
                return;

            _client.Awaiting(c => c.GetPullRequestsAsync(project, repository, null, _cancellationToken))
                .Should().Throw<ArgumentNullException>();
        }

        [Test]
        public void DoesNotThrowIfNullQuery()
        {
            _client.Awaiting(c => c.GetPullRequestsAsync(ProjectName, RepositoryName, null, _cancellationToken))
                .Should().NotThrow<ArgumentNullException>();
        }

        [Test]
        public async Task QueriesPullRequestsFromServer()
        {
            var pullRequests = new[] { new PullRequest(), new PullRequest() };
            SetupOnePageOf(pullRequests);

            var result = await _client.GetPullRequestsAsync(ProjectName, RepositoryName, null, _cancellationToken);

            result.Should().NotBeNull();
            result.Should().NotBeEmpty();
            result.Should().BeEquivalentTo(pullRequests);
        }

        [Test]
        public async Task ReturnsEmptyListIfNullResponse()
        {
            SetupGetCollectionOf<PullRequest>()
                .ReturnsAsync((CollectionResponse<PullRequest>)null);

            var result = await _client.GetPullRequestsAsync(ProjectName, RepositoryName, null, _cancellationToken);

            result.Should().NotBeNull();
            result.Should().BeEmpty();
        }

        [Test]
        public async Task RequestsCorrectUrl()
        {
            var pullRequests = new[] { new PullRequest(), new PullRequest() };
            SetupOnePageOf(pullRequests, s => ValidateUrl(s));

            var result = await _client.GetPullRequestsAsync(ProjectName, RepositoryName, null, _cancellationToken);

            _httpClientMock.VerifyAll();

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

            var result = await _client.GetPullRequestsAsync(ProjectName, RepositoryName, query, _cancellationToken);

            _httpClientMock.VerifyAll();

            result.Should().NotBeNull();
            result.Should().NotBeEmpty();
            result.Should().BeEquivalentTo(pullRequests);
        }

        [Test]
        public async Task FetchAllPages()
        {
            var page1 = new[] { new PullRequest(), new PullRequest() };
            var page2 = new[] { new PullRequest(), new PullRequest() };
            var page3 = new[] { new PullRequest() };

            int skipTotal = 0;
            SetupPagedGetCollectionOf<PullRequest>(u => ValidateUrlWithPaging(u, ref skipTotal))
                .ReturnsAsync(new CollectionResponse<PullRequest> { Value = page1 })
                .ReturnsAsync(new CollectionResponse<PullRequest> { Value = page2 })
                .ReturnsAsync(new CollectionResponse<PullRequest> { Value = page3 })
                .ReturnsAsync(new CollectionResponse<PullRequest> { Value = Enumerable.Empty<PullRequest>() });

            var result = await _client.GetPullRequestsAsync(ProjectName, RepositoryName, null, _cancellationToken);

            skipTotal.Should().Be(11);
            result.Should().NotBeNull();
            result.Should().NotBeEmpty();
            result.Should().HaveCount(5);
            result.Should().BeEquivalentTo(page1.Union(page2).Union(page3));
        }

        [Test]
        public async Task FilterByDate()
        {
            var pullRequests = new[] { CreatePR(daysAgo: 5), CreatePR(daysAgo: 3), CreatePR(daysAgo: 2) };
            SetupOnePageOf(pullRequests);
            var query = new PullRequestQuery { CreatedAfter = DateTime.UtcNow.AddDays(-4) };

            var result = await _client.GetPullRequestsAsync(ProjectName, RepositoryName, query, _cancellationToken);

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

            var result = await _client.GetPullRequestsAsync(ProjectName, RepositoryName, query, _cancellationToken);

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

            var result = await _client.GetPullRequestsAsync(ProjectName, RepositoryName, query, _cancellationToken);

            result.Should().HaveCount(2);
            result.Should().OnlyContain(p => query.CustomFilter(p));
        }

        [Test]
        public async Task CustomFilterWithDateFilter()
        {
            var pullRequests = new[] { CreatePR("Bug 1", daysAgo: 2), CreatePR("Task 1", daysAgo: 2), CreatePR("Bug 2", daysAgo: 4) };
            SetupOnePageOf(pullRequests);
            var query = new PullRequestQuery { CustomFilter = p => p.Title.StartsWith("Bug"), CreatedAfter = DateTime.UtcNow.AddDays(-3) };

            var result = await _client.GetPullRequestsAsync(ProjectName, RepositoryName, query, _cancellationToken);

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

            _client.Awaiting(c => c.GetPullRequestsAsync(ProjectName, RepositoryName, null, _cancellationToken))
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

        private PullRequest CreatePR(DateTime? createdOn)
        {
            if (!createdOn.HasValue)
                createdOn = DateTime.UtcNow;

            return new PullRequest { CreationDate = createdOn.Value };
        }

        private PullRequest CreatePR(int daysAgo)
        {
            return CreatePR(DateTime.UtcNow.AddDays(-daysAgo));
        }

        private PullRequest CreatePR(string title, int daysAgo = 1)
        {
            return new PullRequest { Title = title, CreationDate = DateTime.UtcNow.AddDays(-daysAgo) };
        }
    }
}
