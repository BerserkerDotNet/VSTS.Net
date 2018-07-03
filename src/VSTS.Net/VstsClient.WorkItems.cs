using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using VSTS.Net.Interfaces;
using VSTS.Net.Models.Request;
using VSTS.Net.Models.Response;
using VSTS.Net.Models.WorkItems;
using VSTS.Net.Types;
using static VSTS.Net.Utils.NullCheckUtility;

namespace VSTS.Net
{
    public partial class VstsClient : IVstsClient
    {
        /// <inheritdoc />
        public async Task<WorkItemsQueryResult> ExecuteQueryAsync(string project, WorkItemsQuery query, CancellationToken cancellationToken = default(CancellationToken))
        {
            ThrowIfArgumentNullOrEmpty(project);
            ThrowIfArgumentNull(query, nameof(query));
            ThrowIfArgumentNullOrEmpty(_instanceName, nameof(_instanceName));
            ThrowIfArgumentNullOrEmpty(query.Query, "Query cannot be empty.");

            var url = VstsUrlBuilder.Create(_instanceName)
                .ForWIQL(project)
                .Build(Constants.CurrentWorkItemsApiVersion);

            _logger.LogDebug("Requesting {0}", url);

            if (query.IsHierarchical)
                return await _httpClient.ExecutePost<HierarchicalWorkItemsQueryResult>(url, query, cancellationToken);

            return await _httpClient.ExecutePost<FlatWorkItemsQueryResult>(url, query, cancellationToken);
        }

        /// <inheritdoc />
        public async Task<IEnumerable<WorkItem>> GetWorkItemsAsync(string project, WorkItemsQuery query, CancellationToken cancellationToken = default(CancellationToken))
        {
            var queryResult = await ExecuteQueryAsync(project, query, cancellationToken);
            int[] ids;
            switch (queryResult)
            {
                case FlatWorkItemsQueryResult flat:
                    ids = GetWorkitemIdsFromQuery(flat);
                    break;
                case HierarchicalWorkItemsQueryResult tree:
                    ids = GetWorkitemIdsFromQuery(tree);
                    break;
                default:
                    throw new NotSupportedException($"Query result is of not supported type.");
            }

            if (!ids.Any())
                return Enumerable.Empty<WorkItem>();

            var fieldsString = string.Join(",", queryResult.Columns.Select(c => c.ReferenceName));
            var idsString = string.Join(",", ids);
            var url = VstsUrlBuilder.Create(_instanceName)
                .ForWorkItemsBatch(idsString, project)
                .WithQueryParameter("fields", fieldsString)
                .Build();

            var workitemsResponse = await _httpClient.ExecuteGet<CollectionResponse<WorkItem>>(url, cancellationToken);

            if (workitemsResponse == null)
                return Enumerable.Empty<WorkItem>();

            return workitemsResponse.Value;
        }

        /// <inheritdoc />
        public async Task<IEnumerable<WorkItemUpdate>> GetWorkItemUpdatesAsync(int workitemId, CancellationToken cancellationToken = default(CancellationToken))
        {
            var url = VstsUrlBuilder.Create(_instanceName)
                .ForWorkItems(workitemId)
                .WithSection("updates")
                .Build();

            var result = await _httpClient.ExecuteGet<CollectionResponse<WorkItemUpdate>>(url, cancellationToken);
            if (result == null)
                return Enumerable.Empty<WorkItemUpdate>();

            return result.Value;
        }

        /// <inheritdoc />
        public async Task<WorkItem> GetWorkItemAsync(string project, int workItemId, DateTime? asOf = null, string[] fields = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            ThrowIfArgumentNullOrEmpty(project, nameof(project));

            var fieldsString = fields != null ? string.Join(",", fields) : string.Empty;
            var asOfString = asOf.HasValue ? asOf.Value.ToString("u") : string.Empty;

            var url = VstsUrlBuilder.Create(_instanceName)
                .WithSection(project)
                .ForWorkItems(workItemId)
                .WithQueryParameterIfNotEmpty("fields", fieldsString)
                .WithQueryParameterIfNotEmpty("asOf", asOfString)
                .Build();

            return await _httpClient.ExecuteGet<WorkItem>(url, cancellationToken);
        }

        /// <inheritdoc />
        public async Task<IEnumerable<WorkItem>> GetWorkItemsAsync(string project, int[] ids, DateTime? asOf = null, string[] fields = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            ThrowIfArgumentNullOrEmpty(project, nameof(project));
            ThrowIfArgumentNull(ids, nameof(ids));

            if (!ids.Any())
                return Enumerable.Empty<WorkItem>();

            var fieldsString = fields != null ? string.Join(",", fields) : string.Empty;
            var asOfString = asOf.HasValue ? asOf.Value.ToString("u") : string.Empty;
            var idsString = string.Join(",", ids);
            var url = VstsUrlBuilder.Create(_instanceName)
                .ForWorkItemsBatch(idsString, project)
                .WithQueryParameterIfNotEmpty("fields", fieldsString)
                .WithQueryParameterIfNotEmpty("asOf", asOfString)
                .Build();

            var response = await _httpClient.ExecuteGet<CollectionResponse<WorkItem>>(url, cancellationToken);
            return response?.Value ?? Enumerable.Empty<WorkItem>();
        }

        /// <inheritdoc />
        public async Task<bool> DeleteWorkItemAsync(string project, int id, bool destroy = false, CancellationToken cancellationToken = default(CancellationToken))
        {
            ThrowIfArgumentNullOrEmpty(project, nameof(project));

            var url = VstsUrlBuilder.Create(_instanceName)
                    .WithSection(project)
                    .ForWorkItems(id)
                    .WithQueryParameterIfNotDefault("destroy", destroy)
                    .Build();

            await _httpClient.ExecuteDelete<WorkItemDeleteResponse>(url, cancellationToken);
            return true;
        }

        /// <inheritdoc />
        public async Task<WorkItem> CreateWorkItemAsync(string project, string type, WorkItem item, CancellationToken cancellationToken = default(CancellationToken))
        {
            ThrowIfArgumentNullOrEmpty(project, nameof(project));
            ThrowIfArgumentNullOrEmpty(type, nameof(type));
            ThrowIfArgumentNull(item, nameof(item));

            var updateRequest = new UpdateWorkitemRequest();
            foreach (var field in item.Fields)
            {
                updateRequest.AddFieldValue(field.Key, field.Value);
            }

            var url = VstsUrlBuilder.Create(_instanceName)
               .WithSection(project)
               .ForWorkItems()
               .WithSection($"${type}")
               .Build();

            var result = await _httpClient.ExecutePost<WorkItem>(url, updateRequest.Updates.ToArray(), Constants.JsonPatchMimeType, cancellationToken);
            return result;
        }

        /// <inheritdoc />
        public async Task<WorkItem> UpdateWorkItemAsync(string project, UpdateWorkitemRequest request, CancellationToken cancellationToken = default(CancellationToken))
        {
            ThrowIfArgumentNullOrEmpty(project, nameof(project));
            ThrowIfArgumentNull(request, nameof(request));
            ThrowIfArgumentNull(request.Id, nameof(request.Id));

            var url = VstsUrlBuilder.Create(_instanceName)
               .WithSection(project)
               .ForWorkItems()
               .WithSection(request.Id.Value.ToString())
               .Build();

            var result = await _httpClient.ExecutePatch<WorkItem>(url, request.Updates.ToArray(), Constants.JsonPatchMimeType, cancellationToken);
            return result;
        }

        private int[] GetWorkitemIdsFromQuery(FlatWorkItemsQueryResult query)
        {
            return query.WorkItems.Select(w => w.Id).ToArray();
        }

        private int[] GetWorkitemIdsFromQuery(HierarchicalWorkItemsQueryResult query)
        {
            var targetIds = query.WorkItemRelations.Select(w => w.Target.Id);
            var sourceIds = query.WorkItemRelations.Select(w => w.Source.Id);
            return sourceIds.Union(targetIds)
                .Distinct()
                .ToArray();
        }


    }
}
