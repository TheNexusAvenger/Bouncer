using System;
using System.Collections.Generic;
using Bouncer.Expression;
using Bouncer.Expression.Definition;
using Bouncer.Parser.Model;
using NUnit.Framework;

namespace Bouncer.Test.Expression;

public class ConditionTest
{
    [OneTimeSetUp]
    public void SetUp()
    {
        Condition.AddConditionDefinition(new ConditionDefinition()
        {
            Name = "Test1",
            TotalArguments = 1,
            Evaluate = (_, _) => true,
        });
        Condition.AddConditionDefinition(new ConditionDefinition()
        {
            Name = "Test2",
            TotalArguments = 1,
            Evaluate = (_, _) => true,
        });
        Condition.AddConditionDefinition(new ConditionDefinition()
        {
            Name = "Test2",
            TotalArguments = 2,
            Evaluate = (_, _) => true,
        });
        Condition.AddConditionDefinition(new ConditionDefinition()
        {
            Name = "Test3",
            TotalArguments = 0,
            Evaluate = (_, _) => true,
        });
        Condition.AddConditionDefinition(new ConditionDefinition()
        {
            Name = "Test4",
            TotalArguments = 0,
            Evaluate = (_, _) => false,
        });
        Condition.AddConditionDefinition(new ConditionDefinition()
        {
            Name = "Test5",
            TotalArguments = 0,
            Evaluate = (_, _) => throw new Exception("Should not have run."),
        });
        Condition.AddConditionDefinition(new ConditionDefinition()
        {
            Name = "Test6",
            FormatString = "Checks for {0} and {1}.",
            TotalArguments = 2,
            Evaluate = (_, _) => true,
        });
    }

    [Test]
    public void TestAddConditionDefinitionDuplicate()
    {
        
        var exception = Assert.Throws<InvalidOperationException>(() =>
        {
            Condition.AddConditionDefinition(new ConditionDefinition()
            {
                Name = "Test2",
                TotalArguments = 2,
                Evaluate = (_, _) => true,
            });
        });
        Assert.That(exception.Message, Is.EqualTo("There is an existing condition named \"Test2\" with 2 arguments."));
    }

    [Test]
    public void TestFromParsedCondition()
    {
        var condition = Condition.FromParsedCondition(new ParsedCondition()
        {
            Name = "test1",
            Arguments = new List<string>() { "test1" },
        });
        Assert.That(condition.ConditionDefinition.Name, Is.EqualTo("Test1"));
        Assert.That(condition.ConditionDefinition.TotalArguments, Is.EqualTo(1));
    }

    [Test]
    public void TestFromParsedConditionMultipleArgumentLengths()
    {
        var condition1 = Condition.FromParsedCondition(new ParsedCondition()
        {
            Name = "Test2",
            Arguments = new List<string>() { "test1" },
        });
        Assert.That(condition1.ConditionDefinition.Name, Is.EqualTo("Test2"));
        Assert.That(condition1.ConditionDefinition.TotalArguments, Is.EqualTo(1));
        
        var condition2 = Condition.FromParsedCondition(new ParsedCondition()
        {
            Name = "Test2",
            Arguments = new List<string>() { "test1", "test2" },
        });
        Assert.That(condition2.ConditionDefinition.Name, Is.EqualTo("Test2"));
        Assert.That(condition2.ConditionDefinition.TotalArguments, Is.EqualTo(2));
    }

    [Test]
    public void TestFromParsedConditionUnknownCondition()
    {
        var exception = Assert.Throws<InvalidOperationException>(() =>
        {
            Condition.FromParsedCondition(new ParsedCondition()
            {
                Name = "Test0",
            });
        });
        Assert.That(exception.Message, Is.EqualTo("There are no conditions named \"Test0\"."));
    }

    [Test]
    public void TestFromParsedConditionMismatchParameters()
    {
        var exception = Assert.Throws<InvalidOperationException>(() =>
        {
            Condition.FromParsedCondition(new ParsedCondition()
            {
                Name = "Test1",
                Arguments = new List<string>() { "test1", "test2" },
            });
        });
        Assert.That(exception.Message, Is.EqualTo("The condition \"Test1\" expects 1 arguments but 2 were given."));
    }

    [Test]
    public void TestFromParsedConditionMismatchParametersMultipleOptions()
    {
        var exception = Assert.Throws<InvalidOperationException>(() =>
        {
            Condition.FromParsedCondition(new ParsedCondition()
            {
                Name = "Test2",
                Arguments = new List<string>() { "test1", "test2", "test3" },
            });
        });
        Assert.That(exception.Message, Is.EqualTo("The condition \"Test2\" expects any of [1, 2] arguments but 3 were given."));
    }

    [Test]
    public void TestFromParsedConditionUnknownUnaryOperation()
    {
        var exception = Assert.Throws<InvalidOperationException>(() =>
        {
            Condition.FromParsedCondition(new ParsedCondition()
            {
                Name = "Test1",
                Arguments = new List<string>() { "test1" },
                Operators = new List<string>() { "test" },
            });
        });
        Assert.That(exception.Message, Is.EqualTo("There are no operations named \"test\"."));
    }

    [Test]
    public void TestFromParsedConditionUnknownBinaryOperation()
    {
        var exception = Assert.Throws<InvalidOperationException>(() =>
        {
            Condition.FromParsedCondition(new ParsedCondition()
            {
                Name = "Test1",
                Arguments = new List<string>() { "test1" },
                Conditions = new List<ParsedOperationWithCondition>()
                {
                    new ParsedOperationWithCondition()
                    {
                        Operator = "test",
                        Condition = new ParsedCondition()
                        {
                            Name = "Test1",
                            Arguments = new List<string>() { "test1" },
                        },
                    }
                },
            });
        });
        Assert.That(exception.Message, Is.EqualTo("There are no operations named \"test\"."));
    }

    [Test]
    public void TestEvaluate()
    {
        var result = Condition.FromParsedCondition("Test1(test1)").Evaluate(123L);
        Assert.That(result, Is.EqualTo(true));
    }

    [Test]
    public void TestEvaluateUnaryOperation()
    {
        var result = Condition.FromParsedCondition("not Test1(test1)").Evaluate(123L);
        Assert.That(result, Is.EqualTo(false));
    }

    [Test]
    public void TestEvaluateBinaryOperation()
    {
        var result = Condition.FromParsedCondition("Test3() and Test4()").Evaluate(123L);
        Assert.That(result, Is.EqualTo(false));
    }

    [Test]
    public void TestEvaluateBinaryOperationOptimizedAnd()
    {
        var result = Condition.FromParsedCondition("Test4() and Test5()").Evaluate(123L);
        Assert.That(result, Is.EqualTo(false));
    }

    [Test]
    public void TestEvaluateBinaryOperationOptimizedOr()
    {
        var result = Condition.FromParsedCondition("Test3() or Test5()").Evaluate(123L);
        Assert.That(result, Is.EqualTo(true));
    }

    [Test]
    public void TestEvaluateBinaryOperationAndThenOr()
    {
        var result1 = Condition.FromParsedCondition("Test3() and Test4() or Test3()").Evaluate(123L);
        Assert.That(result1, Is.EqualTo(true));
        var result2 = Condition.FromParsedCondition("(Test3() and Test4() or Test3())").Evaluate(123L);
        Assert.That(result2, Is.EqualTo(true));
    }
    
    [Test]
    public void TestEvaluateBinaryOperationNotWithoutGroups()
    {
        var result1 = Condition.FromParsedCondition("not Test3() and Test3()").Evaluate(123L);
        Assert.That(result1, Is.EqualTo(false));
        var result2 = Condition.FromParsedCondition("Test3() and not Test3()").Evaluate(123L);
        Assert.That(result2, Is.EqualTo(false));
        var result3 = Condition.FromParsedCondition("not Test3() and not Test3()").Evaluate(123L);
        Assert.That(result3, Is.EqualTo(true));
    }

    [Test]
    public void TestEvaluateBinaryOperationNotWithGroups()
    {
        var result1 = Condition.FromParsedCondition("not (Test3() and Test3())").Evaluate(123L);
        Assert.That(result1, Is.EqualTo(false));
        var result2 = Condition.FromParsedCondition("not (Test3() and Test4())").Evaluate(123L);
        Assert.That(result2, Is.EqualTo(true));
        var result3 = Condition.FromParsedCondition("not (not Test3() and Test4())").Evaluate(123L);
        Assert.That(result3, Is.EqualTo(true));
        var result4 = Condition.FromParsedCondition("not (not Test3() and not Test4())").Evaluate(123L);
        Assert.That(result4, Is.EqualTo(true));
    }

    [Test]
    public void TestToString()
    {
        var result = Condition.FromParsedCondition("Test2(test1, test2)").ToString();
        Assert.That(result, Is.EqualTo("Test2(\"test1\", \"test2\")"));
    }

    [Test]
    public void TestToStringUnaryOperations()
    {
        var result = Condition.FromParsedCondition("not not Test2(test1, test2)").ToString();
        Assert.That(result, Is.EqualTo("not not Test2(\"test1\", \"test2\")"));
    }

    [Test]
    public void TestToStringBinaryOperations()
    {
        var result = Condition.FromParsedCondition("Test1(test1) and Test2(test1, test2) or Test2(test1)").ToString();
        Assert.That(result, Is.EqualTo("Test1(\"test1\") and Test2(\"test1\", \"test2\") or Test2(\"test1\")"));
    }

    [Test]
    public void TestToStringGroups()
    {
        var result = Condition.FromParsedCondition("(Test1(test1) and not (Test2(test1, test2) or (Test2(test1))) and Test2(test1))").ToString();
        Assert.That(result, Is.EqualTo("(Test1(\"test1\") and not (Test2(\"test1\", \"test2\") or (Test2(\"test1\"))) and Test2(\"test1\"))"));
    }

    [Test]
    public void TestToStringFormatted()
    {
        var result = Condition.FromParsedCondition("not (not Test6(test1, test2) and Test1(test1))").ToString();
        Assert.That(result, Is.EqualTo("not (not [Checks for test1 and test2.] and Test1(\"test1\"))"));
    }
}