using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VSTS.Net.Types
{
    public class VstsUrlBuilder
    {
        private const string APIsSection = "_apis";
        private const string IdentitiesSection = "Identities";
        private const string WITSection = "wit";
        private const string WIQLApiSection = "wiql";
        private const string QueriesApiSection = "queries";
        private const string WorkItemsSection = "workitems";
        private const string GITSection = "git";
        private const string RepositoriesSection = "repositories";
        private const string PullRequestsSection = "pullRequests";
        private const string APIVersion = "api-version";

        private StringBuilder _url = new StringBuilder();
        private Dictionary<string, string> _parameters = new Dictionary<string, string>();

        private VstsUrlBuilder(string instance, string subDomain = "")
            : this(new Uri($"https://{$"{instance}.{subDomain}".TrimEnd('.')}.visualstudio.com"))
        {
        }

        private VstsUrlBuilder(Uri baseUri)
        {
            _url.Append(baseUri.ToString().Trim('/'));
        }

        public static VstsUrlBuilder Create(string instance, string subDomain = "")
        {
            return new VstsUrlBuilder(instance, subDomain);
        }

        public static VstsUrlBuilder Create(Uri baseUri)
        {
            return new VstsUrlBuilder(baseUri);
        }

        public VstsUrlBuilder ForWIQL()
        {
            _url.Append($"/{APIsSection}/{WITSection}/{WIQLApiSection}");
            return this;
        }

        public VstsUrlBuilder ForQueries(string project, string queryId)
        {
            _url.Append($"/{project}/{APIsSection}/{WITSection}/{QueriesApiSection}/{queryId}");
            return this;
        }

        public VstsUrlBuilder ForWorkItemsBatch(string workItemIds)
        {
            _url.Append($"/{APIsSection}/{WITSection}/{WorkItemsSection}");
            WithQueryParameter("ids", workItemIds);
            return this;
        }

        public VstsUrlBuilder ForWorkItems(int workItemId)
        {
            ForWorkItems().WithSection(workItemId.ToString());
            return this;
        }

        public VstsUrlBuilder ForWorkItems()
        {
            _url.Append($"/{APIsSection}/{WITSection}/{WorkItemsSection}");
            return this;
        }

        public VstsUrlBuilder ForRepository(string projectName, string repositoryName)
        {
            _url.Append($"/{projectName}/{APIsSection}/{GITSection}/{RepositoriesSection}/{repositoryName}");
            return this;
        }

        public VstsUrlBuilder ForPullRequests(string projectName, string repositoryName)
        {
            ForRepository(projectName, repositoryName);
            _url.Append("/pullRequests");
            return this;
        }

        public VstsUrlBuilder ForIdentities()
        {
            _url.Append($"/{APIsSection}/{IdentitiesSection}");
            return this;
        }

        public VstsUrlBuilder WithSection(string name)
        {
            _url.Append($"/{name}");
            return this;
        }

        public VstsUrlBuilder WithQueryParameter<T>(string name, T value)
        {
            _parameters.Add(name, value.ToString());
            return this;
        }

        public VstsUrlBuilder WithQueryParameterIfNotEmpty<T>(string name, T value)
        {
            if (value == null || string.IsNullOrEmpty(value.ToString()))
                return this;
            
            return WithQueryParameter(name, value.ToString());
        }

        public VstsUrlBuilder WithQueryParameterIfNotDefault<T>(string name, T value)
        {
            if (EqualityComparer<T>.Default.Equals(value, default(T)))
                return this;

            return WithQueryParameter(name, value.ToString());
        }

        public VstsUrlBuilder Top(int count)
        {
            _parameters.Add("$top", count.ToString());
            return this;
        }

        public string Build(string apiVersion = Constants.CurrentWorkItemsApiVersion)
        {
            _parameters.Add(APIVersion, apiVersion);
            var queryString = $"?{string.Join("&", _parameters.Select(p => $"{p.Key}={p.Value}"))}";
            _url.Append(queryString);
            return _url.ToString();
        }
    }
}
