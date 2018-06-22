using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using VSTS.Net.Interfaces;
using VSTS.Net.Types;

namespace VSTS.Net
{
    public partial class VstsClient : IVstsClient
    {
        private readonly string _instanceName;
        private readonly IHttpClient _httpClient;
        private readonly ILogger<VstsClient> _logger;

        public VstsClient(string instanceName, IHttpClient client, ILogger<VstsClient> logger)
        {
            _instanceName = instanceName;
            _httpClient = client;
            _logger = logger;
        }

        public VstsClient(string instanceName, IHttpClient client)
            : this(instanceName, client, new NullLogger<VstsClient>())
        {
        }

        public static VstsClient Get(string instanceName, string accessToken, ILogger<VstsClient> logger = null)
        {
            var client = HttpClientUtil.Create(accessToken);
            var httpClient = new DefaultHttpClient(client, new NullLogger<DefaultHttpClient>());
            var clientLogger = logger ?? new NullLogger<VstsClient>();
            return new VstsClient(instanceName, httpClient, logger ?? clientLogger);
        }
    }
}
