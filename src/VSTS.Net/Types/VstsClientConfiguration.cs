namespace VSTS.Net.Types
{
    public class VstsClientConfiguration
    {
        /// <summary>
        /// Version of the VSTS api to use when working with workitems.
        /// Default is 4.1
        /// </summary>
        public string WorkItemsApiVersion { get; set; } = Constants.CurrentWorkItemsApiVersion;

        /// <summary>
        /// Version of the VSTS api to use when working with pull requests.
        /// Default is 4.1
        /// </summary>
        public string PullRequestsApiVersion { get; set; } = Constants.CurrentWorkItemsApiVersion;

        /// <summary>
        /// Maximum number of workitem ids per request.
        /// </summary>
        public int WorkitemsBatchSize { get; set; } = 400;

        /// <summary>
        /// Default configuration
        /// </summary>
        public static VstsClientConfiguration Default => new VstsClientConfiguration();
    }
}
