using System;
using System.Collections.Generic;
using System.Diagnostics;
using VSTS.Net.Models.Common;
using VSTS.Net.Models.Identity;

namespace VSTS.Net.Models.PullRequests
{
    [DebuggerDisplay("{Status}-{PullRequestId} {Title}")]
    public class PullRequest : IEquatable<PullRequest>
    {
        /// <summary>
        /// The ID of the pull request.
        /// </summary>
        public int PullRequestId { get; set; }

        /// <summary>
        /// The status of the pull request.
        /// </summary>
        public string Status { get; set; }

        /// <summary>
        /// The identity of the user who created the pull request.
        /// </summary>
        public IdentityReference CreatedBy { get; set; }

        /// <summary>
        /// The date when the pull request was created.
        /// </summary>
        public DateTime CreationDate { get; set; }

        /// <summary>
        /// The date when the pull request was closed (completed, abandoned, or merged externally).
        /// </summary>
        public DateTime? ClosedDate { get; set; }

        /// <summary>
        /// The title of the pull request.
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// The description of the pull request.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// The name of the source branch of the pull request.
        /// </summary>
        public string SourceRefName { get; set; }

        /// <summary>
        /// The name of the target branch of the pull request.
        /// </summary>
        public string TargetRefName { get; set; }

        /// <summary>
        /// The current status of the pull request merge.
        /// </summary>
        public string MergeStatus { get; set; }

        /// <summary>
        /// The ID of the job used to run the pull request merge. Used internally.
        /// </summary>
        public string MergeId { get; set; }

        /// <summary>
        /// Url of the pull request
        /// </summary>
        public string Url { get; set; }

        /// <summary>
        /// The repository containing the target branch of the pull request.
        /// </summary>
        public Repository Repository { get; set; }

        /// <summary>
        /// A list of reviewers on the pull request along with the state of their votes.
        /// </summary>
        public IEnumerable<IdentityReferenceWithVote> Reviewers { get; set; }

        public virtual bool Equals(PullRequest other)
        {
            if (other == null)
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            return PullRequestId == other.PullRequestId;
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as PullRequest);
        }

        public override int GetHashCode()
        {
            return PullRequestId.GetHashCode();
        }
    }
}
