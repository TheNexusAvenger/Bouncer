using System;
using System.Collections.Generic;
using System.Linq;

namespace Bouncer.Parser.Model;

public class ParsedCondition
{
    /// <summary>
    /// Name of the condition.
    /// </summary>
    public string Name { get; set; } = null!;
    
    /// <summary>
    /// Arguments for the condition.
    /// </summary>
    public List<string> Arguments { get; set; } = new List<string>();
    
    /// <summary>
    /// List of unary operators applied to the condition.
    /// The outer-most operators are added last to the list, and should be applied last.
    /// </summary>
    public List<string> Operators { get; set; } = new List<string>();
    
    /// <summary>
    /// Additional conditions to apply with their binary operator.
    /// Conditions should be applied in order of the table.
    /// </summary>
    public List<ParsedOperationWithCondition> Conditions { get; set; } = new List<ParsedOperationWithCondition>();
    
    /// <summary>
    /// Adds a unary operator.
    /// </summary>
    /// <param name="operation">Unary operator to add.</param>
    /// <returns>The original parsed condition to be used in LINQ expressions.</returns>
    public ParsedCondition AddOperator(string operation)
    {
        this.Operators.Add(operation);
        return this;
    }

    /// <summary>
    /// Adds a list of argument.
    /// </summary>
    /// <param name="arguments">Arguments to add.</param>
    /// <returns>The original parsed condition to be used in LINQ expressions.</returns>
    public ParsedCondition AddArguments(IEnumerable<string> arguments)
    {
        this.Arguments.AddRange(arguments);
        return this;
    }


    /// <summary>
    /// Adds a list of additional conditions with their binary operators.
    /// </summary>
    /// <param name="conditions">Conditions with operators to add.</param>
    /// <returns>The original parsed condition to be used in LINQ expressions.</returns>
    public ParsedCondition AddConditions(IEnumerable<ParsedOperationWithCondition> conditions)
    {
        this.Conditions.AddRange(conditions);
        return this;
    }

    /// <summary>
    /// Flattens the non-grouped conditions.
    /// </summary>
    /// <returns>The original parsed condition to be used in LINQ expressions.</returns>
    public ParsedCondition FlattenConditions()
    {
        // Flatten the child conditions.
        foreach (var condition in this.Conditions)
        {
            condition.Condition.FlattenConditions();
        }
        
        // Pull up the non-grouped conditions.
        var newConditions = new List<ParsedOperationWithCondition>();
        foreach (var condition in this.Conditions)
        {
            newConditions.Add(condition);
            if (condition.Condition.Name != "ConditionGroup")
            {
                newConditions.AddRange(condition.Condition.Conditions);
                condition.Condition.Conditions.Clear();
            }
            condition.Condition.FlattenConditions();
        }
        this.Conditions = newConditions;
        return this;
    }
    
    /// <summary>
    /// Returns if another parsed condition is equal.
    /// </summary>
    /// <param name="other">Other parsed condition to compare.</param>
    /// <returns>Whether the parsed condition is equal.</returns>
    private bool Equals(ParsedCondition other)
    {
        return this.Name == other.Name
               && this.Arguments.SequenceEqual(other.Arguments)
               && this.Operators.SequenceEqual(other.Operators)
               && this.Conditions.SequenceEqual(other.Conditions);
    }

    /// <summary>
    /// Returns if another object is equal.
    /// </summary>
    /// <param name="obj">Other object to compare.</param>
    /// <returns>Whether the object is equal.</returns>
    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != this.GetType()) return false;
        return this.Equals((ParsedCondition) obj);
    }

    /// <summary>
    /// Returns the hash code for the parsed condition.
    /// </summary>
    /// <returns>The hash code for the parsed condition.</returns>
    public override int GetHashCode()
    {
        return HashCode.Combine(this.Name, this.Arguments, this.Operators, this.Conditions);
    }
}