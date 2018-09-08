using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using VSTS.Net.Interfaces;
using VSTS.Net.Types;

namespace VSTS.Net.Extensions
{
    public static class AspNetServiceCollectionExtensions
    {
        public static void AddVstsNet(this IServiceCollection services, string instanceName, string accessToken)
        {
            AddVstsNet(services, instanceName, accessToken, _ => { });
        }

        public static void AddVstsNet(this IServiceCollection services, string instanceName, string accessToken, Action<VstsClientConfiguration> cfg)
        {
            services.AddSingleton<IVstsUrlBuilderFactory, OnlineUrlBuilderFactory>(ctx => new OnlineUrlBuilderFactory(instanceName));
            RegisterServices(services, accessToken, cfg);
        }

        public static void AddVstsNet(this IServiceCollection services, Uri baseAddress, string accessToken)
        {
            AddVstsNet(services, baseAddress, accessToken, _ => { });
        }

        public static void AddVstsNet(this IServiceCollection services, Uri baseAddress, string accessToken, Action<VstsClientConfiguration> cfg)
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
        }

        private static VstsClient CreateVstsClient(string instanceName, IServiceProvider ctx, VstsClientConfiguration config)
        {
            var client = ctx.GetService<IHttpClient>();
            var logger = ctx.GetService<ILogger<VstsClient>>();
            return new VstsClient(new OnlineUrlBuilderFactory(instanceName), client, config, logger);
        }
    }
}
