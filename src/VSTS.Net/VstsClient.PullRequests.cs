using System.Collections.Generic;
using System.Linq;
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
        public async Task<IEnumerable<PullRequest>> GetPullRequestsAsync(string project, string repository, PullRequestQuery query)
        {
            ThrowIfArgumentNullOrEmpty(project, nameof(project));
            ThrowIfArgumentNullOrEmpty(repository, nameof(repository));

            if (query == null)
                query = PullRequestQuery.None;

            var haveMorePullRequests = true;
            var skip = 0;
            var allPullRequests = new List<PullRequest>();
            while (haveMorePullRequests)
            {
                var url = VstsUrlBuilder.Create(_instanceName)
                    .ForPullRequests(project, repository)
                    .WithQueryParameterIfNotEmpty("searchCriteria.status", query.Status)
                    .WithQueryParameterIfNotEmpty("searchCriteria.reviewerId", query.ReviewerId)
                    .WithQueryParameterIfNotEmpty("searchCriteria.creatorId", query.CreatorId)
                    .WithQueryParameter("$skip", skip)
                    .Build();

                var pullRequestsResponse = await _httpClient.ExecuteGet<CollectionResponse<PullRequest>>(url);
                var pullRequests = pullRequestsResponse?.Value ?? Enumerable.Empty<PullRequest>();

                haveMorePullRequests = pullRequests.Any() && (!query.CreatedAfter.HasValue || pullRequests.Min(p => p.CreationDate) >= query.CreatedAfter);

                if (query.CreatedAfter.HasValue)
                    pullRequests = pullRequests.Where(p => p.CreationDate >= query.CreatedAfter);

                if (query.CustomFilter != null)
                    pullRequests = pullRequests.Where(query.CustomFilter);

                allPullRequests.AddRange(pullRequests);
                
                skip = allPullRequests.Count;
            }

            return allPullRequests;
        }

        /// <inheritdoc />
        public async Task<IEnumerable<PullRequestIteration>> GetPullRequestIterationsAsync(string project, string repository, int pullRequestId)
        {
            ThrowIfArgumentNullOrEmpty(project, nameof(project));
            ThrowIfArgumentNullOrEmpty(repository, nameof(repository));

            var iterationsUrl = VstsUrlBuilder.Create(_instanceName)
              .ForPullRequests(project, repository)
              .WithSection(pullRequestId.ToString())
              .WithSection("iterations")
              .Build();

            var iterationsResponse = await _httpClient.ExecuteGet<CollectionResponse<PullRequestIteration>>(iterationsUrl);
            return iterationsResponse?.Value ?? Enumerable.Empty<PullRequestIteration>();
        }

        /// <inheritdoc />
        public async Task<IEnumerable<PullRequestThread>> GetPullRequestThreadsAsync(string project, string repository, int pullRequestId)
        {
            ThrowIfArgumentNullOrEmpty(project, nameof(project));
            ThrowIfArgumentNullOrEmpty(repository, nameof(repository));

            var threadsUrl = VstsUrlBuilder.Create(_instanceName)
              .ForPullRequests(project, repository)
              .WithSection(pullRequestId.ToString())
              .WithSection("threads")
              .Build();

            var threadsResponse = await _httpClient.ExecuteGet<CollectionResponse<PullRequestThread>>(threadsUrl);
            return threadsResponse?.Value ?? Enumerable.Empty<PullRequestThread>();
        }
    }
}
