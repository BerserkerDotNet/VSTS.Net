using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace VSTS.Net.Models.Response.WorkItems
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
