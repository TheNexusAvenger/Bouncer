using System.Collections.Generic;
using System.Linq;
using Bouncer.Parser;
using Bouncer.Parser.Model;
using NUnit.Framework;
using Sprache;

namespace Bouncer.Test.Parser;

public class ExpressionParserTest
{
    [Test]
    public void TestEscapedArgumentCharacterParser()
    {
        var result = ExpressionParser.EscapedArgumentCharacterParser.TryParse("\\\"");
        Assert.That(result.WasSuccessful, Is.True);
        Assert.That(result.Value, Is.EqualTo('\"'));
    }
    
    [Test]
    public void TestEscapedArgumentCharacterParserAtEnd()
    {
        var result = ExpressionParser.EscapedArgumentCharacterParser.TryParse("");
        Assert.That(result.WasSuccessful, Is.False);
        Assert.That(result.Message, Is.EqualTo("Unexpected end of input reached."));
        Assert.That(result.Expectations.First(), Is.EqualTo("The character \\ followed by any character."));
    }
    
    [Test]
    public void TestEscapedArgumentCharacterParserNonEscapeCharacter()
    {
        var result = ExpressionParser.EscapedArgumentCharacterParser.TryParse("!");
        Assert.That(result.WasSuccessful, Is.False);
        Assert.That(result.Message, Is.EqualTo("Character is not \\."));
        Assert.That(result.Expectations.First(), Is.EqualTo("The character \\ followed by any character."));
    }
    
    [Test]
    public void TestEscapedArgumentCharacterParserOnlyEscapeCharacter()
    {
        var result = ExpressionParser.EscapedArgumentCharacterParser.TryParse("\\");
        Assert.That(result.WasSuccessful, Is.False);
        Assert.That(result.Message, Is.EqualTo("Unexpected end of input reached."));
        Assert.That(result.Expectations.First(), Is.EqualTo("The character \\ followed by any character."));
    }
    
    [Test]
    public void TestQuotedArgumentParser()
    {
        var result = ExpressionParser.QuotedArgumentParser.TryParse("\"Test\"");
        Assert.That(result.WasSuccessful, Is.True);
        Assert.That(result.Value, Is.EqualTo("Test"));
    }
    
    [Test]
    public void TestQuotedArgumentParserEscapedCharacters()
    {
        var result = ExpressionParser.QuotedArgumentParser.TryParse("\"\\\"Te\\\"st\\!\\\"\"");
        Assert.That(result.WasSuccessful, Is.True);
        Assert.That(result.Value, Is.EqualTo("\"Te\"st!\""));
    }
    
    [Test]
    public void TestQuotedArgumentParserEmptyString()
    {
        var result = ExpressionParser.QuotedArgumentParser.TryParse("\"\"");
        Assert.That(result.WasSuccessful, Is.True);
        Assert.That(result.Value, Is.EqualTo(""));
    }
    
    [Test]
    public void TestQuotedArgumentParserMissingEndQuote()
    {
        var result = ExpressionParser.QuotedArgumentParser.TryParse("\"Test");
        Assert.That(result.WasSuccessful, Is.False);
    }
    
    [Test]
    public void TestArgumentParser()
    {
        var result = ExpressionParser.ArgumentParser.TryParse("Test");
        Assert.That(result.WasSuccessful, Is.True);
        Assert.That(result.Value, Is.EqualTo("Test"));
    }
    
    [Test]
    public void TestArgumentParserQuotes()
    {
        var result = ExpressionParser.ArgumentParser.TryParse("\"Test\"");
        Assert.That(result.WasSuccessful, Is.True);
        Assert.That(result.Value, Is.EqualTo("Test"));
    }
    
    [Test]
    public void TestArgumentParserEscapedQuotes()
    {
        var result = ExpressionParser.ArgumentParser.TryParse("\"Te\\\"st\"");
        Assert.That(result.WasSuccessful, Is.True);
        Assert.That(result.Value, Is.EqualTo("Te\"st"));
    }
    
    [Test]
    public void TestArgumentParserWhitespace()
    {
        var result = ExpressionParser.ArgumentParser.TryParse("  Test Test  ");
        Assert.That(result.WasSuccessful, Is.True);
        Assert.That(result.Value, Is.EqualTo("Test Test"));
    }
    
    [Test]
    public void TestArgumentParserQuotesWhitespace()
    {
        var result = ExpressionParser.ArgumentParser.TryParse("  \"Test Test\"  ");
        Assert.That(result.WasSuccessful, Is.True);
        Assert.That(result.Value, Is.EqualTo("Test Test"));
    }

    [Test]
    public void TestEmptyConditionParser()
    {
        var result = ExpressionParser.EmptyConditionParser.TryParse("Test()");
        Assert.That(result.WasSuccessful, Is.True);
        Assert.That(result.Value, Is.EqualTo(new ParsedCondition()
        {
            Name = "Test",
            Arguments = new List<string>(),
            Operators = new List<string>(),
            Conditions = new List<ParsedOperationWithCondition>(),
        }));
    }

    [Test]
    public void TestEmptyConditionParserWhitespace()
    {
        var result = ExpressionParser.EmptyConditionParser.TryParse("Test  (  )");
        Assert.That(result.WasSuccessful, Is.True);
        Assert.That(result.Value, Is.EqualTo(new ParsedCondition()
        {
            Name = "Test",
            Arguments = new List<string>(),
            Operators = new List<string>(),
            Conditions = new List<ParsedOperationWithCondition>(),
        }));
    }

    [Test]
    public void TestEmptyConditionParserMissingStartParenthesis()
    {
        var result = ExpressionParser.EmptyConditionParser.TryParse("Test)");
        Assert.That(result.WasSuccessful, Is.False);
    }

    [Test]
    public void TestEmptyConditionParserMissingEndParenthesis()
    {
        var result = ExpressionParser.EmptyConditionParser.TryParse("Test(");
        Assert.That(result.WasSuccessful, Is.False);
    }

    [Test]
    public void TestArgumentConditionParser()
    {
        var result = ExpressionParser.ArgumentConditionParser.TryParse("Test(test1)");
        Assert.That(result.WasSuccessful, Is.True);
        Assert.That(result.Value.Name, Is.EqualTo("Test"));
        Assert.That(result.Value.Arguments, Is.EqualTo(new List<string>() { "test1" }));
        Assert.That(result.Value.Operators, Is.EqualTo(new List<string>()));
        Assert.That(result.Value.Conditions, Is.EqualTo(new List<ParsedOperationWithCondition>()));
    }

    [Test]
    public void TestArgumentConditionParserMultipleArgumentsAndWhitespace()
    {
        var result = ExpressionParser.ArgumentConditionParser.TryParse("Test  (  test1  , \"test2\",test3  )");
        Assert.That(result.WasSuccessful, Is.True);
        Assert.That(result.Value.Name, Is.EqualTo("Test"));
        Assert.That(result.Value.Arguments, Is.EqualTo(new List<string>() { "test1", "test2", "test3" }));
        Assert.That(result.Value.Operators, Is.EqualTo(new List<string>()));
        Assert.That(result.Value.Conditions, Is.EqualTo(new List<ParsedOperationWithCondition>()));
    }

    [Test]
    public void TestConditionParserNoArguments()
    {
        var result = ExpressionParser.ConditionParser.TryParse("Test()");
        Assert.That(result.WasSuccessful, Is.True);
        Assert.That(result.Value.Name, Is.EqualTo("Test"));
        Assert.That(result.Value.Arguments, Is.EqualTo(new List<string>()));
        Assert.That(result.Value.Operators, Is.EqualTo(new List<string>()));
        Assert.That(result.Value.Conditions, Is.EqualTo(new List<ParsedOperationWithCondition>()));
    }

    [Test]
    public void TestConditionParserArguments()
    {
        var result = ExpressionParser.ConditionParser.TryParse("Test(test1)");
        Assert.That(result.WasSuccessful, Is.True);
        Assert.That(result.Value.Name, Is.EqualTo("Test"));
        Assert.That(result.Value.Arguments, Is.EqualTo(new List<string>() { "test1" }));
        Assert.That(result.Value.Operators, Is.EqualTo(new List<string>()));
        Assert.That(result.Value.Conditions, Is.EqualTo(new List<ParsedOperationWithCondition>()));
    }

    [Test]
    public void TestUnaryOperatorParser()
    {
        var result = ExpressionParser.UnaryOperatorParser.TryParse("not");
        Assert.That(result.WasSuccessful, Is.True);
        Assert.That(result.Value, Is.EqualTo("not"));
    }

    [Test]
    public void TestUnaryOperatorParserSpacing()
    {
        var result = ExpressionParser.UnaryOperatorParser.TryParse("  NOT  ");
        Assert.That(result.WasSuccessful, Is.True);
        Assert.That(result.Value, Is.EqualTo("NOT"));
    }

    [Test]
    public void TestBinaryOperatorParser()
    {
        var result = ExpressionParser.BinaryOperatorParser.TryParse("and");
        Assert.That(result.WasSuccessful, Is.True);
        Assert.That(result.Value, Is.EqualTo("and"));
    }

    [Test]
    public void TestBinaryOperatorParserSpacing()
    {
        var result = ExpressionParser.BinaryOperatorParser.TryParse("  OR  ");
        Assert.That(result.WasSuccessful, Is.True);
        Assert.That(result.Value, Is.EqualTo("OR"));
    }

    [Test]
    public void TestUngroupedExpressionParserSingleCondition()
    {
        var result = ExpressionParser.UngroupedExpressionParser.TryParse("Test(test1)");
        Assert.That(result.WasSuccessful, Is.True);
        Assert.That(result.Value.Name, Is.EqualTo("Test"));
        Assert.That(result.Value.Arguments, Is.EqualTo(new List<string>() { "test1" }));
        Assert.That(result.Value.Operators, Is.EqualTo(new List<string>()));
        Assert.That(result.Value.Conditions, Is.EqualTo(new List<ParsedOperationWithCondition>()));
    }

    [Test]
    public void TestUngroupedExpressionParserSimpleBinaryOperation()
    {
        var result = ExpressionParser.UngroupedExpressionParser.TryParse("Test1(test1) and Test2()");
        Assert.That(result.WasSuccessful, Is.True);
        Assert.That(result.Value, Is.EqualTo(new ParsedCondition()
        {
            Name = "Test1",
            Arguments = new List<string>() { "test1" },
            Operators = new List<string>(),
            Conditions = new List<ParsedOperationWithCondition>() { new ParsedOperationWithCondition()
            {
                Operator = "and",
                Condition = new ParsedCondition()
                {
                    Name = "Test2",
                    Arguments = new List<string>(),
                    Operators = new List<string>(),
                    Conditions = new List<ParsedOperationWithCondition>(),
                },
            }},
        }));
    }

    [Test]
    public void TestUngroupedExpressionParserComplexBinaryOperation()
    {
        var result = ExpressionParser.UngroupedExpressionParser.TryParse("Test1(test1) and not (not Test2() and Test3()) or Test4()");
        Assert.That(result.WasSuccessful, Is.True);
        Assert.That(result.Value, Is.EqualTo(new ParsedCondition()
        {
            Name = "Test1",
            Arguments = new List<string>() { "test1" },
            Operators = new List<string>(),
            Conditions = new List<ParsedOperationWithCondition>() { new ParsedOperationWithCondition()
            {
                Operator = "and",
                Condition = new ParsedCondition()
                {
                    Name = "ConditionGroup",
                    Arguments = new List<string>(),
                    Operators = new List<string>() { "not" },
                    Conditions = new List<ParsedOperationWithCondition>()
                    {
                      new ParsedOperationWithCondition()
                      {
                          Operator = "and",
                          Condition = new ParsedCondition()
                          {
                              Name = "Test2",
                              Arguments = new List<string>(),
                              Operators = new List<string>() { "not" },
                              Conditions = new List<ParsedOperationWithCondition>()
                              {
                                  new ParsedOperationWithCondition()
                                  {
                                      Operator = "and",
                                      Condition = new ParsedCondition()
                                      {
                                          Name = "Test3",
                                          Arguments = new List<string>(),
                                          Operators = new List<string>(),
                                          Conditions = new List<ParsedOperationWithCondition>()
                                      },
                                  },
                              },
                          },
                      },
                    },
                },
            }, new ParsedOperationWithCondition()
            {
                Operator = "or",
                Condition = new ParsedCondition()
                {
                    Name = "Test4",
                    Arguments = new List<string>(),
                    Operators = new List<string>(),
                    Conditions = new List<ParsedOperationWithCondition>(),
                },
            }},
        }));
    }

    [Test]
    public void TestGroupedExpressionParser()
    {
        var result = ExpressionParser.GroupedExpressionParser.TryParse("(Test1(test1) and Test2())");
        Assert.That(result.WasSuccessful, Is.True);
        Assert.That(result.Value, Is.EqualTo(new ParsedCondition()
        {
            Name = "ConditionGroup",
            Arguments = new List<string>(),
            Operators = new List<string>(),
            Conditions = new List<ParsedOperationWithCondition>()
            {
              new ParsedOperationWithCondition()
              {
                  Operator = "and",
                  Condition = new ParsedCondition()
                  {
                      Name = "Test1",
                      Arguments = new List<string>() { "test1" },
                      Operators = new List<string>(),
                      Conditions = new List<ParsedOperationWithCondition>()
                      {
                          new ParsedOperationWithCondition()
                          {
                              Operator = "and",
                              Condition = new ParsedCondition()
                              {
                                  Name = "Test2",
                                  Arguments = new List<string>(),
                                  Operators = new List<string>(),
                                  Conditions = new List<ParsedOperationWithCondition>()
                              },
                          },
                      },
                  },
              },
            },
        }));
    }

    [Test]
    public void TestGroupedExpressionParserUngrouped()
    {
        var result = ExpressionParser.GroupedExpressionParser.TryParse("Test1(test1) and Test2()");
        Assert.That(result.WasSuccessful, Is.True);
        Assert.That(result.Value, Is.EqualTo(new ParsedCondition()
        {
            Name = "Test1",
            Arguments = new List<string>() { "test1" },
            Operators = new List<string>(),
            Conditions = new List<ParsedOperationWithCondition>()
            {
                new ParsedOperationWithCondition()
                {
                    Operator = "and",
                    Condition = new ParsedCondition()
                    {
                        Name = "Test2",
                        Arguments = new List<string>(),
                        Operators = new List<string>(),
                        Conditions = new List<ParsedOperationWithCondition>()
                    },
                },
            },
        }));
    }

    [Test]
    public void TestGroupedExpressionParserIncompleteGroup()
    {
        var result = ExpressionParser.GroupedExpressionParser.TryParse("(Test1(test1) and Test2()");
        Assert.That(result.WasSuccessful, Is.False);
    }

    [Test]
    public void TestOperatedGroupedExpressionParser()
    {
        var result = ExpressionParser.OperatedGroupedExpressionParser.TryParse("not (Test1(test1) and not not Test2())");
        Assert.That(result.WasSuccessful, Is.True);
        Assert.That(result.Value, Is.EqualTo(new ParsedCondition()
        {
            Name = "ConditionGroup",
            Arguments = new List<string>(),
            Operators = new List<string>() { "not" },
            Conditions = new List<ParsedOperationWithCondition>()
            {
                new ParsedOperationWithCondition()
                {
                    Operator = "and",
                    Condition = new ParsedCondition()
                    {
                        Name = "Test1",
                        Arguments = new List<string>() { "test1" },
                        Operators = new List<string>(),
                        Conditions = new List<ParsedOperationWithCondition>()
                        {
                            new ParsedOperationWithCondition()
                            {
                                Operator = "and",
                                Condition = new ParsedCondition()
                                {
                                    Name = "Test2",
                                    Arguments = new List<string>(),
                                    Operators = new List<string>() { "not", "not" },
                                    Conditions = new List<ParsedOperationWithCondition>()
                                },
                            },
                        },
                    },
                },
            },
        }));
    }
    
    [Test]
    public void TestFullExpressionParser()
    {
        var result = ExpressionParser.FullExpressionParser.TryParse("     not (Test1(test1) and not not Test2() and Test3() and (Test4() or Test5()))     ");
        Assert.That(result.WasSuccessful, Is.True);
        Assert.That(result.Value, Is.EqualTo(new ParsedCondition()
        {
            Name = "ConditionGroup",
            Arguments = new List<string>(),
            Operators = new List<string>() { "not" },
            Conditions = new List<ParsedOperationWithCondition>()
            {
                new ParsedOperationWithCondition()
                {
                    Operator = "and",
                    Condition = new ParsedCondition()
                    {
                        Name = "Test1",
                        Arguments = new List<string>() { "test1" },
                        Operators = new List<string>(),
                        Conditions = new List<ParsedOperationWithCondition>(),
                    },
                },
                new ParsedOperationWithCondition()
                {
                    Operator = "and",
                    Condition = new ParsedCondition()
                    {
                        Name = "Test2",
                        Arguments = new List<string>(),
                        Operators = new List<string>() { "not", "not" },
                        Conditions = new List<ParsedOperationWithCondition>(),
                    },
                },
                new ParsedOperationWithCondition()
                {
                    Operator = "and",
                    Condition = new ParsedCondition()
                    {
                        Name = "Test3",
                        Arguments = new List<string>(),
                        Operators = new List<string>(),
                        Conditions = new List<ParsedOperationWithCondition>()
                    },
                },
                new ParsedOperationWithCondition()
                {
                    Operator = "and",
                    Condition = new ParsedCondition()
                    {
                        Name = "ConditionGroup",
                        Arguments = new List<string>(),
                        Operators = new List<string>(),
                        Conditions = new List<ParsedOperationWithCondition>()
                        {
                            new ParsedOperationWithCondition()
                            {
                                Operator = "and",
                                Condition = new ParsedCondition()
                                {
                                    Name = "Test4",
                                    Arguments = new List<string>(),
                                    Operators = new List<string>(),
                                    Conditions = new List<ParsedOperationWithCondition>(),
                                },
                            },
                            new ParsedOperationWithCondition()
                            {
                                Operator = "or",
                                Condition = new ParsedCondition()
                                {
                                    Name = "Test5",
                                    Arguments = new List<string>(),
                                    Operators = new List<string>(),
                                    Conditions = new List<ParsedOperationWithCondition>()
                                },
                            },
                        }
                    },
                },
            },
        }));
    }
    
    [Test]
    public void TestFullExpressionParserExtraCharacters()
    {
        var result = ExpressionParser.FullExpressionParser.TryParse("Test1(test1))");
        Assert.That(result.WasSuccessful, Is.False);
    }
    
    [Test]
    public void TestFullExpressionParserNoCondition()
    {
        var result = ExpressionParser.FullExpressionParser.TryParse("");
        Assert.That(result.WasSuccessful, Is.False);
    }
    
    [Test]
    public void TestFullExpressionParserMissingOperator()
    {
        var result = ExpressionParser.FullExpressionParser.TryParse("Test1(test1) Test2()");
        Assert.That(result.WasSuccessful, Is.False);
    }

    [Test]
    public void TestFullExpressionParserDuplicateOperator()
    {
        var result = ExpressionParser.FullExpressionParser.TryParse("Test1(test1) and or Test2()");
        Assert.That(result.WasSuccessful, Is.False);
    }
}