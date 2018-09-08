using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using VSTS.Net.Interfaces;
using VSTS.Net.Models.Response;
using VSTS.Net.Models.WorkItemsMetadata;

namespace VSTS.Net
{
    public partial class VstsClient : IVstsClient
    {
        public async Task<IEnumerable<WorkItemField>> GetWorkItemFieldsAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            var url = _urlBuilderFactory.Create()
                .ForWorkItemFields()
                .Build();

            var result = await _httpClient.ExecuteGet<CollectionResponse<WorkItemField>>(url, cancellationToken);
            return result.Value;
        }
    }
}
