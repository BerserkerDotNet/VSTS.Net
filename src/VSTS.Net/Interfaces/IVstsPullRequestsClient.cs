using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using VSTS.Net.Models.PullRequests;
using VSTS.Net.Models.Request;

namespace VSTS.Net.Interfaces
{
    public interface IVstsPullRequestsClient
    {
        /// <summary>
        /// Retrieve all pull requests matching a specified criteria.
        /// </summary>
        /// <param name="project">Project ID or project name</param>
        /// <param name="repository">The repository ID of the pull request's target branch.</param>
        /// <param name="query">Filter criteria</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>List of pull requests matching a specified criteria.</returns>
        Task<IEnumerable<PullRequest>> GetPullRequestsAsync(string project, string repository, PullRequestQuery query, CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// Retrieve a pull request.
        /// </summary>
        /// <param name="project">Project ID or project name</param>
        /// <param name="repository">The repository ID of the pull request's target branch.</param>
        /// <param name="id">The ID of the pull request to retrieve.</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>An instance of the <see cref="PullRequest"></see> fetched from the server</returns>
        Task<PullRequest> GetPullRequestAsync(string project, string repository, int id, CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// Retrieve a pull request.
        /// </summary>
        /// <param name="id">The ID of the pull request to retrieve.</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns><An instance of the <see cref="PullRequest"></see> fetched from the server/returns>
        Task<PullRequest> GetPullRequestAsync(int id, CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// Get the list of iterations for the specified pull request.
        /// </summary>
        /// <param name="project">Project ID or project name</param>
        /// <param name="repository">The repository ID of the pull request's target branch.</param>
        /// <param name="pullRequestId">ID of the pull request.</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>List of pull request iterations</returns>
        Task<IEnumerable<PullRequestIteration>> GetPullRequestIterationsAsync(string project, string repository, int pullRequestId, CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// Retrieve all threads in a pull request.
        /// </summary>
        /// <param name="project">Project ID or project name</param>
        /// <param name="repository">The repository ID of the pull request's target branch.</param>
        /// <param name="pullRequestId">ID of the pull request.</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>List of pull request threads</returns>
        Task<IEnumerable<PullRequestThread>> GetPullRequestThreadsAsync(string project, string repository, int pullRequestId, CancellationToken cancellationToken = default(CancellationToken));
    }
}
