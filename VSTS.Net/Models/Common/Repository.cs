using System;

namespace VSTS.Net.Models.Common
{
    public class Repository
    {
        /// <summary>
        /// Repository Id.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Repository name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// REpository Url
        /// </summary>
        public string Url { get; set; }

        /// <summary>
        /// Represents a shallow reference to a TeamProject.
        /// </summary>
        public Project Project { get; set; }
    }
}
