using System.Net;
using Bouncer.Test.Web.Client.Shim;
using Bouncer.Web.Client;
using NUnit.Framework;

namespace Bouncer.Test.Web.Client;

public class RobloxGroupClientTest
{
    private TestHttpClient _testHttpClient;
    private RobloxGroupClient _client;

    [SetUp]
    public void SetUp()
    {
        _testHttpClient = new TestHttpClient();
        _client = new RobloxGroupClient(_testHttpClient, _testHttpClient);
    }

    [Test]
    public void TestGetRankInGroupAsync()
    {
        this._testHttpClient.SetResponse("https://groups.roblox.com/v2/users/1/groups/roles", HttpStatusCode.OK, "{\"data\":[{\"group\":{\"id\":12345},\"role\":{\"rank\":5}}]}");
        Assert.That(this._client.GetRankInGroupAsync(1, 12345).Result, Is.EqualTo(5));
    }

    [Test]
    public void TestGetRankInGroupAsyncNotInGroup()
    {
        this._testHttpClient.SetResponse("https://groups.roblox.com/v2/users/1/groups/roles", HttpStatusCode.OK, "{\"data\":[{\"group\":{\"id\":12345},\"role\":{\"rank\":5}}]}");
        Assert.That(this._client.GetRankInGroupAsync(1, 123).Result, Is.EqualTo(0));
    }

    [Test]
    public void TestGetJoinRequests()
    {
        this._testHttpClient.SetResponse("https://apis.roblox.com/cloud/v2/groups/12345/join-requests?maxPageSize=1", HttpStatusCode.OK, "{\"groupJoinRequests\":[{\"path\":\"groups/12345/join-requests/156\",\"createDate\":\"2023-07-05T12:34:56Z\",\"user\":\"users/156\"}],\"nextPageToken\":\"token\"}");
        var response = this._client.GetJoinRequests(12345, null, 1).Result;
        Assert.That(response.GroupJoinRequests[0].UserId, Is.EqualTo(156));
    }

    [Test]
    public void TestGetJoinRequestsToken()
    {
        this._testHttpClient.SetResponse("https://apis.roblox.com/cloud/v2/groups/12345/join-requests?maxPageSize=1&pageToken=token", HttpStatusCode.OK, "{\"groupJoinRequests\":[{\"path\":\"groups/12345/join-requests/156\",\"createDate\":\"2023-07-05T12:34:56Z\",\"user\":\"users/156\"}],\"nextPageToken\":\"token\"}");
        var response = this._client.GetJoinRequests(12345, "token", 1).Result;
        Assert.That(response.GroupJoinRequests[0].UserId, Is.EqualTo(156));
    }
}