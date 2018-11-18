﻿using System.Collections.Generic;

namespace VSTS.Net.Models.WorkItems
{
    public class FlatWorkItemsQueryResult : WorkItemsQueryResult
    {
        /// <summary>
        /// Contains reference to a work item.
        /// </summary>
        public IEnumerable<WorkItemReference> WorkItems { get; set; }
    }
}