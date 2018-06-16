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
            var httpClient = HttpClientUtil.Create(accessToken);
            services.AddSingleton<IHttpClient, DefaultHttpClient>(ctx =>
            {
                var logger = ctx.GetService<ILogger<DefaultHttpClient>>();
                return new DefaultHttpClient(httpClient, logger);
            });

            services.AddSingleton<IVstsClient, VstsClient>(ctx => CreateVstsClient(instanceName, ctx));
            services.AddSingleton<IVstsWorkItemsClient, VstsClient>(ctx => GetVstsClient(instanceName, ctx));
            services.AddSingleton<IVstsPullRequestsClient, VstsClient>(ctx => GetVstsClient(instanceName, ctx));
        }

        private static VstsClient CreateVstsClient(string instanceName, System.IServiceProvider ctx)
        {
            var client = ctx.GetService<IHttpClient>();
            var logger = ctx.GetService<ILogger<VstsClient>>();
            return new VstsClient(instanceName, client, logger);
        }

        private static VstsClient GetVstsClient(string instanceName, System.IServiceProvider ctx)
        {
            return ctx.GetService<IVstsClient>() as VstsClient;
        }
    }
}
