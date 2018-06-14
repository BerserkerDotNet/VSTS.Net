using System;
using VSTS.Net.Models.PullRequests;

namespace VSTS.Net.Models.Request
{
    public class PullRequestQuery
    {
        public static PullRequestQuery None => new PullRequestQuery();

        /// <summary>
        /// If set, search for pull requests that were created by this identity.
        /// </summary>
        public Guid? CreatorId { get; set; }

        /// <summary>
        /// If set, search for pull requests that have this identity as a reviewer.
        /// </summary>
        public Guid? ReviewerId { get; set; }

        /// <summary>
        /// If set, search for pull requests that are in this state.
        /// </summary>
        public string Status { get; set; }

        /// <summary>
        /// If set, search for pull requests that are created after specified date.
        /// </summary>
        public DateTime? CreatedAfter { get; set; }

        /// <summary>
        /// Custom filter to apply to retrieved pull requests.
        /// </summary>
        public Func<PullRequest, bool> CustomFilter { get; set; }
    }
}
