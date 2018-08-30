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
    public class GetPullRequestIterationsTests : BaseHttpClientTests
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

            Client.Awaiting(c => c.GetPullRequestIterationsAsync(project, repository, 0))
                .Should().Throw<ArgumentNullException>();
        }

        [Test]
        public async Task ReturnsIterations()
        {
            var iterations = new[] { new PullRequestIteration(), new PullRequestIteration() };
            SetupOnePageOf(iterations);

            var result = await Client.GetPullRequestIterationsAsync(ProjectName, RepositoryName, 0, CancellationToken);

            result.Should().HaveCount(2);
            result.Should().BeSameAs(iterations);
        }

        [Test]
        public async Task ReturnEmptyCollectionIfResponseNull()
        {
            SetupGetCollectionOf<PullRequestIteration>()
                .ReturnsAsync((CollectionResponse<PullRequestIteration>)null);

            var result = await Client.GetPullRequestIterationsAsync(ProjectName, RepositoryName, 0, CancellationToken);

            result.Should().BeEmpty();
        }

        [Test]
        public async Task UrlIsCorrect()
        {
            const int pullRequestId = 124579;
            var iterations = new[] { new PullRequestIteration(), new PullRequestIteration() };
            SetupOnePageOf(iterations, u => VerifyUrl(u, pullRequestId));

            var result = await Client.GetPullRequestIterationsAsync(ProjectName, RepositoryName, pullRequestId, CancellationToken);

            VerifyPagedRequests<PullRequestIteration>(Times.Once());

            result.Should().HaveCount(2);
            result.Should().BeSameAs(iterations);
        }

        private bool VerifyUrl(string url, int pullRequestId)
        {
            var expectedUrl = $"https://{InstanceName}.visualstudio.com/{ProjectName}/_apis/git/repositories/{RepositoryName}/pullRequests/{pullRequestId}/iterations";
            return url.StartsWith(expectedUrl, StringComparison.OrdinalIgnoreCase);
        }
    }
}
