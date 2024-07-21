using System.Collections.Generic;
using Bouncer.Parser.Model;
using NUnit.Framework;

namespace Bouncer.Test.Parser.Model;

public class ParsedConditionTest
{
    [Test]
    public void TestAddOperator()
    {
        var condition = new ParsedCondition()
        {
            Operators = new List<string>() { "test1" },
        };
        Assert.That(condition.AddOperator("test2"), Is.EqualTo(condition));
        Assert.That(condition.Operators, Is.EqualTo(new List<string>() { "test1", "test2" }));
    }
    
    [Test]
    public void TestAddArguments()
    {
        var condition = new ParsedCondition()
        {
            Arguments = new List<string>() { "test1" },
        };
        Assert.That(condition.AddArguments(new List<string>() { "test2", "test3" }), Is.EqualTo(condition));
        Assert.That(condition.Arguments, Is.EqualTo(new List<string>() { "test1", "test2", "test3" }));
    }

    [Test]
    public void TestAddConditions()
    {
        var condition1 = new ParsedOperationWithCondition();
        var condition2 = new ParsedOperationWithCondition();
        var condition3 = new ParsedOperationWithCondition();
        var condition = new ParsedCondition()
        {
            Conditions = new List<ParsedOperationWithCondition>() { condition1 },
        };
        Assert.That(condition.AddConditions(new List<ParsedOperationWithCondition>() { condition2, condition3 }), Is.EqualTo(condition));
        Assert.That(condition.Conditions, Is.EqualTo(new List<ParsedOperationWithCondition>() { condition1, condition2, condition3 }));
    }

    [Test]
    public void TestFlattenConditions()
    {
        var condition = new ParsedCondition()
        {
            Name = "Test1",
            Conditions = new List<ParsedOperationWithCondition>()
            {
                new ParsedOperationWithCondition()
                {
                    Condition = new ParsedCondition()
                    {
                        Name = "Test2",
                        Conditions = new List<ParsedOperationWithCondition>()
                        {
                            new ParsedOperationWithCondition()
                            {
                                Condition = new ParsedCondition()
                                {
                                    Name = "Test3",
                                }
                            },
                        },
                    }
                },
                new ParsedOperationWithCondition()
                {
                    Condition = new ParsedCondition()
                    {
                        Name = "ConditionGroup",
                        Conditions = new List<ParsedOperationWithCondition>()
                        {
                            new ParsedOperationWithCondition()
                            {
                                Condition = new ParsedCondition()
                                {
                                    Name = "Test4",
                                }
                            },
                        },
                    }
                },
                new ParsedOperationWithCondition()
                {
                    Condition = new ParsedCondition()
                    {
                        Name = "Test5",
                        Conditions = new List<ParsedOperationWithCondition>()
                        {
                            new ParsedOperationWithCondition()
                            {
                                Condition = new ParsedCondition()
                                {
                                    Name = "ConditionGroup",
                                    Conditions = new List<ParsedOperationWithCondition>()
                                    {
                                        new ParsedOperationWithCondition()
                                        {
                                            Condition = new ParsedCondition()
                                            {
                                                Name = "Test6",
                                            }
                                        },
                                    },
                                }
                            },
                        },
                    }
                },
            },
        };
        
        Assert.That(condition.FlattenConditions(), Is.EqualTo(condition));
        Assert.That(condition.Conditions[0].Condition.Name, Is.EqualTo("Test2"));
        Assert.That(condition.Conditions[0].Condition.Conditions.Count, Is.EqualTo(0));
        Assert.That(condition.Conditions[1].Condition.Name, Is.EqualTo("Test3"));
        Assert.That(condition.Conditions[2].Condition.Name, Is.EqualTo("ConditionGroup"));
        Assert.That(condition.Conditions[2].Condition.Conditions[0].Condition.Name, Is.EqualTo("Test4"));
        Assert.That(condition.Conditions[3].Condition.Name, Is.EqualTo("Test5"));
        Assert.That(condition.Conditions[4].Condition.Name, Is.EqualTo("ConditionGroup"));
        Assert.That(condition.Conditions[4].Condition.Conditions[0].Condition.Name, Is.EqualTo("Test6"));
    }
}