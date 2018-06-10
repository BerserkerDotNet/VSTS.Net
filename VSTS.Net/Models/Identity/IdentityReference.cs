using System;

namespace VSTS.Net.Models.Identity
{
    public class IdentityReference
    {
        public Guid Id { get; set; }

        public string Name { get; set; }

        public string DisplayName { get; set; }

        public string UniqueName { get; set; }

        public string Url { get; set; }

        public string ImageUrl { get; set; }
    }
}
