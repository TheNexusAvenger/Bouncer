using System.Collections.Generic;
using Bouncer.Diagnostic;
using Bouncer.Expression;
using Bouncer.Expression.Definition;
using Bouncer.State;
using NUnit.Framework;

namespace Bouncer.Test.Diagnostic;

public class ConfigurationVerificationTest
{
    [OneTimeSetUp]
    public void SetUp()
    {
        Condition.AddConditionDefinition(new ConditionDefinition()
        {
            Name = "Test",
            TotalArguments = 0,
            Evaluate = (_, _) => true,
        });
    }
    
    [Test]
    public void TestVerifyRules()
    {
        var result = ConfigurationVerification.VerifyRules(new Configuration()
        {
            Groups = new List<GroupConfiguration>()
            {
                new GroupConfiguration()
                {
                    Id = 12345L,
                    OpenCloudApiKey = "test",
                    Rules = new List<JoinRequestRuleEntry>()
                    {
                        new JoinRequestRuleEntry()
                        {
                            Name = "Test Rule",
                            Rule = "Test()",
                            Action = JoinRequestAction.Accept,
                        },
                    },
                },
            },
        });
        Assert.That(result.TotalRuleConfigurationErrors, Is.EqualTo(0));
        Assert.That(result.TotalParseErrors, Is.EqualTo(0));
        Assert.That(result.TotalTransformErrors, Is.EqualTo(0));
    }
    
    [Test]
    public void TestVerifyRulesNullGroups()
    {
        var result = ConfigurationVerification.VerifyRules(new Configuration()
        {
            Groups = null,
        });
        Assert.That(result.TotalRuleConfigurationErrors, Is.EqualTo(1));
        Assert.That(result.TotalParseErrors, Is.EqualTo(0));
        Assert.That(result.TotalTransformErrors, Is.EqualTo(0));
    }
    
    [Test]
    public void TestVerifyRulesNullGroupConfigurationFields()
    {
        var result = ConfigurationVerification.VerifyRules(new Configuration()
        {
            Groups = new List<GroupConfiguration>()
            {
                new GroupConfiguration()
                {
                    Id = null,
                    OpenCloudApiKey = null,
                    Rules = null,
                },
            },
        });
        Assert.That(result.TotalRuleConfigurationErrors, Is.EqualTo(3));
        Assert.That(result.TotalParseErrors, Is.EqualTo(0));
        Assert.That(result.TotalTransformErrors, Is.EqualTo(0));
    }
    
    [Test]
    public void TestVerifyRulesNullRuleFields()
    {
        var result = ConfigurationVerification.VerifyRules(new Configuration()
        {
            Groups = new List<GroupConfiguration>()
            {
                new GroupConfiguration()
                {
                    Id = 12345L,
                    OpenCloudApiKey = "test",
                    Rules = new List<JoinRequestRuleEntry>()
                    {
                        new JoinRequestRuleEntry()
                        {
                            Name = null,
                            Rule = null,
                            Action = null,
                        },
                    },
                },
            },
        });
        Assert.That(result.TotalRuleConfigurationErrors, Is.EqualTo(2));
        Assert.That(result.TotalParseErrors, Is.EqualTo(0));
        Assert.That(result.TotalTransformErrors, Is.EqualTo(0));
    }
    
    [Test]
    public void TestVerifyRulesParseError()
    {
        var result = ConfigurationVerification.VerifyRules(new Configuration()
        {
            Groups = new List<GroupConfiguration>()
            {
                new GroupConfiguration()
                {
                    Id = 12345L,
                    OpenCloudApiKey = "test",
                    Rules = new List<JoinRequestRuleEntry>()
                    {
                        new JoinRequestRuleEntry()
                        {
                            Name = "Test Rule",
                            Rule = "Test(",
                            Action = JoinRequestAction.Accept,
                        },
                    },
                },
            },
        });
        Assert.That(result.TotalRuleConfigurationErrors, Is.EqualTo(0));
        Assert.That(result.TotalParseErrors, Is.EqualTo(1));
        Assert.That(result.TotalTransformErrors, Is.EqualTo(0));
    }
    
    [Test]
    public void TestVerifyRulesTransformError()
    {
        var result = ConfigurationVerification.VerifyRules(new Configuration()
        {
            Groups = new List<GroupConfiguration>()
            {
                new GroupConfiguration()
                {
                    Id = 12345L,
                    OpenCloudApiKey = "test",
                    Rules = new List<JoinRequestRuleEntry>()
                    {
                        new JoinRequestRuleEntry()
                        {
                            Name = "Test Rule",
                            Rule = "Unknown()",
                            Action = JoinRequestAction.Accept,
                        },
                    },
                },
            },
        });
        Assert.That(result.TotalRuleConfigurationErrors, Is.EqualTo(0));
        Assert.That(result.TotalParseErrors, Is.EqualTo(0));
        Assert.That(result.TotalTransformErrors, Is.EqualTo(1));
    }
}