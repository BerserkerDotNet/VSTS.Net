using System;
using VSTS.Net.Models.Identity;

namespace VSTS.Net.Models.PullRequests
{
    /// <summary>
    /// Represents a comment which is one of potentially many in a comment thread.
    /// </summary>
    public class PullRequestComment
    {
        /// <summary>
        /// The comment ID. IDs start at 1 and are unique to a pull request.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// The ID of the parent comment. This is used for replies.
        /// </summary>
        public int? ParentCommentId { get; set; }

        /// <summary>
        /// Whether or not this comment was soft-deleted.
        /// </summary>
        public bool IsDeleted { get; set; }

        /// <summary>
        /// The author of the comment.
        /// </summary>
        public IdentityReferenceWithVote Author { get; set; }

        /// <summary>
        /// The comment type at the time of creation.
        /// </summary>
        public string CommentType { get; set; }

        /// <summary>
        /// The comment content.
        /// </summary>
        public string Content { get; set; }

        /// <summary>
        /// The date the comment's content was last updated.
        /// </summary>
        public DateTime LastContentUpdatedDate { get; set; }

        /// <summary>
        /// The date the comment was last updated.
        /// </summary>
        public DateTime LastUpdatedDate { get; set; }

        /// <summary>
        /// The date the comment was first published.
        /// </summary>
        public DateTime PublishedDate { get; set; }
    }
}
