using System;
using Newtonsoft.Json;

namespace VSTS.Net.Models.Identity
{
    public class Identity
    {
        public Guid Id { get; set; }

        [JsonProperty("providerDisplayName")]
        public string DisplayName { get; set; }

        public bool IsActive { get; set; }
    }
}
