using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace VSTS.Net.Models.Response.WorkItems
{
    public class WorkItem
    {
        /// <summary>
        /// The work item ID.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Revision number of the work item.
        /// </summary>
        [JsonProperty("rev")]
        public int Revision { get; set; }

        /// <summary>
        /// Map of field and values for the work item.
        /// </summary>
        public Dictionary<string, string> Fields { get; set; }

        /// <summary>
        /// API link to the work item
        /// </summary>
        public Uri Url {get; set; }

        /// <summary>
        /// Relations of the work item.
        /// </summary>
        public IEnumerable<WorkItemRelation> Relations { get; set; }
    }
}
