using System;

namespace Bouncer.Expression.Definition;

public class BinaryOperationDefinition
{
    /// <summary>
    /// Name of the binary operation definition.
    /// </summary>
    public string Name { get; set; } = null!;
    
    /// <summary>
    /// Evaluates the operation given the result of a condition and an action for the second condition.
    /// The second condition is passed to avoid running when unneeded.
    /// </summary>
    public Func<bool, Func<bool>, bool> Evaluate { get; set; } = null!;
}