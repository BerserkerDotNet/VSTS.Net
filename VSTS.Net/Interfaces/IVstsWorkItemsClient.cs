using System.Collections.Generic;
using System.Threading.Tasks;
using VSTS.Net.Models.Request;
using VSTS.Net.Models.Response.WorkItems;

namespace VSTS.Net.Interfaces
{
    public interface IVstsWorkItemsClient
    {
        /// <summary>
        /// Executes a work item query
        /// </summary>
        /// <param name="query">Work item query to execute</param>
        /// <returns>Returns <see cref="WorkItemsQueryResult"/></returns>
        Task<WorkItemsQueryResult> ExecuteQueryAsync(WorkItemsQuery query);

        /// <summary>
        /// Executes a work item query
        /// </summary>
        /// <param name="query">Work item query to execute</param>
        /// <returns>List of work items</returns>
        Task<IEnumerable<WorkItem>> GetWorkItemsAsync(WorkItemsQuery query);
    }
}
