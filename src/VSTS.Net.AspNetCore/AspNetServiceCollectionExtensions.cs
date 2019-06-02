using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using VSTS.Net.Interfaces;
using VSTS.Net.Types;

namespace VSTS.Net.Extensions
{
    public static class AspNetServiceCollectionExtensions
    {
        [Obsolete("VSTS was renamed to Azure DevOps Services, use AddAzureDevOpsServices instead")]
        [ExcludeFromCodeCoverage]
        public static void AddVstsNet(this IServiceCollection services, string instanceName, string accessToken)
        {
            AddAzureDevOpsServices(services, instanceName, accessToken, _ => { });
        }

        [Obsolete("VSTS was renamed to Azure DevOps Services, use AddAzureDevOpsServices instead")]
        [ExcludeFromCodeCoverage]
        public static void AddVstsNet(this IServiceCollection services, string instanceName, string accessToken, Action<VstsClientConfiguration> cfg)
        {
            AddAzureDevOpsServices(services, accessToken, accessToken, cfg);
        }

        [Obsolete("VSTS was renamed to Azure DevOps Services, use AddAzureDevOpsServices instead")]
        [ExcludeFromCodeCoverage]
        public static void AddVstsNet(this IServiceCollection services, Uri baseAddress, string accessToken)
        {
            AddAzureDevOpsServices(services, baseAddress, accessToken, _ => { });
        }

        [Obsolete("VSTS was renamed to Azure DevOps Services, use AddAzureDevOpsServices instead")]
        [ExcludeFromCodeCoverage]
        public static void AddVstsNet(this IServiceCollection services, Uri baseAddress, string accessToken, Action<VstsClientConfiguration> cfg)
        {
            AddAzureDevOpsServices(services, baseAddress, accessToken, cfg);
        }

        public static void AddAzureDevOpsServices(this IServiceCollection services, string instanceName, string accessToken)
        {
            AddAzureDevOpsServices(services, instanceName, accessToken, _ => { });
        }

        public static void AddAzureDevOpsServices(this IServiceCollection services, string instanceName, string accessToken, Action<VstsClientConfiguration> cfg)
        {
            services.AddSingleton<IVstsUrlBuilderFactory, OnlineUrlBuilderFactory>(ctx => new OnlineUrlBuilderFactory(instanceName));
            RegisterServices(services, accessToken, cfg);
        }

        public static void AddAzureDevOpsServices(this IServiceCollection services, Uri baseAddress, string accessToken)
        {
            AddAzureDevOpsServices(services, baseAddress, accessToken, _ => { });
        }

        public static void AddAzureDevOpsServices(this IServiceCollection services, Uri baseAddress, string accessToken, Action<VstsClientConfiguration> cfg)
        {
            services.AddSingleton<IVstsUrlBuilderFactory, OnPremUrlBuilderFactory>(ctx => new OnPremUrlBuilderFactory(baseAddress));
            RegisterServices(services, accessToken, cfg);
        }

        private static void RegisterServices(IServiceCollection services, string accessToken, Action<VstsClientConfiguration> cfg)
        {
            var config = VstsClientConfiguration.Default;
            cfg(config);
            services.Add(new ServiceDescriptor(typeof(VstsClientConfiguration), config));

            services.AddHttpClient<IHttpClient, DefaultHttpClient>(client =>
            {
                HttpClientUtil.ConfigureHttpClient(client, accessToken);
            });

            services.AddSingleton<IVstsClient, VstsClient>();
            services.AddSingleton<IVstsWorkItemsClient, VstsClient>();
            services.AddSingleton<IVstsPullRequestsClient, VstsClient>();
            services.AddSingleton<IVstsWorkItemsMetadataClient, VstsClient>();
            services.AddSingleton<IVstsIdentityClient, VstsClient>();
        }
    }
}
