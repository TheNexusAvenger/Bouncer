using Bouncer.Expression;
using Bouncer.Expression.Definition;

namespace Bouncer.Parser;

public class OperationWithCondition
{
    /// <summary>
    /// Operation to apply with the condition.
    /// </summary>
    public BinaryOperationDefinition OperationDefinition { get; set; } = null!;

    /// <summary>
    /// Condition to apply with the operation.
    /// </summary>
    public Condition Condition { get; set; } = null!;
}