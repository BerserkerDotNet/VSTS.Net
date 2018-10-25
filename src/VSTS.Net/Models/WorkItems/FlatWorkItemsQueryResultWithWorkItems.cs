using System.Collections.Generic;

namespace VSTS.Net.Models.WorkItems
{
    public class FlatWorkItemsQueryResultWithWorkItems : WorkItemsQueryResult
    {
        public IEnumerable<WorkItem> WorkItems { get; set; }
    }
}