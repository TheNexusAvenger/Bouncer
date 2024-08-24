using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Bouncer.Web.Client.Shim;

public class CachingHttpClient : IHttpClient
{
    /// <summary>
    /// Base HTTP client to call.
    /// </summary>
    private readonly IHttpClient _httpClient;
    
    /// <summary>
    /// Cache for the GET requests.
    /// </summary>
    private readonly Dictionary<string, Task<HttpStringResponseMessage>> _getRequestCache = new Dictionary<string, Task<HttpStringResponseMessage>>();

    /// <summary>
    /// Semaphore for accessing and modifying the cache.
    /// </summary>
    private readonly SemaphoreSlim _cacheSemaphore = new SemaphoreSlim(1);
    
    /// <summary>
    /// Creates a caching HTTP client.
    /// </summary>
    /// <param name="httpClient">HTTP client to send requests with.</param>
    public CachingHttpClient(IHttpClient httpClient)
    {
        this._httpClient = httpClient;
    }
    
    /// <summary>
    /// Creates a caching HTTP client.
    /// </summary>
    public CachingHttpClient() : this(new HttpClientImpl())
    {
        
    }

    /// <summary>
    /// Clears the cache.
    /// </summary>
    public async Task ClearCacheAsync()
    {
        await this._cacheSemaphore.WaitAsync();
        this._getRequestCache.Clear();
        this._cacheSemaphore.Release();
    }

    /// <summary>
    /// Sends a request and returns the response.
    /// </summary>
    /// <param name="request">Request to send.</param>
    /// <returns>Response for the request.</returns>
    public async Task<HttpStringResponseMessage> SendAsync(HttpRequestMessage request)
    {
        // Perform the request directly if it isn't a GET request.
        // Only GET requests are cached.
        if (request.Method != HttpMethod.Get)
        {
            return await this._httpClient.SendAsync(request);
        }
        
        // Prepare the cached response.
        await this._cacheSemaphore.WaitAsync();
        var requestUrl = request.RequestUri?.ToString() ?? "";
        if (!this._getRequestCache.ContainsKey(requestUrl))
        {
            this._getRequestCache[requestUrl] = Task.Run(async () => await this._httpClient.SendAsync(request));
        }
        var responseTask = this._getRequestCache[requestUrl];
        this._cacheSemaphore.Release();
        return await responseTask;
    }
}