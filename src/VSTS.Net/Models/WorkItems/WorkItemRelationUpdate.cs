using System.Collections.Generic;

namespace VSTS.Net.Models.WorkItems
{
    public class WorkItemRelationUpdate
    {
        public IEnumerable<WorkItemRelation> Added { get; set; }

        public IEnumerable<WorkItemRelation> Removed { get; set; }

        public IEnumerable<WorkItemRelation> Updated { get; set; }
    }
}
