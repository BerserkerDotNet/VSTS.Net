using System;

namespace VSTS.Net.Models.Request
{
    public class WorkItemsQuery
    {
        [Obsolete("Use WorkItemsQuery.Get instead", error: false)]
        public WorkItemsQuery(string query, bool isHierarchical = false)
        {
            Query = query;
            IsHierarchical = isHierarchical;
        }

        public string Query { get; private set; }

        public bool IsHierarchical { get; private set; }

        public static WorkItemsQuery Get(string query, bool isHierarchical = false)
        {
#pragma warning disable CS0618 // Type or member is obsolete
            return new WorkItemsQuery(query, isHierarchical);
#pragma warning restore CS0618 // Type or member is obsolete
        }
    }
}
