using System.Net.Http;
using System.Threading.Tasks;

namespace Bouncer.Web.Client.Shim;

public class HttpClientImpl : IHttpClient
{
    /// <summary>
    /// HttpClient to send requests with.
    /// </summary>
    private readonly HttpClient _httpClient = new HttpClient();
    
    /// <summary>
    /// Sends a request and returns the response.
    /// </summary>
    /// <param name="request">Request to send.</param>
    /// <returns>Response for the request.</returns>
    public async Task<HttpStringResponseMessage> SendAsync(HttpRequestMessage request)
    {
        var response = await this._httpClient.SendAsync(request);
        return new HttpStringResponseMessage()
        {
            StatusCode = response.StatusCode,
            Content = await response.Content.ReadAsStringAsync(),
        };
    }
}