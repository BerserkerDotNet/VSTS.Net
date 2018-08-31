using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;

namespace VSTS.Net.Types
{
    public static class HttpClientUtil
    {
        /// <summary>
        /// Creates an instance of the <see cref="System.Net.Http.HttpClient" with correct Accept and Authorization headers />
        /// </summary>
        /// <param name="accessToken">Personal access token for VSTS instance</param>
        /// <returns>An instance of HttpClient with authorization header pre-populated</returns>
        public static HttpClient Create(string accessToken)
        {
            var httpClient = new HttpClient();
            ConfigureHttpClient(httpClient, accessToken);
            return httpClient;
        }

        /// <summary>
        /// Sets correct Accept and Authorization headers for VSTS HTTP client
        /// </summary>
        /// <param name="httpClient">An instance of http client ot configure</param>
        /// <param name="accessToken">Personal access token for VSTS instance</param>
        public static void ConfigureHttpClient(HttpClient httpClient, string accessToken)
        {
            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(Constants.JsonMimeType));
            var parameter = Convert.ToBase64String(Encoding.ASCII.GetBytes(string.Format("{0}:{1}", string.Empty, accessToken)));
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(Constants.AuthenticationSchemaBasic, parameter);
        }
    }
}
