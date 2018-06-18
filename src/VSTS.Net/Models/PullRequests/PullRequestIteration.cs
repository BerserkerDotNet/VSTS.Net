using System;
using VSTS.Net.Models.Identity;

namespace VSTS.Net.Models.PullRequests
{
    /// <summary>
    /// Provides properties that describe a pull request iteration. Iterations are created as a result of creating and pushing updates to a pull request
    /// </summary>
    public class PullRequestIteration
    {
        /// <summary>
        /// ID of the pull request iteration.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Author of the pull request iteration.
        /// </summary>
        public IdentityReference Author { get; set; }

        /// <summary>
        /// The creation date of the pull request iteration.
        /// </summary>
        public DateTime CreatedDate { get; set; }

        /// <summary>
        /// Description of the pull request iteration.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// The updated date of the pull request iteration.
        /// </summary>
        public DateTime UpdatedDate { get; set; }
    }
}
