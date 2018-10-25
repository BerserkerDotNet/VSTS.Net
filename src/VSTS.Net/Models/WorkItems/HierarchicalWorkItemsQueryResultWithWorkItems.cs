using System.Collections.Generic;

namespace VSTS.Net.Models.WorkItems
{
    public class HierarchicalWorkItemsQueryResultWithWorkItems : HierarchicalWorkItemsQueryResult
    {
        public IEnumerable<WorkItem> WorkItems { get; set; }
    }
}