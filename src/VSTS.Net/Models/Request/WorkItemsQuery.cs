namespace VSTS.Net.Models.Request
{
    public class WorkItemsQuery
    {
        public WorkItemsQuery(string query, bool isHierarchical = false)
        {
            Query = query;
            IsHierarchical = isHierarchical;
        }

        public string Query { get; set; }

        public bool IsHierarchical { get; private set; }
    }
}
