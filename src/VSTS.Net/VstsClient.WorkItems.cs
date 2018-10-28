using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using VSTS.Net.Exceptions;
using VSTS.Net.Extensions;
using VSTS.Net.Interfaces;
using VSTS.Net.Interfaces.Internal;
using VSTS.Net.Models.Request;
using VSTS.Net.Models.Response;
using VSTS.Net.Models.WorkItems;
using VSTS.Net.Types;
using static VSTS.Net.Utils.NullCheckUtility;

namespace VSTS.Net
{
    public partial class VstsClient : IVstsClient
    {
        private const string ExpandAllWorkItemFieldsParameterValue = "All";

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
            {
                return await _httpClient.ExecutePost<HierarchicalWorkItemsQueryResult>(url, query, cancellationToken);
            }

            return await _httpClient.ExecutePost<FlatWorkItemsQueryResult>(url, query, cancellationToken);
        }

        /// <inheritdoc />
        public async Task<T> ExecuteQueryAsync<T>(Guid queryId, CancellationToken cancellationToken = default(CancellationToken))
            where T : WorkItemsQueryResult
        {
            return await ExecuteQueryInternalAsync<T>(queryId, cancellationToken);
        }

        /// <inheritdoc />
        public async Task<WorkItemsQueryResult> ExecuteQueryAndExpandAsync(Guid queryId, bool expandFields, CancellationToken cancellationToken = default(CancellationToken))
        {
            ThrowIfArgumentIsDefault(queryId, nameof(queryId));

            return await ExecuteQueryAndExpandInternal(async () => await ExecuteQueryInternalAsync<JObject>(queryId, cancellationToken), expandFields, cancellationToken);
        }

        /// <inheritdoc />
        public async Task<WorkItemsQueryResult> ExecuteQueryAndExpandAsync(string query, bool expandFields, CancellationToken cancellationToken = default(CancellationToken))
        {
            ThrowIfArgumentNullOrEmpty(query, nameof(query));

            return await ExecuteQueryAndExpandInternal(async () => await ExecuteQueryInternalAsync<JObject>(WorkItemsQuery.Get(query), cancellationToken), expandFields, cancellationToken);
        }

        /// <inheritdoc />
        public async Task<FlatWorkItemsQueryResult> ExecuteFlatQueryAsync(string query, CancellationToken cancellationToken = default(CancellationToken))
        {
            var result = await ExecuteQueryAsync(WorkItemsQuery.Get(query, isHierarchical: false), cancellationToken);
            return result as FlatWorkItemsQueryResult;
        }

        /// <inheritdoc />
        public async Task<HierarchicalWorkItemsQueryResult> ExecuteHierarchicalQueryAsync(string query, CancellationToken cancellationToken = default(CancellationToken))
        {
            var result = await ExecuteQueryAsync(WorkItemsQuery.Get(query, isHierarchical: true), cancellationToken);
            return result as HierarchicalWorkItemsQueryResult;
        }

        /// <inheritdoc />
        public async Task<IEnumerable<WorkItem>> GetWorkItemsAsync(WorkItemsQuery query, bool expand = false, CancellationToken cancellationToken = default(CancellationToken))
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
            {
                return Enumerable.Empty<WorkItem>();
            }

            if (!expand)
            {
                var columns = queryResult.Columns.Select(c => c.ReferenceName).ToArray();
                return await GetWorkItemsAsync(ids, fields: columns, cancellationToken: cancellationToken);
            }
            else
            {
                return await GetWorkItemsAsync(ids, cancellationToken: cancellationToken);
            }
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
            {
                return Enumerable.Empty<WorkItemUpdate>();
            }

            return result.Value;
        }

        /// <inheritdoc />
        public async Task<WorkItem> GetWorkItemAsync(int workItemId, DateTime? asOf = null, string[] fields = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            var fieldsString = fields != null ? string.Join(",", fields) : string.Empty;
            var expandValue = string.IsNullOrEmpty(fieldsString) ? ExpandAllWorkItemFieldsParameterValue : string.Empty;
            var asOfString = asOf.HasValue ? asOf.Value.ToString("u") : string.Empty;

            var url = _urlBuilderFactory.Create()
                .ForWorkItems(workItemId)
                .WithQueryParameterIfNotEmpty("$expand", expandValue)
                .WithQueryParameterIfNotEmpty("fields", fieldsString)
                .WithQueryParameterIfNotEmpty("asOf", asOfString)
                .Build();

            return await _httpClient.ExecuteGet<WorkItem>(url, cancellationToken);
        }

        /// <inheritdoc />
        public Task<WorkItem> GetWorkItemExpandedAsync(int workItemId, DateTime? asOf = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            return GetWorkItemAsync(workItemId, asOf, cancellationToken: cancellationToken);
        }

        /// <inheritdoc />
        public async Task<IEnumerable<WorkItem>> GetWorkItemsAsync(int[] ids, DateTime? asOf = null, string[] fields = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            ThrowIfArgumentNull(ids, nameof(ids));

            if (!ids.Any())
            {
                return Enumerable.Empty<WorkItem>();
            }

            var result = new List<WorkItem>(ids.Length);
            var fieldsString = fields != null ? string.Join(",", fields) : string.Empty;
            var expandValue = string.IsNullOrEmpty(fieldsString) ? ExpandAllWorkItemFieldsParameterValue : string.Empty;
            var asOfString = asOf.HasValue ? asOf.Value.ToString("u") : string.Empty;
            var batchSize = Configuration.WorkitemsBatchSize;
            var batches = Math.Ceiling((decimal)ids.Length / batchSize);
            for (int i = 0; i < batches; i++)
            {
                var idsString = string.Join(",", ids.Skip(i * batchSize).Take(batchSize));
                var url = _urlBuilderFactory.Create()
                    .ForWorkItemsBatch(idsString)
                    .WithQueryParameterIfNotEmpty("$expand", expandValue)
                    .WithQueryParameterIfNotEmpty("fields", fieldsString)
                    .WithQueryParameterIfNotEmpty("asOf", asOfString)
                    .Build();

                var response = await _httpClient.ExecuteGet<CollectionResponse<WorkItem>>(url, cancellationToken);
                result.AddRange(response?.Value ?? Enumerable.Empty<WorkItem>());
            }

            return result;
        }

        /// <inheritdoc />
        public async Task<IEnumerable<WorkItem>> GetWorkItemsAsync(Guid queryId, bool expand, CancellationToken cancellationToken = default(CancellationToken))
        {
            ThrowIfArgumentIsDefault(queryId, nameof(queryId));

            var queryResultObject = await ExecuteQueryInternalAsync<JObject>(queryId, cancellationToken);
            var queryTypeString = queryResultObject.GetValue("queryType", StringComparison.OrdinalIgnoreCase).Value<string>();
            var queryType = (QueryType)Enum.Parse(typeof(QueryType), queryTypeString);

            int[] ids = new int[0];
            WorkItemsQueryResult resultObject;
            if (queryType == QueryType.Tree)
            {
                var treeQueryResult = queryResultObject.ToObject<HierarchicalWorkItemsQueryResult>();
                ids = treeQueryResult.WorkItemRelations.ToIdsArray();
                resultObject = treeQueryResult;
            }
            else if (queryType == QueryType.Flat)
            {
                var flatQueryResult = queryResultObject.ToObject<FlatWorkItemsQueryResult>();
                ids = flatQueryResult.WorkItems.Select(r => r.Id).ToArray();
                resultObject = flatQueryResult;
            }
            else
            {
                throw new UnknownWorkItemQueryTypeException($"Query of type {queryType} is not supported.");
            }

            if (expand)
            {
                return await GetWorkItemsExpandedAsync(ids, cancellationToken: cancellationToken);
            }
            else
            {
                var fields = resultObject.Columns.Select(c => c.ReferenceName).ToArray();
                return await GetWorkItemsAsync(ids, fields: fields, cancellationToken: cancellationToken);
            }
        }

        /// <inheritdoc />
        public Task<IEnumerable<WorkItem>> GetWorkItemsExpandedAsync(int[] ids, DateTime? asOf = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            return GetWorkItemsAsync(ids, asOf, fields: null, cancellationToken: cancellationToken);
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

        private async Task<T> ExecuteQueryInternalAsync<T>(Guid queryId, CancellationToken cancellationToken)
        {
            ThrowIfArgumentIsDefault(queryId, nameof(queryId));

            var url = _urlBuilderFactory.Create()
                .ForWIQL()
                .WithSection(queryId.ToString())
                .Build(Constants.CurrentWorkItemsApiVersion);

            return await _httpClient.ExecuteGet<T>(url, cancellationToken);
        }

        private async Task<T> ExecuteQueryInternalAsync<T>(WorkItemsQuery query, CancellationToken cancellationToken)
        {
            ThrowIfArgumentNull(query, nameof(query));

            var url = _urlBuilderFactory.Create()
                .ForWIQL()
                .Build(Constants.CurrentWorkItemsApiVersion);

            return await _httpClient.ExecutePost<T>(url, query, cancellationToken);
        }

        private async Task<WorkItemsQueryResult> ExecuteQueryAndExpandInternal(Func<Task<JObject>> queryExecutor, bool expandFields, CancellationToken cancellationToken)
        {
            const string queryTypeProperty = "queryType";
            var queryResultObject = await queryExecutor();
            var queryTypeToken = queryResultObject.GetValue(queryTypeProperty, StringComparison.OrdinalIgnoreCase);
            if (queryTypeToken == null)
            {
                throw new UnknownWorkItemQueryTypeException($"Response does not contain property '{queryTypeProperty}'");
            }

            var queryTypeString = queryTypeToken.Value<string>();
            var isKnownQueryType = Enum.TryParse<QueryType>(queryTypeString, out var queryType);
            if (!isKnownQueryType)
            {
                throw new UnknownWorkItemQueryTypeException($"Query of type '{queryTypeString}' is not supported");
            }

            IHaveWorkItems result = null;
            var idsToQuery = new int[0];
            if (queryType == QueryType.Flat)
            {
                result = queryResultObject.ToObject<FlatWorkItemsQueryResultWithWorkItems>();
                idsToQuery = result.WorkItems.Select(w => w.Id).ToArray();
            }
            else if (queryType == QueryType.Tree)
            {
                var heirarchicalQueryResult = queryResultObject.ToObject<HierarchicalWorkItemsQueryResultWithWorkItems>();
                idsToQuery = heirarchicalQueryResult.WorkItemRelations.ToIdsArray();
                result = heirarchicalQueryResult;
            }

            var fields = expandFields ? null : ((IHaveColumns)result).Columns.Select(c => c.ReferenceName).ToArray();
            result.WorkItems = await GetWorkItemsAsync(idsToQuery, fields: fields, cancellationToken: cancellationToken);
            return (WorkItemsQueryResult)result;
        }
    }
}
