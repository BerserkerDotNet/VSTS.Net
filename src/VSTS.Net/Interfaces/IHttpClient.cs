using System.Security;
using System.Threading;
using System.Threading.Tasks;

namespace VSTS.Net.Interfaces
{
    public interface IHttpClient
    {
        /// <summary>
        /// Sends a GET request to the specified URL
        /// </summary>
        /// <typeparam name="T">Type of the result</typeparam>
        /// <param name="url">Url to send request to</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <returns></returns>
        Task<T> ExecuteGet<T>(string url, CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// Sends a POST request to the specified URL
        /// </summary>
        /// <typeparam name="T">Type of the result</typeparam>
        /// <param name="url">Url to send request to</param>
        /// <param name="payload">content to send</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <returns></returns>
        Task<T> ExecutePost<T>(string url, object payload, CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// Sends a POST request to the specified URL
        /// </summary>
        /// <typeparam name="T">Type of the result</typeparam>
        /// <param name="url">Url to send request to</param>
        /// <param name="payload">content to send</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <returns></returns>
        Task<T> ExecutePost<T>(string url, string payload, CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// Sends a DELETE request to the specified URL
        /// </summary>
        /// <typeparam name="T">Type of the result</typeparam>
        /// <param name="url">Url to send request to</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <returns></returns>
        Task<T> ExecuteDelete<T>(string url, CancellationToken cancellationToken = default(CancellationToken));
    }
}
