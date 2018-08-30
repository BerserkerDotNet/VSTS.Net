using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using VSTS.Net.Interfaces;
using VSTS.Net.Types;

namespace VSTS.Net
{
    public partial class VstsClient : IVstsClient
    {
        private readonly IVstsUrlBuilderFactory _urlBuilderFactory;
        private readonly IHttpClient _httpClient;
        private readonly ILogger<VstsClient> _logger;

        public VstsClient(IVstsUrlBuilderFactory urlBuilderFactory, IHttpClient client, VstsClientConfiguration configuration, ILogger<VstsClient> logger)
        {
            _urlBuilderFactory = urlBuilderFactory;
            _httpClient = client;
            _logger = logger;
            Configuration = configuration;
        }

        public VstsClient(IVstsUrlBuilderFactory urlBuilderFactory, IHttpClient client, VstsClientConfiguration configuration)
            : this(urlBuilderFactory, client, configuration, new NullLogger<VstsClient>())
        {
        }

        public VstsClient(IVstsUrlBuilderFactory urlBuilderFactory, IHttpClient client)
            : this(urlBuilderFactory, client, VstsClientConfiguration.Default, new NullLogger<VstsClient>())
        {
        }

        /// <summary>
        /// Client configuration
        /// </summary>
        public VstsClientConfiguration Configuration { get; set; }

        public static VstsClient Get(IVstsUrlBuilderFactory urlBuilderFactory, string accessToken, VstsClientConfiguration configuration = null, ILogger<VstsClient> logger = null)
        {
            var client = HttpClientUtil.Create(accessToken);
            var httpClient = new DefaultHttpClient(client, new NullLogger<DefaultHttpClient>());
            var clientLogger = logger ?? new NullLogger<VstsClient>();
            return new VstsClient(urlBuilderFactory, httpClient, configuration ?? VstsClientConfiguration.Default, logger ?? clientLogger);
        }
    }
}
