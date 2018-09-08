using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using VSTS.Net.Models.WorkItemsMetadata;

namespace VSTS.Net.Interfaces
{
    public interface IVstsWorkItemsMetadataClient
    {
        Task<IEnumerable<WorkItemField>> GetWorkItemFieldsAsync(CancellationToken cancellationToken = default(CancellationToken));
    }
}
