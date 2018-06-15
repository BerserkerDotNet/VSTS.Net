using System;
using System.Collections.Generic;

namespace VSTS.Net.Models.PullRequests
{
    /// <summary>
    /// Represents a comment thread of a pull request. 
    /// </summary>
    public class PullRequestThread
    {
        /// <summary>
        /// The comment thread id.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Specify if the thread is deleted which happens when all comments are deleted.
        /// </summary>
        public bool IsDeleted { get; set; }

        /// <summary>
        /// The time this thread was last updated.
        /// </summary>
        public DateTime LastUpdatedDate { get; set; }

        /// <summary>
        /// The time this thread was published.
        /// </summary>
        public DateTime PublishedDate { get; set; }

        /// <summary>
        /// The status of the comment thread.
        /// </summary>
        public string Status { get; set; }

        /// <summary>
        /// A list of the comments.
        /// </summary>
        public IEnumerable<PullRequestComment> Comments { get; set; }
    }
}
