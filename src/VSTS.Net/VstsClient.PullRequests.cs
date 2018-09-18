using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using VSTS.Net.Interfaces;
using VSTS.Net.Models.PullRequests;
using VSTS.Net.Models.Request;
using VSTS.Net.Models.Response;
using VSTS.Net.Types;
using static VSTS.Net.Utils.NullCheckUtility;

namespace VSTS.Net
{
    public partial class VstsClient : IVstsClient
    {
        /// <inheritdoc />
        public async Task<IEnumerable<PullRequest>> GetPullRequestsAsync(string project, string repository, PullRequestQuery query, CancellationToken cancellationToken = default(CancellationToken))
        {
            ThrowIfArgumentNullOrEmpty(project, nameof(project));
            ThrowIfArgumentNullOrEmpty(repository, nameof(repository));

            if (query == null)
            {
                query = PullRequestQuery.None;
            }

            var haveMorePullRequests = true;
            var skip = 0;
            var allPullRequests = new List<PullRequest>();
            while (haveMorePullRequests)
            {
                var url = _urlBuilderFactory.Create()
                    .ForPullRequests(project, repository)
                    .WithQueryParameterIfNotEmpty("searchCriteria.status", query.Status)
                    .WithQueryParameterIfNotEmpty("searchCriteria.reviewerId", query.ReviewerId)
                    .WithQueryParameterIfNotEmpty("searchCriteria.creatorId", query.CreatorId)
                    .WithQueryParameter("$skip", skip)
                    .Build();

                var pullRequestsResponse = await _httpClient.ExecuteGet<CollectionResponse<PullRequest>>(url, cancellationToken);
                var pullRequests = pullRequestsResponse?.Value ?? Enumerable.Empty<PullRequest>();

                skip += pullRequests.Count();
                haveMorePullRequests = pullRequests.Any() && (!query.CreatedAfter.HasValue || pullRequests.Min(p => p.CreationDate) >= query.CreatedAfter);

                if (query.CreatedAfter.HasValue)
                {
                    pullRequests = pullRequests.Where(p => p.CreationDate >= query.CreatedAfter);
                }

                if (query.CustomFilter != null)
                {
                    pullRequests = pullRequests.Where(query.CustomFilter);
                }

                allPullRequests.AddRange(pullRequests);
            }

            return allPullRequests;
        }

        /// <inheritdoc />
        public async Task<PullRequest> GetPullRequestAsync(string project, string repository, int id, CancellationToken cancellationToken = default(CancellationToken))
        {
            ThrowIfArgumentNullOrEmpty(project, nameof(project));
            ThrowIfArgumentNullOrEmpty(repository, nameof(repository));

            var url = _urlBuilderFactory.Create()
                .ForPullRequests(project, repository)
                .WithSection(id.ToString())
                .Build();

            return await _httpClient.ExecuteGet<PullRequest>(url, cancellationToken);
        }

        /// <inheritdoc />
        public async Task<PullRequest> GetPullRequestAsync(int id, CancellationToken cancellationToken = default(CancellationToken))
        {
            var url = _urlBuilderFactory.Create()
                .ForPullRequestId(id)
                .Build();

            return await _httpClient.ExecuteGet<PullRequest>(url, cancellationToken);
        }

        /// <inheritdoc />
        public async Task<IEnumerable<PullRequestIteration>> GetPullRequestIterationsAsync(string project, string repository, int pullRequestId, CancellationToken cancellationToken = default(CancellationToken))
        {
            ThrowIfArgumentNullOrEmpty(project, nameof(project));
            ThrowIfArgumentNullOrEmpty(repository, nameof(repository));

            var iterationsUrl = _urlBuilderFactory.Create()
              .ForPullRequests(project, repository)
              .WithSection(pullRequestId.ToString())
              .WithSection("iterations")
              .Build();

            var iterationsResponse = await _httpClient.ExecuteGet<CollectionResponse<PullRequestIteration>>(iterationsUrl, cancellationToken);
            return iterationsResponse?.Value ?? Enumerable.Empty<PullRequestIteration>();
        }

        /// <inheritdoc />
        public async Task<IEnumerable<PullRequestThread>> GetPullRequestThreadsAsync(string project, string repository, int pullRequestId, CancellationToken cancellationToken = default(CancellationToken))
        {
            ThrowIfArgumentNullOrEmpty(project, nameof(project));
            ThrowIfArgumentNullOrEmpty(repository, nameof(repository));

            var threadsUrl = _urlBuilderFactory.Create()
              .ForPullRequests(project, repository)
              .WithSection(pullRequestId.ToString())
              .WithSection("threads")
              .Build();

            var threadsResponse = await _httpClient.ExecuteGet<CollectionResponse<PullRequestThread>>(threadsUrl, cancellationToken);
            return threadsResponse?.Value ?? Enumerable.Empty<PullRequestThread>();
        }
    }
}
