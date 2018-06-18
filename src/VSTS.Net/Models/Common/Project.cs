using System;

namespace VSTS.Net.Models.Common
{
    public class Project
    {
        /// <summary>
        /// Project identifier.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Project name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The project's description (if any).
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Url to the full version of the object.
        /// </summary>
        public string Url { get; set; }
    }
}
