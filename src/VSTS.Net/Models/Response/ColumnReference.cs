using System;

namespace VSTS.Net.Models.Response
{
    public class ColumnReference
    {
        public ColumnReference(string name, string referenceName)
        {
            Name = name;
            ReferenceName = referenceName;
        }

        /// <summary>
        /// The name of the field.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The reference name of the field.
        /// </summary>
        public string ReferenceName { get; set; }

        /// <summary>
        /// API link of the field
        /// </summary>
        public Uri Url { get; set; }
    }
}
