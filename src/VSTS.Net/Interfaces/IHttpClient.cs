using System.Security;
using System.Threading;
using System.Threading.Tasks;

namespace VSTS.Net.Interfaces
{
    public interface IHttpClient
    {
        /// <summary>
        /// Executes a GET request to the specified URL
        /// </summary>
        /// <typeparam name="T">Type of the result</typeparam>
        /// <param name="url">Url ton reach</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <returns></returns>
        Task<T> ExecuteGet<T>(string url, CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="url"></param>
        /// <param name="payload"></param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <returns></returns>
        Task<T> ExecutePost<T>(string url, object payload, CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="url"></param>
        /// <param name="payload"></param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <returns></returns>
        Task<T> ExecutePost<T>(string url, string payload, CancellationToken cancellationToken = default(CancellationToken));
    }
}
