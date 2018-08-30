using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using VSTS.Net.Interfaces;
using static VSTS.Net.Utils.NullCheckUtility;

namespace VSTS.Net.Types
{
    public class DefaultHttpClient : IHttpClient
    {
        private readonly HttpClient _client;
        private readonly ILogger<DefaultHttpClient> _logger;

        public DefaultHttpClient(HttpClient client, ILogger<DefaultHttpClient> logger)
        {
            ThrowIfArgumentNull(client, nameof(client));
            _client = client;
            _logger = logger;
        }

        /// <inheritdoc />
        public async Task<T> ExecuteGet<T>(string url, CancellationToken cancellationToken = default(CancellationToken))
        {
            _logger.LogDebug($"Requesting '{url}' via GET");
            using (var response = await _client.GetAsync(url, cancellationToken))
            {
                response.EnsureSuccessStatusCode();
                var content = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<T>(content);
            }
        }

        /// <inheritdoc />
        public async Task<T> ExecutePost<T>(string url, object payload, CancellationToken cancellationToken = default(CancellationToken))
        {
            return await ExecutePost<T>(url, payload, Constants.JsonMimeType, cancellationToken);
        }

        /// <inheritdoc />
        public async Task<T> ExecutePost<T>(string url, string payload, CancellationToken cancellationToken = default(CancellationToken))
        {
            return await ExecutePost<T>(url, payload, Constants.JsonMimeType, cancellationToken);
        }

        /// <inheritdoc />
        public async Task<T> ExecutePost<T>(string url, object payload, string mimeType = Constants.JsonMimeType, CancellationToken cancellationToken = default(CancellationToken))
        {
            return await ExecutePost<T>(url, JsonConvert.SerializeObject(payload), mimeType, cancellationToken);
        }

        /// <inheritdoc />
        public async Task<T> ExecutePost<T>(string url, string payload, string mimeType = Constants.JsonMimeType, CancellationToken cancellationToken = default(CancellationToken))
        {
            _logger.LogDebug($"Requesting '{url}' via POST");
            var content = new StringContent(payload, Encoding.UTF8, mimeType);
            using (var response = await _client.PostAsync(url, content, cancellationToken))
            {
                response.EnsureSuccessStatusCode();
                var resultContent = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<T>(resultContent);
            }
        }

        /// <inheritdoc />
        public async Task<T> ExecutePatch<T>(string url, object payload, string mimeType, CancellationToken cancellationToken = default(CancellationToken))
        {
            return await ExecutePatch<T>(url, JsonConvert.SerializeObject(payload), mimeType, cancellationToken);
        }

        /// <inheritdoc />
        public async Task<T> ExecutePatch<T>(string url, string payload, string mimeType, CancellationToken cancellationToken = default(CancellationToken))
        {
            _logger.LogDebug($"Requesting '{url}' via PATCH");
            var content = new StringContent(payload, Encoding.UTF8, mimeType);
            var message = new HttpRequestMessage(new HttpMethod(Constants.HttpMethodPatch), url);
            message.Content = content;
            using (var response = await _client.SendAsync(message, cancellationToken))
            {
                response.EnsureSuccessStatusCode();
                var resultContent = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<T>(resultContent);
            }
        }

        /// <inheritdoc />
        public async Task<T> ExecuteDelete<T>(string url, CancellationToken cancellationToken = default(CancellationToken))
        {
            _logger.LogDebug($"Deleting {url}");
            using (var response = await _client.DeleteAsync(url, cancellationToken))
            {
                response.EnsureSuccessStatusCode();
                var content = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<T>(content);
            }
        }
    }
}
