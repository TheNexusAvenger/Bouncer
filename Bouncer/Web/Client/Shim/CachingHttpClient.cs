using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Bouncer.Web.Client.Shim;

public class HttpClientCacheEntry
{
    /// <summary>
    /// Task for the response.
    /// </summary>
    public Task<HttpStringResponseMessage> ResponseTask { get; set; } = null!;

    /// <summary>
    /// Time that the response was cached.
    /// </summary>
    public DateTime StartTime { get; set; }
}

public class CachingHttpClient : IHttpClient
{
    /// <summary>
    /// Static instance of a caching HTTP client.
    /// </summary>
    public static readonly CachingHttpClient Instance = new CachingHttpClient();
    
    /// <summary>
    /// Time to clear the cache.
    /// </summary>
    public TimeSpan CacheClearTime { get; set; }= TimeSpan.FromMinutes(5);
    
    /// <summary>
    /// Base HTTP client to call.
    /// </summary>
    private readonly IHttpClient _httpClient;
    
    /// <summary>
    /// Cache for the GET requests.
    /// </summary>
    private readonly Dictionary<string, HttpClientCacheEntry> _getRequestCache = new Dictionary<string, HttpClientCacheEntry>();

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
        
        // Clear cache entries in the background.
        Task.Run(async () =>
        {
            while (true)
            {
                await this._cacheSemaphore.WaitAsync();
                var urlsToRemove = new List<string>();
                foreach (var (url, cacheEntry) in this._getRequestCache)
                {
                    if (DateTime.Now - cacheEntry.StartTime < this.CacheClearTime) continue;
                    urlsToRemove.Add(url);
                }
                foreach (var url in urlsToRemove)
                {
                    this._getRequestCache.Remove(url);
                }
                this._cacheSemaphore.Release();
                await Task.Delay(5000);
            }
        });
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
            this._getRequestCache[requestUrl] = new HttpClientCacheEntry()
            {
                ResponseTask = Task.Run(async () => await this._httpClient.SendAsync(request)),
                StartTime = DateTime.Now,
            };
        }
        var cachedResponse = this._getRequestCache[requestUrl];
        this._cacheSemaphore.Release();
        return await cachedResponse.ResponseTask;
    }
}