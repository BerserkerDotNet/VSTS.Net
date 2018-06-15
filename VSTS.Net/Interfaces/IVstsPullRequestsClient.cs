using System.Collections.Generic;
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
        /// <returns>List of pull requests matching a specified criteria.</returns>
        Task<IEnumerable<PullRequest>> GetPullRequestsAsync(string project, string repository, PullRequestQuery query);

        /// <summary>
        /// Get the list of iterations for the specified pull request.
        /// </summary>
        /// <param name="project">Project ID or project name</param>
        /// <param name="repository">The repository ID of the pull request's target branch.</param>
        /// <param name="pullRequestId">ID of the pull request.</param>
        /// <returns></returns>
        Task<IEnumerable<PullRequestIteration>> GetPullRequestIterationsAsync(string project, string repository, int pullRequestId);

        /// <summary>
        /// Retrieve all threads in a pull request.
        /// </summary>
        /// <param name="project">Project ID or project name</param>
        /// <param name="repository">The repository ID of the pull request's target branch.</param>
        /// <param name="pullRequestId">ID of the pull request.</param>
        /// <returns></returns>
        Task<IEnumerable<PullRequestThread>> GetPullRequestThreadsAsync(string project, string repository, int pullRequestId);
    }
}
