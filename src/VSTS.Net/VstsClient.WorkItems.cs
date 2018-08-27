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
        public async Task<WorkItemsQueryResult> ExecuteQueryAsync(WorkItemsQuery query, CancellationToken cancellationToken = default(CancellationToken))
        {
            ThrowIfArgumentNull(query, nameof(query));
            ThrowIfArgumentNullOrEmpty(query.Query, "Query cannot be empty.");

            var url = _urlBuilderFactory.Create()
                .ForWIQL()
                .Build(Constants.CurrentWorkItemsApiVersion);

            _logger.LogDebug("Requesting {0}", url);

            if (query.IsHierarchical)
                return await _httpClient.ExecutePost<HierarchicalWorkItemsQueryResult>(url, query, cancellationToken);

            return await _httpClient.ExecutePost<FlatWorkItemsQueryResult>(url, query, cancellationToken);
        }

        /// <inheritdoc />
        public async Task<IEnumerable<WorkItem>> GetWorkItemsAsync(WorkItemsQuery query, CancellationToken cancellationToken = default(CancellationToken))
        {
            var queryResult = await ExecuteQueryAsync(query, cancellationToken);
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

            var columns = queryResult.Columns.Select(c => c.ReferenceName).ToArray();
            return await GetWorkItemsAsync(ids, fields: columns, cancellationToken: cancellationToken);
        }

        /// <inheritdoc />
        public async Task<IEnumerable<WorkItemUpdate>> GetWorkItemUpdatesAsync(int workitemId, CancellationToken cancellationToken = default(CancellationToken))
        {
            var url = _urlBuilderFactory.Create()
                .ForWorkItems(workitemId)
                .WithSection("updates")
                .Build();

            var result = await _httpClient.ExecuteGet<CollectionResponse<WorkItemUpdate>>(url, cancellationToken);
            if (result == null)
                return Enumerable.Empty<WorkItemUpdate>();

            return result.Value;
        }

        /// <inheritdoc />
        public async Task<WorkItem> GetWorkItemAsync(int workItemId, DateTime? asOf = null, string[] fields = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            var fieldsString = fields != null ? string.Join(",", fields) : string.Empty;
            var asOfString = asOf.HasValue ? asOf.Value.ToString("u") : string.Empty;

            var url = _urlBuilderFactory.Create()
                .ForWorkItems(workItemId)
                .WithQueryParameterIfNotEmpty("fields", fieldsString)
                .WithQueryParameterIfNotEmpty("asOf", asOfString)
                .Build();

            return await _httpClient.ExecuteGet<WorkItem>(url, cancellationToken);
        }

        /// <inheritdoc />
        public async Task<IEnumerable<WorkItem>> GetWorkItemsAsync(int[] ids, DateTime? asOf = null, string[] fields = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            ThrowIfArgumentNull(ids, nameof(ids));

            if (!ids.Any())
                return Enumerable.Empty<WorkItem>();

            var result = new List<WorkItem>(ids.Length);
            var fieldsString = fields != null ? string.Join(",", fields) : string.Empty;
            var asOfString = asOf.HasValue ? asOf.Value.ToString("u") : string.Empty;
            var batchSize = Configuration.WorkitemsBatchSize;
            var batches = Math.Ceiling((decimal)ids.Length / batchSize);
            for (int i = 0; i < batches; i++)
            {
                var idsString = string.Join(",", ids.Skip(i * batchSize).Take(batchSize));
                var url = _urlBuilderFactory.Create()
                    .ForWorkItemsBatch(idsString)
                    .WithQueryParameterIfNotEmpty("fields", fieldsString)
                    .WithQueryParameterIfNotEmpty("asOf", asOfString)
                    .Build();

                var response = await _httpClient.ExecuteGet<CollectionResponse<WorkItem>>(url, cancellationToken);
                result.AddRange(response?.Value ?? Enumerable.Empty<WorkItem>());
            }

            return result;
        }

        /// <inheritdoc />
        public async Task<bool> DeleteWorkItemAsync(int id, bool destroy = false, CancellationToken cancellationToken = default(CancellationToken))
        {
            var url = _urlBuilderFactory.Create()
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

            var url = _urlBuilderFactory.Create()
               .WithSection(project)
               .ForWorkItems()
               .WithSection($"${type}")
               .Build();

            var result = await _httpClient.ExecutePost<WorkItem>(url, updateRequest.Updates.ToArray(), Constants.JsonPatchMimeType, cancellationToken);
            return result;
        }

        /// <inheritdoc />
        public async Task<WorkItem> UpdateWorkItemAsync(UpdateWorkitemRequest request, CancellationToken cancellationToken = default(CancellationToken))
        {
            ThrowIfArgumentNull(request, nameof(request));
            ThrowIfArgumentNull(request.Id, nameof(request.Id));

            var url = _urlBuilderFactory.Create()
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
