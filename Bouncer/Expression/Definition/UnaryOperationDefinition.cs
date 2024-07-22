using System;

namespace Bouncer.Expression.Definition;

public class UnaryOperationDefinition
{
    /// <summary>
    /// Name of the unary operation definition.
    /// </summary>
    public string Name { get; set; } = null!;
    
    /// <summary>
    /// Evaluates the operation given the result of a condition.
    /// </summary>
    public Func<bool, bool> Evaluate { get; set; } = null!;
}