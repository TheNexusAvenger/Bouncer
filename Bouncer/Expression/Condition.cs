using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Bouncer.Expression.Definition;
using Bouncer.Parser;
using Bouncer.Parser.Model;
using Sprache;

namespace Bouncer.Expression;

public class Condition
{
    /// <summary>
    /// List of condition definitions.
    /// </summary>
    private static readonly List<ConditionDefinition> ConditionDefinitions = new List<ConditionDefinition>()
    {
        new ConditionDefinition()
        {
            Name = "ConditionGroup",
            TotalArguments = 0,
            Evaluate = (_, _) => true,
        },
    };

    /// <summary>
    /// List of unary operation definitions.
    /// </summary>
    private static readonly List<UnaryOperationDefinition> UnaryOperationDefinitions = new List<UnaryOperationDefinition>()
    {
        new UnaryOperationDefinition()
        {
            Name = "not",
            Evaluate = (value) => !value,
        },
    };

    /// <summary>
    /// List of binary operation definitions.
    /// </summary>
    private static readonly List<BinaryOperationDefinition> BinaryOperationDefinitions = new List<BinaryOperationDefinition>()
    {
        new BinaryOperationDefinition()
        {
            Name = "or",
            Evaluate = (value1, condition2) => value1 || condition2(),
        },
        new BinaryOperationDefinition()
        {
            Name = "and",
            Evaluate = (value1, condition2) => value1 && condition2(),
        },
    };

    /// <summary>
    /// Semaphore for adding new definitions.
    /// </summary>
    private static readonly SemaphoreSlim AddDefinitionSemaphore = new SemaphoreSlim(1);

    /// <summary>
    /// Definition of the condition.
    /// </summary>
    public readonly ConditionDefinition ConditionDefinition;

    /// <summary>
    /// Additional arguments to pass to the condition.
    /// </summary>
    private readonly List<string> _conditionArguments;

    /// <summary>
    /// Unary operations to apply.
    /// </summary>
    private readonly List<UnaryOperationDefinition> _unaryOperations;

    /// <summary>
    /// Binary operations to apply.
    /// </summary>
    private readonly List<OperationWithCondition> _binaryOperations;

    /// <summary>
    /// Creates a condition.
    /// </summary>
    /// <param name="conditionDefinition">Definition of the condition.</param>
    /// <param name="conditionArguments">Additional arguments to pass to the condition.</param>
    /// <param name="unaryOperations">Unary operations to apply to the condition.</param>
    /// <param name="binaryOperations">Binary operations to apply to the condition.</param>
    public Condition(ConditionDefinition conditionDefinition, List<string> conditionArguments, List<UnaryOperationDefinition> unaryOperations, List<OperationWithCondition> binaryOperations)
    {
        this.ConditionDefinition = conditionDefinition;
        this._conditionArguments = conditionArguments;
        this._unaryOperations = unaryOperations;
        this._binaryOperations = binaryOperations;
    }

    /// <summary>
    /// Adds a condition definition.
    /// </summary>
    /// <param name="conditionDefinition">Condition definition to add.</param>
    public static void AddConditionDefinition(ConditionDefinition conditionDefinition)
    {
        AddDefinitionSemaphore.Wait();
        var existingDefinition = ConditionDefinitions.Where(condition =>
                conditionDefinition.Name.Equals(condition.Name, StringComparison.InvariantCultureIgnoreCase))
            .FirstOrDefault(condition => condition.TotalArguments == conditionDefinition.TotalArguments);
        if (existingDefinition != null)
        {
            AddDefinitionSemaphore.Release();
            throw new InvalidOperationException($"There is an existing condition named \"{conditionDefinition.Name}\" with {conditionDefinition.TotalArguments} arguments.");
        }
        ConditionDefinitions.Add(conditionDefinition);
        AddDefinitionSemaphore.Release();
    }

    /// <summary>
    /// Parses a condition.
    /// </summary>
    /// <param name="parsedCondition">Parsed condition to convert.</param>
    /// <returns>Condition to run with user ids.</returns>
    public static Condition FromParsedCondition(ParsedCondition parsedCondition)
    {
        // Determine the definition.
        var nameMatchingConditions = ConditionDefinitions.Where(condition =>
            parsedCondition.Name.Equals(condition.Name, StringComparison.InvariantCultureIgnoreCase)).ToList();
        if (nameMatchingConditions.Count == 0)
        {
            throw new InvalidOperationException($"There are no conditions named \"{parsedCondition.Name}\".");
        }
        var conditionDefinition =
            nameMatchingConditions.FirstOrDefault(condition => condition.TotalArguments == parsedCondition.Arguments.Count);
        if (conditionDefinition == null)
        {
            if (nameMatchingConditions.Count == 1)
            {
                throw new InvalidOperationException($"The condition \"{parsedCondition.Name}\" expects {nameMatchingConditions.First().TotalArguments} arguments but {parsedCondition.Arguments.Count} were given.");
            }
            else
            {
                var argumentCountOptions = nameMatchingConditions.Select(definition => definition.TotalArguments);
                throw new InvalidOperationException($"The condition \"{parsedCondition.Name}\" expects any of [{string.Join(", ", argumentCountOptions)}] arguments but {parsedCondition.Arguments.Count} were given.");
            }
        }
        
        // Determine the unary operators.
        var unaryOperations = new List<UnaryOperationDefinition>();
        foreach (var operationName in parsedCondition.Operators)
        {
            var operation = UnaryOperationDefinitions.FirstOrDefault(operation => operationName.Equals(operation.Name, StringComparison.InvariantCultureIgnoreCase));
            if (operation == null)
            {
                throw new InvalidOperationException($"There are no operations named \"{operationName}\".");
            }
            unaryOperations.Add(operation);
        }
        
        // Add the binary operations.
        var binaryOperations = new List<OperationWithCondition>();
        foreach (var subCondition in parsedCondition.Conditions)
        {
            var operation = BinaryOperationDefinitions.FirstOrDefault(operation => subCondition.Operator.Equals(operation.Name, StringComparison.InvariantCultureIgnoreCase));
            if (operation == null)
            {
                throw new InvalidOperationException($"There are no operations named \"{subCondition.Operator}\".");
            }
            binaryOperations.Add(new OperationWithCondition()
            {
                OperationDefinition = operation,
                Condition = Condition.FromParsedCondition(subCondition.Condition),
            });
        }
        
        // Return the condition.
        return new Condition(conditionDefinition, parsedCondition.Arguments, unaryOperations, binaryOperations);
    }
    
    /// <summary>
    /// Parses a condition.
    /// </summary>
    /// <param name="conditionInput">String input of the condition.</param>
    /// <returns>Condition to run with user ids.</returns>
    public static Condition FromParsedCondition(string conditionInput)
    {
        return FromParsedCondition(ExpressionParser.FullExpressionParser.Parse(conditionInput));
    }

    /// <summary>
    /// Evaluates the condition
    /// </summary>
    /// <param name="userId">Roblox user id to evaluate with.</param>
    /// <returns>Whether the condition passed or not.</returns>
    public bool Evaluate(long userId)
    {
        // Calculate the result of the condition.
        var conditionResult = this.ConditionDefinition.Evaluate(userId, this._conditionArguments);
        
        // Apply the binary operations.
        foreach (var operation in this._binaryOperations)
        {
            conditionResult = operation.OperationDefinition.Evaluate(conditionResult, () => operation.Condition.Evaluate(userId));
        }
        
        // Apply the unary operations.
        foreach (var operation in this._unaryOperations)
        {
            conditionResult = operation.Evaluate(conditionResult);
        }

        // return the result.
        return conditionResult;
    }
}