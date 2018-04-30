using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using VSTS.Net.Interfaces;
using VSTS.Net.Types;

namespace VSTS.Net.Extensions
{
    public static class AspNetServiceCollectionExtensions
    {
        public static void AddVstsNet(this IServiceCollection services, string instanceName, string accessToken)
        {
            var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(Constants.JsonMimeType));
            var parameter = Convert.ToBase64String(Encoding.ASCII.GetBytes(string.Format("{0}:{1}", "", accessToken)));
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(Constants.AuthenticationSchemaBasic, parameter);

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
