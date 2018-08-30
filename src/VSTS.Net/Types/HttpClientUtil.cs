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
            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(Constants.JsonMimeType));
            var parameter = Convert.ToBase64String(Encoding.ASCII.GetBytes(string.Format("{0}:{1}", string.Empty, accessToken)));
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(Constants.AuthenticationSchemaBasic, parameter);

            return httpClient;
        }
    }
}
