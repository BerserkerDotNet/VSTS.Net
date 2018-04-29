using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using VSTS.Net.Interfaces;
using VSTS.Net.Models.Request;
using VSTS.Net.Models.Response.WorkItems;
using static VSTS.Net.Utils.NullCheckUtility;

namespace VSTS.Net
{
    public class VstsClient : IVstsWorkItemsClient
    {
        private readonly string _instanceName;
        private readonly IHttpClient _httpClient;

        public VstsClient(string instanceName, IHttpClient client)
        {
            _instanceName = instanceName;
            _httpClient = client;
        }

        public async Task<WorkItemsQueryResult> ExecuteQueryAsync(WorkItemsQuery query)
        {
            ThrowIfArgumentNull(query, nameof(query));
            ThrowIfNullOrEmpty(_instanceName, nameof(_instanceName));
            ThrowIfNullOrEmpty(query.Query, "Query cannot be empty.");

            if (query.IsHierarchical)
                return await _httpClient.ExecutePost<HierarchicalWorkItemsQueryResult>(string.Empty, query);

            return await _httpClient.ExecutePost<FlatWorkItemsQueryResult>(string.Empty, query);
        }

        public Task<IEnumerable<WorkItem>> GetWorkItemsAsync(WorkItemsQuery query)
        {
            throw new NotImplementedException();
        }
    }
}
