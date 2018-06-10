using System.Collections.Generic;

namespace VSTS.Net.Models.Response
{
    public class CollectionResponse<T>
    {
        /// <summary>
        /// Number of elements returned in the response
        /// </summary>
        public int Count { get; set; }

        /// <summary>
        /// Collection of elements fetched from the server
        /// </summary>
        public IEnumerable<T> Value { get; set; }
    }
}
