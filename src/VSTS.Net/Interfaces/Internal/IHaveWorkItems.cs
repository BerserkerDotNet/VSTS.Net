using System.Collections.Generic;
using VSTS.Net.Models.WorkItems;

namespace VSTS.Net.Interfaces.Internal
{
    internal interface IHaveWorkItems
    {
        IEnumerable<WorkItem> WorkItems { get; set; }
    }
}
