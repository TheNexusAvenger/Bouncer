using System;
using System.Net;
using System.Net.Http;
using Bouncer.Test.Web.Client.Shim;
using Bouncer.Web.Client;
using Bouncer.Web.Client.Response;
using NUnit.Framework;

namespace Bouncer.Test.Web.Client;

public class RobloxOpenCloudClientTest
{
    private TestHttpClient _testHttpClient;
    private RobloxOpenCloudClient _client;

    [SetUp]
    public void SetUp()
    {
        _testHttpClient = new TestHttpClient();
        _client = new RobloxOpenCloudClient(_testHttpClient);
    }
    
    [Test]
    public void TestRequestAsyncMissingApiKey()
    {
        this._testHttpClient.SetResponse("https://apis.roblox.com/cloud/v2/test", HttpStatusCode.Unauthorized, "{\"errors\":[{\"code\":0,\"message\":\"Missing API Key Header\"}]}");
        var exception = Assert.Throws<AggregateException>(() =>
        {
            this._client.RequestAsync(HttpMethod.Get, "https://apis.roblox.com/cloud/v2/test", BaseRobloxOpenCloudResponseJsonContext.Default.BaseRobloxOpenCloudResponse).Wait();
        });
        var openCloudException = (OpenCloudAccessException<BaseRobloxOpenCloudResponse>) exception.InnerException!;
        Assert.That(openCloudException.Issue, Is.EqualTo(OpenCloudAccessIssue.MissingApiKey));
        Assert.That(openCloudException.Response.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
    }
    
    [Test]
    public void TestRequestAsyncInvalidApiKey()
    {
        this._testHttpClient.SetResponse("https://apis.roblox.com/cloud/v2/test", HttpStatusCode.Unauthorized, "{\"errors\":[{\"code\":0,\"message\":\"Invalid API Key\"}]}");
        var exception = Assert.Throws<AggregateException>(() =>
        {
            this._client.RequestAsync(HttpMethod.Get, "https://apis.roblox.com/cloud/v2/test", BaseRobloxOpenCloudResponseJsonContext.Default.BaseRobloxOpenCloudResponse).Wait();
        });
        var openCloudException = (OpenCloudAccessException<BaseRobloxOpenCloudResponse>) exception.InnerException!;
        Assert.That(openCloudException.Issue, Is.EqualTo(OpenCloudAccessIssue.InvalidApiKey));
        Assert.That(openCloudException.Response.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
    }
    
    [Test]
    public void TestRequestAsyncPermissionDenied()
    {
        this._testHttpClient.SetResponse("https://apis.roblox.com/cloud/v2/test", HttpStatusCode.Forbidden, "{\"code\":\"PERMISSION_DENIED\"}");
        var exception = Assert.Throws<AggregateException>(() =>
        {
            this._client.RequestAsync(HttpMethod.Get, "https://apis.roblox.com/cloud/v2/test", BaseRobloxOpenCloudResponseJsonContext.Default.BaseRobloxOpenCloudResponse).Wait();
        });
        var openCloudException = (OpenCloudAccessException<BaseRobloxOpenCloudResponse>) exception.InnerException!;
        Assert.That(openCloudException.Issue, Is.EqualTo(OpenCloudAccessIssue.PermissionDenied));
        Assert.That(openCloudException.Response.StatusCode, Is.EqualTo(HttpStatusCode.Forbidden));
    }
    
    [Test]
    public void TestRequestAsyncInvalidScope()
    {
        this._testHttpClient.SetResponse("https://apis.roblox.com/cloud/v2/test", HttpStatusCode.Forbidden, "{\"code\":\"INSUFFICIENT_SCOPE\"}");
        var exception = Assert.Throws<AggregateException>(() =>
        {
            this._client.RequestAsync(HttpMethod.Get, "https://apis.roblox.com/cloud/v2/test", BaseRobloxOpenCloudResponseJsonContext.Default.BaseRobloxOpenCloudResponse).Wait();
        });
        var openCloudException = (OpenCloudAccessException<BaseRobloxOpenCloudResponse>) exception.InnerException!;
        Assert.That(openCloudException.Issue, Is.EqualTo(OpenCloudAccessIssue.PermissionDenied));
        Assert.That(openCloudException.Response.StatusCode, Is.EqualTo(HttpStatusCode.Forbidden));
    }
    
    [Test]
    public void TestRequestAsyncInvalidUser()
    {
        this._testHttpClient.SetResponse("https://apis.roblox.com/cloud/v2/test", HttpStatusCode.Forbidden, "{\"code\":\"UNAUTHENTICATED\"}");
        var exception = Assert.Throws<AggregateException>(() =>
        {
            this._client.RequestAsync(HttpMethod.Get, "https://apis.roblox.com/cloud/v2/test", BaseRobloxOpenCloudResponseJsonContext.Default.BaseRobloxOpenCloudResponse).Wait();
        });
        var openCloudException = (OpenCloudAccessException<BaseRobloxOpenCloudResponse>) exception.InnerException!;
        Assert.That(openCloudException.Issue, Is.EqualTo(OpenCloudAccessIssue.Unauthenticated));
        Assert.That(openCloudException.Response.StatusCode, Is.EqualTo(HttpStatusCode.Forbidden));
    }
    
    [Test]
    public void TestRequestAsyncTooManyRequests()
    {
        this._testHttpClient.SetResponse("https://apis.roblox.com/cloud/v2/test", HttpStatusCode.TooManyRequests, "{\"code\":\"RESOURCE_EXHAUSTED\"}");
        var exception = Assert.Throws<AggregateException>(() =>
        {
            this._client.RequestAsync(HttpMethod.Get, "https://apis.roblox.com/cloud/v2/test", BaseRobloxOpenCloudResponseJsonContext.Default.BaseRobloxOpenCloudResponse).Wait();
        });
        var openCloudException = (OpenCloudAccessException<BaseRobloxOpenCloudResponse>) exception.InnerException!;
        Assert.That(openCloudException.Issue, Is.EqualTo(OpenCloudAccessIssue.TooManyRequests));
        Assert.That(openCloudException.Response.StatusCode, Is.EqualTo(HttpStatusCode.TooManyRequests));
    }
    
    [Test]
    public void TestGetAsync()
    {
        this._testHttpClient.SetResponse("https://apis.roblox.com/cloud/v2/test", HttpStatusCode.OK, "{\"code\":\"SUCCESS\"}");
        var response = this._client.GetAsync("https://apis.roblox.com/cloud/v2/test", BaseRobloxOpenCloudResponseJsonContext.Default.BaseRobloxOpenCloudResponse).Result;
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        Assert.That(response.Code, Is.EqualTo("SUCCESS"));
    }
}