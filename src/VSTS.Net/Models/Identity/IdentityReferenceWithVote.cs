using System.Collections.Generic;

namespace VSTS.Net.Models.Identity
{
    public class IdentityReferenceWithVote : IdentityReference
    {
        public int Vote { get; set; }

        public bool IsContainer { get; set; }

        public bool IsRequired { get; set; }

        public IEnumerable<IdentityReferenceWithVote> VotedFor { get; set; }
    }
}
