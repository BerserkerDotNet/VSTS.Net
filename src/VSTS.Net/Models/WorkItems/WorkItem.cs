using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace VSTS.Net.Models.WorkItems
{
    public class WorkItem : IEquatable<WorkItem>
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

        public virtual bool Equals(WorkItem other)
        {
            if (other == null)
                return false;

            if (ReferenceEquals(this, other))
                return true;

            return Id == other.Id;
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as WorkItem);
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }
    }
}
