using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Bouncer.Web.Client.Shim;

namespace Bouncer.Test.Web.Client.Shim;

public class TestHttpClient : IHttpClient
{
    /// <summary>
    /// Response resolvers for URLs.
    /// </summary>
    private readonly Dictionary<string, Func<HttpRequestMessage, HttpStringResponseMessage>> _urlResolvers = new Dictionary<string, Func<HttpRequestMessage, HttpStringResponseMessage>>();
    
    /// <summary>
    /// Sends a request and returns the response.
    /// </summary>
    /// <param name="request">Request to send.</param>
    /// <returns>Response for the request.</returns>
    public Task<HttpStringResponseMessage> SendAsync(HttpRequestMessage request)
    {
        // Throw an exception if there is no resolver.
        if (!this._urlResolvers.TryGetValue(request.RequestUri!.ToString(), out var resolver))
        {
            throw new KeyNotFoundException($"No response resolver for \"{request.RequestUri!.ToString()}\".");
        }
        
        // Return the response.
        return Task.FromResult(resolver(request));
    }

    /// <summary>
    /// Sets a response resolver for a URL.
    /// </summary>
    /// <param name="url">URL to resolve the response for.</param>
    /// <param name="resolver">Resolver for the response.</param>
    public void SetResponseResolver(string url, Func<HttpRequestMessage, HttpStringResponseMessage> resolver)
    {
        this._urlResolvers[url] = resolver;
    }

    /// <summary>
    /// Sets a response for a URL.
    /// </summary>
    /// <param name="url">URL of the request to return for.</param>
    /// <param name="statusCode">Status code of the response.</param>
    /// <param name="responseBody">Body of the response to return.</param>
    public void SetResponse(string url, HttpStatusCode statusCode, string responseBody)
    {
        this.SetResponseResolver(url, (_) => new HttpStringResponseMessage()
        {
            StatusCode = statusCode,
            Content = responseBody,
        });
    }
}