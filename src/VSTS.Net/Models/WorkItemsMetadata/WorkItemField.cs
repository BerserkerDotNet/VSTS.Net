using System;
using System.Collections.Generic;
using VSTS.Net.Models.Response;

namespace VSTS.Net.Models.WorkItemsMetadata
{
    public class WorkItemField
    {
        /// <summary>
        /// The name of the field.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The reference name of the field.
        /// </summary>
        public string ReferenceName { get; set; }

        /// <summary>
        /// The type of the field.
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// Indicates whether the field is read only.
        /// </summary>
        public bool ReadOnly { get; set; }

        /// <summary>
        /// The supported operations on this field.
        /// </summary>
        public IEnumerable<OperationReference> SupportedOperations { get; set; }

        /// <summary>
        /// API link of the field
        /// </summary>
        public Uri Url { get; set; }
    }
}
