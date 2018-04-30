using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using VSTS.Net.Interfaces;
using VSTS.Net.Models.Request;
using VSTS.Net.Models.Response.WorkItems;
using VSTS.Net.Types;
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

        /// <inheritdoc />
        public async Task<WorkItemsQueryResult> ExecuteQueryAsync(string project, WorkItemsQuery query)
        {
            ThrowIfArgumentNullOrEmpty(project);
            ThrowIfArgumentNull(query, nameof(query));
            ThrowIfArgumentNullOrEmpty(_instanceName, nameof(_instanceName));
            ThrowIfArgumentNullOrEmpty(query.Query, "Query cannot be empty.");

            var url = VstsUrlBuilder.Create(_instanceName)
                .ForWIQL(project)
                .Build(Constants.CurrentWorkItemsApiVersion);

            if (query.IsHierarchical)
                return await _httpClient.ExecutePost<HierarchicalWorkItemsQueryResult>(url, query);

            return await _httpClient.ExecutePost<FlatWorkItemsQueryResult>(url, query);
        }

        /// <inheritdoc />
        public Task<IEnumerable<WorkItem>> GetWorkItemsAsync(string project, WorkItemsQuery query)
        {
            throw new NotImplementedException();
        }
    }
}
