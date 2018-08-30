using System;

namespace VSTS.Net.Models.Identity
{
    public class IdentityReference : IEquatable<IdentityReference>
    {
        public Guid Id { get; set; }

        public string Name { get; set; }

        public string DisplayName { get; set; }

        public string UniqueName { get; set; }

        public bool IsContainer { get; set; }

        public string Url { get; set; }

        public string ImageUrl { get; set; }

        public virtual bool Equals(IdentityReference other)
        {
            if (other == null)
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            return string.Equals(UniqueName, other.UniqueName, StringComparison.OrdinalIgnoreCase);
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as IdentityReference);
        }

        public override int GetHashCode()
        {
            return UniqueName.ToLower().GetHashCode();
        }
    }
}
