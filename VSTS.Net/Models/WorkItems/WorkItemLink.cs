using Newtonsoft.Json;

namespace VSTS.Net.Models.WorkItems
{
    public class WorkItemLink
    {
        public WorkItemLink(WorkItemReference source, WorkItemReference target, string relationship)
        {
            Relationship = relationship;
            Source = source;
            Target = target;
        }

        /// <summary>
        /// The type of link.
        /// </summary>
        [JsonProperty("rel")]
        public string Relationship { get; set; }

        /// <summary>
        /// The source work item.
        /// </summary>
        public WorkItemReference Source { get; set; }

        /// <summary>
        /// The target work item.
        /// </summary>
        public WorkItemReference Target { get; set; }
    }
}
