﻿using Bouncer.Diagnostic.Model;
using Bouncer.Web.Server;
using Bouncer.Web.Server.Model;
using NUnit.Framework;

namespace Bouncer.Test.Web.Server;

public class HealthCheckStateTest
{
    [Test]
    public void TestGetHealthCheckResultNoErrors()
    {
        var result = new HealthCheckState().GetHealthCheckResult(new VerifyRulesResult()
        {
            TotalRuleConfigurationErrors = 0,
            TotalParseErrors = 0,
            TotalTransformErrors = 0,
        });
        
        Assert.That(result.Status, Is.EqualTo(HealthCheckResultStatus.Up));
        Assert.That(result.Configuration.Status, Is.EqualTo(HealthCheckResultStatus.Up));
        Assert.That(result.Configuration.TotalRuleConfigurationErrors, Is.EqualTo(0));
        Assert.That(result.Configuration.TotalRuleParseErrors, Is.EqualTo(0));
        Assert.That(result.Configuration.TotalRuleTransformErrors, Is.EqualTo(0));
    }
        
    [Test]
    public void TestGetHealthCheckResultRuleErrors()
    {
        var result = new HealthCheckState().GetHealthCheckResult(new VerifyRulesResult()
        {
            TotalRuleConfigurationErrors = 1,
            TotalParseErrors = 2,
            TotalTransformErrors = 3,
        });
        
        Assert.That(result.Status, Is.EqualTo(HealthCheckResultStatus.Down));
        Assert.That(result.Configuration.Status, Is.EqualTo(HealthCheckResultStatus.Down));
        Assert.That(result.Configuration.TotalRuleConfigurationErrors, Is.EqualTo(1));
        Assert.That(result.Configuration.TotalRuleParseErrors, Is.EqualTo(2));
        Assert.That(result.Configuration.TotalRuleTransformErrors, Is.EqualTo(3));
    }
}