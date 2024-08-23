using System;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using System.Threading.Tasks;
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
    /// Performs a request to Roblox Open Cloud.
    /// </summary>
    /// <param name="httpMethod">HTTP method to send.</param>
    /// <param name="url">URL to request.</param>
    /// <param name="jsonResponseTypeInfo">JSON type information to deserialize the response.</param>
    /// <typeparam name="T">Type of the response.</typeparam>
    /// <returns>JSON response for the request.</returns>
    public async Task<T> RequestAsync<T>(HttpMethod httpMethod, string url, JsonTypeInfo<T> jsonResponseTypeInfo) where T : BaseRobloxOpenCloudResponse
    {
        // Perform the request.
        // TODO: Request body for non-GET requests
        var request = new HttpRequestMessage()
        {
            RequestUri = new Uri(url),
            Headers =
            {
                {"x-api-key", OpenCloudApiKey},
            },
            Method = httpMethod,
        };
        var response = await this._httpClient.SendAsync(request);
        
        // Parse the response.
        var responseObject = JsonSerializer.Deserialize<T>(response.Content, jsonResponseTypeInfo)!;
        responseObject.StatusCode = response.StatusCode;
        
        // Throw an exception if there was an API key error.
        if (responseObject.Code == "PERMISSION_DENIED" || responseObject.Code == "INSUFFICIENT_SCOPE")
        {
            throw new OpenCloudAccessException<T>()
            {
                Issue = OpenCloudAccessIssue.PermissionDenied,
                Response = responseObject,
            };
        }
        else if (responseObject.Code == "UNAUTHENTICATED")
        {
            throw new OpenCloudAccessException<T>()
            {
                Issue = OpenCloudAccessIssue.Unauthenticated,
                Response = responseObject,
            };
        }
        else if (responseObject.Code == "RESOURCE_EXHAUSTED")
        {
            throw new OpenCloudAccessException<T>()
            {
                Issue = OpenCloudAccessIssue.TooManyRequests,
                Response = responseObject,
            };
        }
        if (responseObject.Errors != null)
        {
            foreach (var error in responseObject.Errors)
            {
                if (error.Message == "Missing API Key Header")
                {
                    throw new OpenCloudAccessException<T>()
                    {
                        Issue = OpenCloudAccessIssue.MissingApiKey,
                        Response = responseObject,
                    };
                }
                else if (error.Message == "Invalid API Key")
                {
                    throw new OpenCloudAccessException<T>()
                    {
                        Issue = OpenCloudAccessIssue.InvalidApiKey,
                        Response = responseObject,
                    };
                }
            }
        }
        
        // Return the response object.
        return responseObject;
    }

    /// <summary>
    /// Performs a GET request to Roblox Open Cloud.
    /// </summary>
    /// <param name="url">URL to request.</param>
    /// <param name="jsonResponseTypeInfo">JSON type information to deserialize the response.</param>
    /// <typeparam name="T">Type of the response.</typeparam>
    /// <returns>JSON response for the request.</returns>
    public Task<T> GetAsync<T>(string url, JsonTypeInfo<T> jsonResponseTypeInfo) where T : BaseRobloxOpenCloudResponse
    {
        return this.RequestAsync(HttpMethod.Get, url, jsonResponseTypeInfo);
    }
}