using System.Collections.Generic;
using VSTS.Net.Interfaces.Internal;

namespace VSTS.Net.Models.WorkItems
{
    public class HierarchicalWorkItemsQueryResultWithWorkItems : HierarchicalWorkItemsQueryResult, IHaveWorkItems, IHaveColumns
    {
        public IEnumerable<WorkItem> WorkItems { get; set; }
    }
}