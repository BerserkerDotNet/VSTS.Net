using System;

namespace VSTS.Net.Models.WorkItems
{
    public class WorkItemReference : IEquatable<WorkItemReference>
    {
        public WorkItemReference(int id)
        {
            Id = id;
        }

        /// <summary>
        /// Work item ID.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// REST API URL of the resource
        /// </summary>
        public Uri Url { get; set; }

        public bool Equals(WorkItemReference other)
        {
            if (ReferenceEquals(this, other))
            {
                return true;
            }

            if (other == null)
            {
                return false;
            }

            return Id == other.Id;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            return base.Equals(obj as WorkItemReference);
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }
    }
}
