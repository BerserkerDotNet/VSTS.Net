using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace VSTS.Net.Models.WorkItems
{
    public class WorkItemRelation
    {
        /// <summary>
        /// Collection of link attributes.
        /// </summary>
        public Dictionary<string, string> Attributes { get; set; }

        /// <summary>
        /// Relation type.
        /// </summary>
        [JsonProperty("rel")]
        public string RelationType { get; set; }

        /// <summary>
        /// Link url.
        /// </summary>
        public Uri Url { get; set; }
    }
}
