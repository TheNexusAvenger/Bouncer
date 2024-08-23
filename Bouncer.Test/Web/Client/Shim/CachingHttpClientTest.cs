using System;
using System.Net;
using System.Net.Http;
using Bouncer.Web.Client.Shim;
using NUnit.Framework;

namespace Bouncer.Test.Web.Client.Shim;

public class CachingHttpClientTest
{
    
    private TestHttpClient _testHttpClient;
    private CachingHttpClient _client;

    [SetUp]
    public void SetUp()
    {
        _testHttpClient = new TestHttpClient();
        _client = new CachingHttpClient(_testHttpClient);
    }

    [Test]
    public void TestSendAsyncNonGetNoCaching()
    {
        this._testHttpClient.SetResponse("http://localhost/test", HttpStatusCode.OK, "Test1");
        var response = this._client.SendAsync(new HttpRequestMessage()
        {
            Method = HttpMethod.Post,
            RequestUri = new Uri("http://localhost/test"),
        });
        Assert.That(response.Result.Content, Is.EqualTo("Test1"));
        
        this._testHttpClient.SetResponse("http://localhost/test", HttpStatusCode.OK, "Test2");
        response = this._client.SendAsync(new HttpRequestMessage()
        {
            Method = HttpMethod.Post,
            RequestUri = new Uri("http://localhost/test"),
        });
        Assert.That(response.Result.Content, Is.EqualTo("Test2"));
    }

    [Test]
    public void TestSendAsyncDifferentUrlNoCaching()
    {
        this._testHttpClient.SetResponse("http://localhost/test1", HttpStatusCode.OK, "Test1");
        var response = this._client.SendAsync(new HttpRequestMessage()
        {
            Method = HttpMethod.Get,
            RequestUri = new Uri("http://localhost/test1"),
        });
        Assert.That(response.Result.Content, Is.EqualTo("Test1"));
        
        this._testHttpClient.SetResponse("http://localhost/test2", HttpStatusCode.OK, "Test2");
        response = this._client.SendAsync(new HttpRequestMessage()
        {
            Method = HttpMethod.Get,
            RequestUri = new Uri("http://localhost/test2"),
        });
        Assert.That(response.Result.Content, Is.EqualTo("Test2"));
    }

    [Test]
    public void TestSendAsyncCaching()
    {
        this._testHttpClient.SetResponse("http://localhost/test", HttpStatusCode.OK, "Test1");
        var response = this._client.SendAsync(new HttpRequestMessage()
        {
            Method = HttpMethod.Get,
            RequestUri = new Uri("http://localhost/test"),
        });
        Assert.That(response.Result.Content, Is.EqualTo("Test1"));
        
        this._testHttpClient.SetResponse("http://localhost/test", HttpStatusCode.OK, "Test2");
        response = this._client.SendAsync(new HttpRequestMessage()
        {
            Method = HttpMethod.Get,
            RequestUri = new Uri("http://localhost/test"),
        });
        Assert.That(response.Result.Content, Is.EqualTo("Test1"));
    }

    [Test]
    public void TestSendAsyncCachingException()
    {
        this._testHttpClient.SetResponseResolver("http://localhost/test", (_) => throw new Exception("Test exception"));
        Assert.Throws<AggregateException>(() =>
        {
            this._client.SendAsync(new HttpRequestMessage()
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri("http://localhost/test"),
            }).Wait();
        });
        
        this._testHttpClient.SetResponse("http://localhost/test", HttpStatusCode.OK, "Test");
        Assert.Throws<AggregateException>(() =>
        {
            this._client.SendAsync(new HttpRequestMessage()
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri("http://localhost/test"),
            }).Wait();
        });
    }

    [Test]
    public void TestSendAsyncClearCaching()
    {
        this._testHttpClient.SetResponse("http://localhost/test", HttpStatusCode.OK, "Test1");
        var response = this._client.SendAsync(new HttpRequestMessage()
        {
            Method = HttpMethod.Get,
            RequestUri = new Uri("http://localhost/test"),
        });
        Assert.That(response.Result.Content, Is.EqualTo("Test1"));
        this._client.ClearCacheAsync().Wait();
        
        this._testHttpClient.SetResponse("http://localhost/test", HttpStatusCode.OK, "Test2");
        response = this._client.SendAsync(new HttpRequestMessage()
        {
            Method = HttpMethod.Get,
            RequestUri = new Uri("http://localhost/test"),
        });
        Assert.That(response.Result.Content, Is.EqualTo("Test2"));
    }
}