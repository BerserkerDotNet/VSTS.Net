using System.Collections.Generic;
using System.Linq;
using VSTS.Net.Models.WorkItems;

namespace VSTS.Net.Extensions
{
    public static class WorkItemLinkExtensions
    {
        public static int[] ToIdsArray(this IEnumerable<WorkItemLink> links)
        {
            return links.SelectMany(r => new[] { r.Source, r.Target })
                    .Select(r => r.Id)
                    .Distinct()
                    .ToArray();
        }
    }
}
