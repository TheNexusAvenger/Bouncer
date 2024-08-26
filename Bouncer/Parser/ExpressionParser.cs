using System.Collections.Generic;
using System.Linq;
using Bouncer.Expression;
using Bouncer.Parser.Model;
using Sprache;

namespace Bouncer.Parser;

public static class ExpressionParser
{
    #region Tokens
    /// <summary>
    /// Token for separating arguments when not in quotes.
    /// </summary>
    public static readonly Parser<char> ArgumentSeparatorToken = Parse.Char(',');
    
    /// <summary>
    /// Token for start and eend strings that can contain commas and quotes.
    /// </summary>
    public static readonly Parser<char> StringArgumentDelimiterToken = Parse.Char('"');
    
    /// <summary>
    /// Token for the start of arguments passed into a condition.
    /// </summary>
    public static readonly Parser<char> ConditionArgumentStartToken = Parse.Char('(');
    
    /// <summary>
    /// Token for the end of arguments passed into a condition.
    /// </summary>
    public static readonly Parser<char> ConditionArgumentEndToken = Parse.Char(')');
    
    /// <summary>
    /// Token for the start of a condition group.
    /// </summary>
    public static readonly Parser<char> ExpressionGroupStartToken = Parse.Char('(');
    
    /// <summary>
    /// Token for the end of a condition group.
    /// </summary>
    public static readonly Parser<char> ExpressionGroupEndToken = Parse.Char(')');
    #endregion

    #region Single Character Parsers
    /// <summary>
    /// Parser for escaping any character starting with \.
    /// This is implemented manually due to issues with non-quote escapes not working.
    /// </summary>
    public static readonly Parser<char> EscapedArgumentCharacterParser = (input) =>
    {
        // Check if the first character is \ and 2 characters can be read.
        if (!input.AtEnd)
        {
            if (input.Current == '\\')
            {
                // Advance the input twice and return the escaped character.
                input = input.Advance();
                if (!input.AtEnd)
                {
                    return Result.Success<char>(input.Current, input.Advance());
                }
            }
            else
            {
                // Return that the character is not \\.
                return Result.Failure<char>(input, "Character is not \\.", new string[1]
                {
                    "The character \\ followed by any character."
                });
            }
        }
    
        // Return that the end of string was reached.
        return Result.Failure<char>(input, "Unexpected end of input reached.", new string[1]
        {
            "The character \\ followed by any character."
        });
    };
    
    /// <summary>
    /// Parser for a character in an argument that is unquoted (ex: numbers).
    /// </summary>
    public static readonly Parser<char> UnquotedArgumentCharacterParser = Parse.AnyChar.Except(ArgumentSeparatorToken).Except(ConditionArgumentEndToken);
    
    /// <summary>
    /// Parser for a character in an argument that is quoted (ex: strings).
    /// </summary>
    public static readonly Parser<char> QuotedArgumentCharacterParser = EscapedArgumentCharacterParser.Or(Parse.AnyChar.Except(StringArgumentDelimiterToken));
    #endregion
    
    #region Argument String Parsers
    /// <summary>
    /// Parser for an unquoted argument (ex: numbers) for a condition.
    /// </summary>
    public static readonly Parser<string> UnquotedArgumentParser = from argument in UnquotedArgumentCharacterParser.Many().Text()
        select argument.Trim();
    
    /// <summary>
    /// Parser for a quoted argument (ex: strings) for a condition.
    /// </summary>
    public static readonly Parser<string> QuotedArgumentParser = from argumentStart in ExpressionParser.StringArgumentDelimiterToken
        from argumentText in ExpressionParser.QuotedArgumentCharacterParser.Many().Text()
        from argumentEnd in ExpressionParser.StringArgumentDelimiterToken
        select argumentText;
    
    /// <summary>
    /// Parser for a quoted or unquoted argument for a condition.
    /// </summary>
    public static readonly Parser<string> ArgumentParser = from leadingWhitespace in Parse.WhiteSpace.Many()
        from argumentText in ExpressionParser.QuotedArgumentParser.XOr(UnquotedArgumentParser)
        from trailingWhitespace in Parse.WhiteSpace.Many()
        select argumentText;
    #endregion
    
    #region Condition Parsers
    /// <summary>
    /// Parser for a condition with now arguments, in the format of Condition().
    /// This special-case prevents empty parameters from being passed.
    /// </summary>
    public static readonly Parser<ParsedCondition> EmptyConditionParser = from conditionName in Parse.LetterOrDigit.AtLeastOnce().Text()
        from leadingWhitespace in Parse.WhiteSpace.Many()
        from argumentStart in ExpressionParser.ConditionArgumentStartToken
        from whitespace in Parse.WhiteSpace.Many()
        from conditionEnd in ExpressionParser.ConditionArgumentEndToken
        select new ParsedCondition()
        {
            Name = conditionName,
        };
    
    /// <summary>
    /// Parser for a condition with arguments, in the format of Condition(Argument1, Argument2, ...).
    /// </summary>
    public static readonly Parser<ParsedCondition> ArgumentConditionParser = from conditionName in Parse.LetterOrDigit.AtLeastOnce().Text()
        from leadingWhitespace in Parse.WhiteSpace.Many()
        from argumentStart in ExpressionParser.ConditionArgumentStartToken
        from leadingArgument in ExpressionParser.ArgumentParser
        from remainingArguments in ExpressionParser.ArgumentSeparatorToken.Then(_ => ExpressionParser.ArgumentParser).Many()
        from conditionEnd in ExpressionParser.ConditionArgumentEndToken
        select new ParsedCondition()
        {
            Name = conditionName,
            Arguments = [leadingArgument],
        }.AddArguments(remainingArguments);

    /// <summary>
    /// Parser for a condition with or without arguments.
    /// </summary>
    public static readonly Parser<ParsedCondition> ConditionParser = EmptyConditionParser.Or(ArgumentConditionParser);
    #endregion
    
    #region Operator Parsers
    /// <summary>
    /// Parser for a unary operator word.
    /// Condition.UnaryOperations is allowed to change at runtime.
    /// </summary>
    public static readonly Parser<string> UnaryOperatorWordParser = (input) =>
    {
        return Condition.UnaryOperations.Select(keyword => Parse.IgnoreCase(keyword).Text())
            .Aggregate((current, next) => current.Or(next)).Invoke(input);
    };
    
    /// <summary>
    /// Parser for a unary operator (ex: not).
    /// </summary>
    public static readonly Parser<string> UnaryOperatorParser = from leadingWhitespace in Parse.WhiteSpace.Many()
        from operationText in UnaryOperatorWordParser.Text()
        from trailingWhitespace in Parse.WhiteSpace.Many()
        select operationText;

    /// <summary>
    /// Parser for a binary operator word.
    /// Condition.BinaryOperations is allowed to change at runtime.
    /// </summary>
    public static readonly Parser<string> BinaryOperatorWordParser = (input) =>
    {
        return Condition.BinaryOperations.Select(keyword => Parse.IgnoreCase(keyword).Text())
            .Aggregate((current, next) => current.Or(next)).Invoke(input);
    };
    
    /// <summary>
    /// Parser for a binary operator (ex: and, or, nand, nor, xor).
    /// </summary>
    public static readonly Parser<string> BinaryOperatorParser = from leadingWhitespace in Parse.WhiteSpace.Many()
        from operationText in BinaryOperatorWordParser
        from trailingWhitespace in Parse.WhiteSpace.Many()
        select operationText;
    #endregion
    
    #region Expression Parsers
    /// <summary>
    /// Parser for an expression with binary a binary operators.
    /// This will try to add all conditions afterward, and includes grouped expressions.
    /// TODO: Operators are currently allowed to be followed by a condition with no separator.
    /// </summary>
    public static readonly Parser<ParsedCondition> UngroupedExpressionParser = from leadingCondition in ExpressionParser.ConditionParser
        from trailingConditions in ExpressionParser.BinaryOperatorParser.Then(conditionOperator => from nextCondition in OperatedGroupedExpressionParser
            select new ParsedOperationWithCondition()
            {
                Condition = nextCondition,
                Operator = conditionOperator,
            }).Many()
        select leadingCondition.AddConditions(trailingConditions);
    
    /// <summary>
    /// Parser for an expression in a group.
    /// Groups can contain 1 or multiple expressions, including additional groups.
    /// </summary>
    public static readonly Parser<ParsedCondition> GroupedExpressionParser =
        (from leftGroup in ExpressionParser.ExpressionGroupStartToken
            from innerCondition in Parse.Ref(() => OperatedGroupedExpressionParser)
            from rightGroup in ExpressionParser.ExpressionGroupEndToken
            select new ParsedCondition()
            {
              Name = "ConditionGroup",
              Conditions = new List<ParsedOperationWithCondition>()
              {
                  new ParsedOperationWithCondition()
                  {
                      Operator = "and",
                      Condition = innerCondition,
                  },
              },
            })
        .XOr(ExpressionParser.UngroupedExpressionParser);
    
    /// <summary>
    /// Parser for a grouped expression or expression with a unary operation on the front (ex: not).
    /// TODO: Operators are currently allowed to be followed by a condition with no separator.
    /// </summary>
    public static readonly Parser<ParsedCondition> OperatedGroupedExpressionParser = (from invertOperator in ExpressionParser.UnaryOperatorParser
        from conditionExpression in Parse.Ref(() => OperatedGroupedExpressionParser)
        select conditionExpression.AddOperator(invertOperator)).Or(ExpressionParser.GroupedExpressionParser);
    #endregion
    
    #region Input Parsers
    /// <summary>
    /// Parser for an expression with no extra trailing characters.
    /// This is intended for parsing user input from configurations.
    /// </summary>
    public static readonly Parser<ParsedCondition> FullExpressionParser = from expressionCondition in OperatedGroupedExpressionParser
        from end in Parse.WhiteSpace.Many().End()
        select expressionCondition.FlattenConditions();
    #endregion
}