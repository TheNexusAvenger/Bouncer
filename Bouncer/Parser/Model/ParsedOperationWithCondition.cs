using System;

namespace Bouncer.Parser.Model;

public class ParsedOperationWithCondition
{
    /// <summary>
    /// Operator to apply with the condition.
    /// </summary>
    public string Operator { get; set; } = null!;

    /// <summary>
    /// Condition to apply with the parent condition.
    /// </summary>
    public ParsedCondition Condition { get; set; } = null!;
    
    /// <summary>
    /// Returns if another parsed operation with condition is equal.
    /// </summary>
    /// <param name="other">Other parsed operation with condition to compare.</param>
    /// <returns>Whether the parsed operation with condition is equal.</returns>
    private bool Equals(ParsedOperationWithCondition other)
    {
        return this.Operator == other.Operator && this.Condition.Equals(other.Condition);
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
        return this.Equals((ParsedOperationWithCondition) obj);
    }

    /// <summary>
    /// Returns the hash code for the parsed condition.
    /// </summary>
    /// <returns>The hash code for the parsed condition.</returns>
    public override int GetHashCode()
    {
        return HashCode.Combine(this.Operator, this.Condition);
    }
}