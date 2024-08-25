using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using Bouncer.State;
using Bouncer.State.Loop;
using Bouncer.Test.Web.Client.Shim;
using Bouncer.Web.Client;
using Bouncer.Web.Client.Response;
using NUnit.Framework;

namespace Bouncer.Test.State.Loop;

public class GroupJoinRequestLoopTest
{
    private TestHttpClient _testHttpClient;
    private GroupJoinRequestLoop _loop;
    
    [SetUp]
    public void SetUp()
    {
        _testHttpClient = new TestHttpClient();
        _loop = new GroupJoinRequestLoop(12345L, new RobloxGroupClient(_testHttpClient, _testHttpClient));
    }

    [Test]
    public void TestRunAsyncAccept()
    {
        this._testHttpClient.SetResponse("https://apis.roblox.com/cloud/v2/groups/12345/join-requests?maxPageSize=20", HttpStatusCode.OK, "{\"groupJoinRequests\":[{\"user\":\"users/123\"}]}");
        this._testHttpClient.SetResponse("https://apis.roblox.com/cloud/v2/groups/12345/join-requests/123:accept", HttpStatusCode.NoContent, "{}");
        this._loop.SetRules(new List<JoinRequestRuleEntry>()
        {
            new JoinRequestRuleEntry() {
                Rule = "Always()",
                Action = JoinRequestAction.Accept,
            },
        });
        this._loop.RunAsync().Wait();
        Assert.That(this._loop.Status, Is.EqualTo(GroupJoinRequestLoopStatus.Complete));
    }

    [Test]
    public void TestRunAsyncDecline()
    {
        this._testHttpClient.SetResponse("https://apis.roblox.com/cloud/v2/groups/12345/join-requests?maxPageSize=20", HttpStatusCode.OK, "{\"groupJoinRequests\":[{\"user\":\"users/123\"}]}");
        this._testHttpClient.SetResponse("https://apis.roblox.com/cloud/v2/groups/12345/join-requests/123:decline", HttpStatusCode.NoContent, "{}");
        this._loop.SetRules(new List<JoinRequestRuleEntry>()
        {
            new JoinRequestRuleEntry() {
                Rule = "Always()",
                Action = JoinRequestAction.Decline,
            },
        });
        this._loop.RunAsync().Wait();
        Assert.That(this._loop.Status, Is.EqualTo(GroupJoinRequestLoopStatus.Complete));
    }

    [Test]
    public void TestRunAsyncIgnore()
    {
        this._testHttpClient.SetResponse("https://apis.roblox.com/cloud/v2/groups/12345/join-requests?maxPageSize=20", HttpStatusCode.OK, "{\"groupJoinRequests\":[{\"user\":\"users/123\"}]}");
        this._loop.SetRules(new List<JoinRequestRuleEntry>()
        {
            new JoinRequestRuleEntry() {
                Rule = "Always()",
                Action = JoinRequestAction.Ignore,
            },
        });
        this._loop.RunAsync().Wait();
        Assert.That(this._loop.Status, Is.EqualTo(GroupJoinRequestLoopStatus.Complete));
    }

    [Test]
    public void TestRunAsyncDefault()
    {
        this._testHttpClient.SetResponse("https://apis.roblox.com/cloud/v2/groups/12345/join-requests?maxPageSize=20", HttpStatusCode.OK, "{\"groupJoinRequests\":[{\"user\":\"users/123\"}]}");
        this._loop.SetRules(new List<JoinRequestRuleEntry>()
        {
            new JoinRequestRuleEntry() {
                Rule = "Never()",
                Action = JoinRequestAction.Accept,
            },
        });
        this._loop.RunAsync().Wait();
        Assert.That(this._loop.Status, Is.EqualTo(GroupJoinRequestLoopStatus.Complete));
    }

    [Test]
    public void TestRunAsyncSameNextPageToken()
    {
        this._testHttpClient.SetResponse("https://apis.roblox.com/cloud/v2/groups/12345/join-requests?maxPageSize=20", HttpStatusCode.OK, "{\"groupJoinRequests\":[{\"user\":\"users/123\"}],\"nextPageToken\":\"token\"}");
        this._testHttpClient.SetResponse("https://apis.roblox.com/cloud/v2/groups/12345/join-requests?maxPageSize=20&pageToken=token", HttpStatusCode.OK, "{\"groupJoinRequests\":[{\"user\":\"users/123\"}],\"nextPageToken\":\"token\"}");
        var exception = Assert.Throws<AggregateException>(() =>
        {
            this._loop.RunAsync().Wait();
        });
        Assert.That(exception.InnerException!.GetType(), Is.EqualTo(typeof(InvalidDataException)));
        Assert.That(this._loop.Status, Is.EqualTo(GroupJoinRequestLoopStatus.Error));
    }

    [Test]
    public void TestRunAsyncTooManyRequests()
    {
        this._testHttpClient.SetResponse("https://apis.roblox.com/cloud/v2/groups/12345/join-requests?maxPageSize=20", HttpStatusCode.TooManyRequests, "{\"code\":\"RESOURCE_EXHAUSTED\"}");
        this._loop.RunAsync().Wait();
        Assert.That(this._loop.Status, Is.EqualTo(GroupJoinRequestLoopStatus.TooManyRequests));
    }

    [Test]
    public void TestRunAsyncInvalidApiKey()
    {
        this._testHttpClient.SetResponse("https://apis.roblox.com/cloud/v2/groups/12345/join-requests?maxPageSize=20", HttpStatusCode.Unauthorized, "{\"errors\":[{\"code\":0,\"message\":\"Invalid API Key\"}]}");
        var exception = Assert.Throws<AggregateException>(() =>
        {
            this._loop.RunAsync().Wait();
        });
        Assert.That(exception.InnerException! is OpenCloudAccessException, Is.True);
        Assert.That(this._loop.Status, Is.EqualTo(GroupJoinRequestLoopStatus.InvalidApiKey));
    }

    [Test]
    public void TestRunAsyncUnknownAccessError()
    {
        this._testHttpClient.SetResponse("https://apis.roblox.com/cloud/v2/groups/12345/join-requests?maxPageSize=20", HttpStatusCode.ServiceUnavailable, "{}");
        var exception = Assert.Throws<AggregateException>(() =>
        {
            this._loop.RunAsync().Wait();
        });
        Assert.That(exception.InnerException! is OpenCloudAccessException, Is.True);
        Assert.That(this._loop.Status, Is.EqualTo(GroupJoinRequestLoopStatus.Error));
    }
}