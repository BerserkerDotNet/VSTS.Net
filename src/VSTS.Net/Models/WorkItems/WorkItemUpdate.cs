using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using VSTS.Net.Models.Identity;

namespace VSTS.Net.Models.WorkItems
{
    public class WorkItemUpdate
    {
        /// <summary>
        /// ID of update.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// The work item ID.
        /// </summary>
        public int WorkItemId { get; set; }

        /// <summary>
        /// The revision number of work item update.
        /// </summary>
        [JsonProperty("rev")]
        public int Revision { get; set; }

        /// <summary>
        /// List of updates to fields.
        /// </summary>
        public Dictionary<string, WorkItemFieldUpdate> Fields { get; set; }

        /// <summary>
        /// The work item updates revision date.
        /// </summary>
        public DateTime RevisedDate { get; set; }

        /// <summary>
        /// Identity for the work item update.
        /// </summary>
        public IdentityReference RevisedBy { get; set; }

        /// <summary>
        /// Link to the work item update
        /// </summary>
        public string Url { get; set; }

        public WorkItemFieldUpdate this[string key]
        {
            get
            {
                if (string.IsNullOrEmpty(key) || Fields == null || !Fields.ContainsKey(key))
                {
                    return new WorkItemFieldUpdate();
                }

                return Fields[key];
            }
        }
    }
}
