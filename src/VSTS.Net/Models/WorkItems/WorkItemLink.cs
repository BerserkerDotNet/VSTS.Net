using System;
using Newtonsoft.Json;

namespace VSTS.Net.Models.WorkItems
{
    public class WorkItemLink : IEquatable<WorkItemLink>
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

        public bool Equals(WorkItemLink other)
        {
            if (ReferenceEquals(this, other))
            {
                return true;
            }

            if (other == null)
            {
                return false;
            }

            return (Target, Source, Relationship).Equals((other.Target, other.Source, other.Relationship));
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            return Equals(obj as WorkItemLink);
        }

        public override int GetHashCode()
        {
            return (Target, Source, Relationship).GetHashCode();
        }
    }
}
