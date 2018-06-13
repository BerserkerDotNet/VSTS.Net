using FluentAssertions;
using Moq;
using NUnit.Framework;
using System;
using System.Linq;
using System.Threading.Tasks;
using VSTS.Net.Models.PullRequests;
using VSTS.Net.Models.Request;
using VSTS.Net.Models.Response;
using VSTS.Net.Tests.Types;
using VSTS.Net.Types;

namespace VSTS.Net.Tests
{
    [TestFixture]
    public class VstsPullRequestsClientTests : BaseHttpClientTests
    {
        [Test, Combinatorial]
        public void GetPullRequestsThrowsIfEmptyInput(
            [Values(null, "", ProjectName)]string project, 
            [Values(null, "", RepositoryName)]string repository)
        {
            if (!string.IsNullOrEmpty(project) && !string.IsNullOrEmpty(repository))
                return;

            _client.Awaiting(c=>c.GetPullRequestsAsync(project, repository, null))
                .Should().Throw<ArgumentNullException>();
        }

        [Test]
        public void GetPullRequestsDoesNotThrowIfNullQuery()
        {
            _client.Awaiting(c => c.GetPullRequestsAsync(ProjectName, RepositoryName, null))
                .Should().NotThrow<ArgumentNullException>();
        }

        [Test]
        public async Task GetPullRequestsQueriesPullRequestsFromServer()
        {
            var pullRequests = new[] { new PullRequest(), new PullRequest() };
            SetupGetCollectionOf<PullRequest>()
                .ReturnsAsync(new CollectionResponse<PullRequest> { Value = pullRequests });

            var result = await _client.GetPullRequestsAsync(ProjectName, RepositoryName, null);

            result.Should().NotBeNull();
            result.Should().NotBeEmpty();
            result.Should().BeEquivalentTo(pullRequests);
        }

        [Test]
        public async Task GetPullRequestsReturnsEmptyListIfNullResponse()
        {
            SetupGetCollectionOf<PullRequest>()
                .ReturnsAsync((CollectionResponse<PullRequest>)null);

            var result = await _client.GetPullRequestsAsync(ProjectName, RepositoryName, null);

            result.Should().NotBeNull();
            result.Should().BeEmpty();
        }

        [Test]
        public async Task GetPullRequestsRequestsCorrectUrl()
        {
            var pullRequests = new[] { new PullRequest(), new PullRequest() };
            SetupGetCollectionOf<PullRequest>(s => ValidateGetPullRequestsUrl(s))
                .ReturnsAsync(new CollectionResponse<PullRequest> { Value = pullRequests })
                .Verifiable();

            var result = await _client.GetPullRequestsAsync(ProjectName, RepositoryName, null);

            _httpClientMock.Verify();

            result.Should().NotBeNull();
            result.Should().NotBeEmpty();
            result.Should().BeEquivalentTo(pullRequests);
        }

        [Test]
        public async Task GetPullRequestsIncludesQueryParametersInUrl()
        {
            var query = new PullRequestQuery
            {
                CreatorId = Guid.NewGuid(),
                ReviewerId = Guid.NewGuid(),
                Status = "Active",
                CreatedAfter = DateTime.UtcNow
            };
            var pullRequests = new[] { new PullRequest(), new PullRequest() };
            SetupGetCollectionOf<PullRequest>(s => ValidateGetPullRequestsUrlWithQuery(s, query))
                .ReturnsAsync(new CollectionResponse<PullRequest> { Value = pullRequests })
                .Verifiable();

            var result = await _client.GetPullRequestsAsync(ProjectName, RepositoryName, query);

            _httpClientMock.Verify();

            result.Should().NotBeNull();
            result.Should().NotBeEmpty();
            result.Should().BeEquivalentTo(pullRequests);
        }

        [Test]
        public async Task GetPullRequestsFetchAllPages()
        {
            var page1 = new[] { new PullRequest(), new PullRequest() };
            var page2 = new[] { new PullRequest(), new PullRequest() };
            var page3 = new[] { new PullRequest() };

            SetupPagedGetCollectionOf<PullRequest>()
                .ReturnsAsync(new CollectionResponse<PullRequest> { Value = page1 })
                .ReturnsAsync(new CollectionResponse<PullRequest> { Value = page2 })
                .ReturnsAsync(new CollectionResponse<PullRequest> { Value = page3 })
                .ReturnsAsync(new CollectionResponse<PullRequest> { Value = Enumerable.Empty<PullRequest>()});

            var result = await _client.GetPullRequestsAsync(ProjectName, RepositoryName, null);

            result.Should().NotBeNull();
            result.Should().NotBeEmpty();
            result.Should().HaveCount(5);
            result.Should().BeEquivalentTo(page1.Union(page2).Union(page3));
        }

        private bool ValidateGetPullRequestsUrl(string url)
        {
            var expectedUrl = $"https://{InstanceName}.visualstudio.com/{ProjectName}/_apis/git/repositories/{RepositoryName}/pullrequests?api-version={Constants.CurrentWorkItemsApiVersion}";
            return string.Equals(expectedUrl, url, StringComparison.OrdinalIgnoreCase);
        }

        private bool ValidateGetPullRequestsUrlWithQuery(string url, PullRequestQuery query)
        {
            var uri = new Uri(url);
            var queryString = uri.Query;
            return queryString.Contains($"searchCriteria.status={query.Status}") &&
                queryString.Contains($"searchCriteria.reviewerId={query.ReviewerId}") &&
                queryString.Contains($"searchCriteria.creatorId={query.CreatorId}");
        }
    }
}
