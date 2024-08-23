using System.Net.Http;
using System.Threading.Tasks;

namespace Bouncer.Web.Client.Shim;

public interface IHttpClient
{
    /// <summary>
    /// Sends a request and returns the response.
    /// </summary>
    /// <param name="request">Request to send.</param>
    /// <returns>Response for the request.</returns>
    public Task<HttpStringResponseMessage> SendAsync(HttpRequestMessage request);
}