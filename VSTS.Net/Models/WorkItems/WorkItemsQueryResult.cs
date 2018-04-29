using System;
using System.Collections.Generic;

namespace VSTS.Net.Models.Response.WorkItems
{
    public abstract class WorkItemsQueryResult
    {
        /// <summary>
        /// The type of the query
        /// </summary>
        public string QueryType { get; set; }

        /// <summary>
        /// The date the query was run in the context of.
        /// </summary>
        public DateTime AsOf { get; set; }

        /// <summary>
        /// Reference to a field in a work item
        /// </summary>
        public IEnumerable<ColumnReference> Columns { get; set; }

        /// <summary>
        /// A sort column.
        /// </summary>
        public IEnumerable<SortColumn> SortColumns { get; set; }
    }
}
