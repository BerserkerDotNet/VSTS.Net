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

            services.AddSingleton<IVstsWorkItemsClient, VstsClient>(ctx => 
            {
                var client = ctx.GetService<IHttpClient>();
                return new VstsClient(instanceName, client);
            });
        }
    }
}
