using System;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using VSTS.Net.Models.PullRequests;
using VSTS.Net.Models.Response;
using VSTS.Net.Tests.Types;

namespace VSTS.Net.Tests.PullRequests
{
    [TestFixture]
    public class GetPullRequestThreadsTests : BaseHttpClientTests
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

            Client.Awaiting(c => c.GetPullRequestThreadsAsync(project, repository, 0, CancellationToken))
                .Should().Throw<ArgumentNullException>();
        }

        [Test]
        public async Task ReturnsIterations()
        {
            var threads = new[] { new PullRequestThread(), new PullRequestThread() };
            SetupOnePageOf(threads);

            var result = await Client.GetPullRequestThreadsAsync(ProjectName, RepositoryName, 0, CancellationToken);

            result.Should().HaveCount(2);
            result.Should().BeSameAs(threads);
        }

        [Test]
        public async Task ReturnEmptyCollectionIfResponseNull()
        {
            SetupGetCollectionOf<PullRequestThread>()
                .ReturnsAsync((CollectionResponse<PullRequestThread>)null);

            var result = await Client.GetPullRequestThreadsAsync(ProjectName, RepositoryName, 0, CancellationToken);

            result.Should().BeEmpty();
        }

        [Test]
        public async Task UrlIsCorrect()
        {
            const int pullRequestId = 124579;
            var threads = new[] { new PullRequestThread(), new PullRequestThread() };
            SetupOnePageOf(threads, u => VerifyUrl(u, pullRequestId));

            var result = await Client.GetPullRequestThreadsAsync(ProjectName, RepositoryName, pullRequestId, CancellationToken);

            VerifyPagedRequests<PullRequestThread>(Times.Once());

            result.Should().HaveCount(2);
            result.Should().BeSameAs(threads);
        }

        private bool VerifyUrl(string url, int pullRequestId)
        {
            var expectedUrl = $"https://{InstanceName}.visualstudio.com/{ProjectName}/_apis/git/repositories/{RepositoryName}/pullRequests/{pullRequestId}/threads";
            return url.StartsWith(expectedUrl, StringComparison.OrdinalIgnoreCase);
        }
    }
}
