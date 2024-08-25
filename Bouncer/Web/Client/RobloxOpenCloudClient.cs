using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using System.Threading.Tasks;
using Bouncer.Web.Client.Request;
using Bouncer.Web.Client.Response;
using Bouncer.Web.Client.Shim;

namespace Bouncer.Web.Client;

public class RobloxOpenCloudClient
{
    /// <summary>
    /// Roblox Open Cloud API key.
    /// </summary>
    public string? OpenCloudApiKey { get; set; }
    
    /// <summary>
    /// HTTP client to send requests.
    /// </summary>
    private readonly IHttpClient _httpClient;

    /// <summary>
    /// Creates a Roblox Open Cloud client.
    /// </summary>
    /// <param name="httpClient">HTTP client to send requests with.</param>
    public RobloxOpenCloudClient(IHttpClient httpClient)
    {
        this._httpClient = httpClient;
    }
    
    /// <summary>
    /// Clears the cache.
    /// Only clears the cache if the HTTP client is caching.
    /// </summary>
    public async Task ClearCacheAsync()
    {
        if (this._httpClient is not CachingHttpClient cachingHttpClient) return;
        await cachingHttpClient.ClearCacheAsync();
    }

    /// <summary>
    /// Performs a request to Roblox Open Cloud.
    /// </summary>
    /// <param name="httpMethod">HTTP method to send.</param>
    /// <param name="url">URL to request.</param>
    /// <param name="jsonResponseTypeInfo">JSON type information to deserialize the response.</param>
    /// <param name="content">Content of the request.</param>
    /// <typeparam name="TResponse">Type of the response.</typeparam>
    /// <returns>JSON response for the request.</returns>
    public async Task<TResponse> RequestAsync<TResponse>(HttpMethod httpMethod, string url, JsonTypeInfo<TResponse> jsonResponseTypeInfo, HttpContent? content = null) where TResponse : BaseRobloxOpenCloudResponse
    {
        // Perform the request.
        var request = new HttpRequestMessage()
        {
            RequestUri = new Uri(url),
            Headers =
            {
                {"x-api-key", OpenCloudApiKey},
            },
            Method = httpMethod,
        };
        if (content != null)
        {
            request.Content = content;
        }
        var response = await this._httpClient.SendAsync(request);
        
        // Parse the response.
        var responseObject = JsonSerializer.Deserialize<TResponse>(response.Content, jsonResponseTypeInfo)!;
        responseObject.StatusCode = response.StatusCode;
        
        // Throw an exception if there was an API key error.
        OpenCloudAccessIssue? accessIssue = null;
        if (responseObject.Code == "PERMISSION_DENIED" || responseObject.Code == "INSUFFICIENT_SCOPE")
        {
            accessIssue = OpenCloudAccessIssue.PermissionDenied;
        }
        else if (responseObject.Code == "UNAUTHENTICATED")
        {
            accessIssue = OpenCloudAccessIssue.Unauthenticated;
        }
        else if (responseObject.Code == "RESOURCE_EXHAUSTED")
        {
            accessIssue = OpenCloudAccessIssue.TooManyRequests;
        }
        if (responseObject.Errors != null)
        {
            foreach (var error in responseObject.Errors)
            {
                if (error.Message == "Missing API Key Header")
                {
                    accessIssue = OpenCloudAccessIssue.MissingApiKey;
                }
                else if (error.Message == "Invalid API Key")
                {
                    accessIssue = OpenCloudAccessIssue.InvalidApiKey;
                }
                else if (error.Message == "The user is invalid or does not exist.")
                {
                    accessIssue = OpenCloudAccessIssue.InvalidUser;
                }
                else if (error.Message == "Too many requests")
                {
                    accessIssue = OpenCloudAccessIssue.TooManyRequests;
                }
            }
        }
        if (accessIssue == null && (int) response.StatusCode >= 300)
        {
            if (response.StatusCode == HttpStatusCode.TooManyRequests)
            {
                accessIssue = OpenCloudAccessIssue.TooManyRequests;
            }
            else if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                accessIssue = OpenCloudAccessIssue.InvalidApiKey;
            }
            else if (response.StatusCode == HttpStatusCode.Forbidden)
            {
                accessIssue = OpenCloudAccessIssue.Unauthenticated;
            }
            else
            {
                accessIssue = OpenCloudAccessIssue.Unknown;
            }
        }
        if (accessIssue != null)
        {
            throw new OpenCloudAccessException<TResponse>()
            {
                Issue = accessIssue.Value,
                Response = responseObject,
            };
        }
        
        // Return the response object.
        return responseObject;
    }

    /// <summary>
    /// Performs a GET request to Roblox Open Cloud.
    /// </summary>
    /// <param name="url">URL to request.</param>
    /// <param name="jsonResponseTypeInfo">JSON type information to deserialize the response.</param>
    /// <typeparam name="TResponse">Type of the response.</typeparam>
    /// <returns>JSON response for the request.</returns>
    public Task<TResponse> GetAsync<TResponse>(string url, JsonTypeInfo<TResponse> jsonResponseTypeInfo) where TResponse : BaseRobloxOpenCloudResponse
    {
        return this.RequestAsync(HttpMethod.Get, url, jsonResponseTypeInfo);
    }

    /// <summary>
    /// Performs a POST request to Roblox Open Cloud.
    /// </summary>
    /// <param name="url">URL to request.</param>
    /// <param name="jsonResponseTypeInfo">JSON type information to deserialize the response.</param>
    /// <param name="content">Content of the request to send.</param>
    /// <typeparam name="TResponse">Type of the response.</typeparam>
    /// <returns>JSON response for the request.</returns>
    public Task<TResponse> PostAsync<TResponse>(string url, JsonTypeInfo<TResponse> jsonResponseTypeInfo, HttpContent content) where TResponse : BaseRobloxOpenCloudResponse
    {
        return this.RequestAsync(HttpMethod.Post, url, jsonResponseTypeInfo, content);
    }

    /// <summary>
    /// Performs a POST request to Roblox Open Cloud with an empty request body.
    /// </summary>
    /// <param name="url">URL to request.</param>
    /// <param name="jsonResponseTypeInfo">JSON type information to deserialize the response.</param>
    /// <typeparam name="TResponse">Type of the response.</typeparam>
    /// <returns>JSON response for the request.</returns>
    public Task<TResponse> PostAsync<TResponse>(string url, JsonTypeInfo<TResponse> jsonResponseTypeInfo) where TResponse : BaseRobloxOpenCloudResponse
    {
        return this.PostAsync(url, jsonResponseTypeInfo, JsonContent.Create(new EmptyRequest(), EmptyRequestJsonContext.Default.EmptyRequest));
    }

    /// <summary>
    /// Performs a POST request to Roblox Open Cloud with an empty request body and no response.
    /// </summary>
    /// <param name="url">URL to request.</param>
    public Task PostAsync(string url)
    {
        return this.PostAsync(url, BaseRobloxOpenCloudResponseJsonContext.Default.BaseRobloxOpenCloudResponse);
    }
}