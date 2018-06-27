using System;

namespace VSTS.Net.Models.Response
{
    /// <summary>
    /// Workitem deletion result
    /// </summary>
    public class WorkItemDeleteResponse
    {
        /// <summary>
        /// Work item ID.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// The HTTP status code for work item operation in a batch request.
        /// </summary>
        public int Code { get; set; }

        /// <summary>
        /// The user who deleted the work item type.
        /// </summary>
        public string DeletedBy { get; set; }

        /// <summary>
        /// The work item deletion date.
        /// </summary>
        public DateTime DeletedDate { get; set; }
    }
}
