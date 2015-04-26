using System;
using System.Net.Http;

namespace Jalex.Infrastructure.Net
{
    public interface IHttpCommunicator
    {
        /// <summary>
        /// Gets http response converted to type TRet for a given URI, with the given parameters
        /// </summary>
        /// <typeparam name="TRet">The return type of the request</typeparam>
        /// <typeparam name="TParam">The parameters for the request</typeparam>
        /// <param name="uri">The uri to which to send the request</param>
        /// <param name="parameters">The parameters to include in the request</param>
        /// <param name="method">The HTTP method to use</param>
        /// <param name="timeout">The timeout for the request</param>
        /// <returns>The response retrieved from the remote server</returns>
        TRet GetHttpResponse<TRet, TParam>(Uri uri, TParam parameters, HttpMethod method, TimeSpan timeout);
    }
}