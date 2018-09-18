using System;
using System.Threading.Tasks;
using FluentAssertions;
using NUnit.Framework;
using VSTS.Net.Models.PullRequests;
using VSTS.Net.Tests.Types;

namespace VSTS.Net.Tests.PullRequests
{
    [TestFixture]
    public class GetPullRequestTests : BaseHttpClientTests
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

            Client.Awaiting(c => c.GetPullRequestAsync(project, repository, 0, CancellationToken))
                .Should().Throw<ArgumentNullException>();
        }

        [Test]
        public async Task GetPullRequestByIdRequestsCorrectUrl()
        {
            var pullRequest = new PullRequest { PullRequestId = 234567, Title = "Fooo" };
            SetupSingle(pullRequest, s => ValidateUrlWithId(s, pullRequest.PullRequestId));

            var pr = await Client.GetPullRequestAsync(pullRequest.PullRequestId, CancellationToken);

            pr.Should().NotBeNull();
            pr.PullRequestId.Should().Be(pullRequest.PullRequestId);
            pr.Title.Should().Be(pullRequest.Title);

            HttpClientMock.Verify();
        }

        [Test]
        public async Task GetPullRequestByProjectRequestsCorrectUrl()
        {
            var pullRequest = new PullRequest { PullRequestId = 234567, Title = "Fooo2" };
            SetupSingle(pullRequest, s => ValidateUrlWithProject(s, pullRequest.PullRequestId));

            var pr = await Client.GetPullRequestAsync(ProjectName, RepositoryName, pullRequest.PullRequestId, CancellationToken);

            pr.Should().NotBeNull();
            pr.PullRequestId.Should().Be(pullRequest.PullRequestId);
            pr.Title.Should().Be(pullRequest.Title);

            HttpClientMock.Verify();
        }

        private bool ValidateUrlWithProject(string url, int id)
        {
            var expectedUrl = $"https://{InstanceName}.visualstudio.com/{ProjectName}/_apis/git/repositories/{RepositoryName}/pullrequests/{id}?";
            return url.StartsWith(expectedUrl, StringComparison.OrdinalIgnoreCase);
        }

        private bool ValidateUrlWithId(string url, int id)
        {
            var expectedUrl = $"https://{InstanceName}.visualstudio.com/_apis/git/pullrequests/{id}?";
            return url.StartsWith(expectedUrl, StringComparison.OrdinalIgnoreCase);
        }
    }
}
