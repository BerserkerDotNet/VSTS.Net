using System;

namespace VSTS.Net.Models.Response.WorkItems
{
    public class WorkItemReference
    {
        /// <summary>
        /// Work item ID.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// REST API URL of the resource
        /// </summary>
        public Uri Url { get; set; }
    }
}
