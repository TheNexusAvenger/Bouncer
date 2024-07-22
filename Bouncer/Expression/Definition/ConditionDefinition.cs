using System;
using System.Collections.Generic;

namespace Bouncer.Expression.Definition;

public class ConditionDefinition
{
    /// <summary>
    /// Name of the condition definition.
    /// </summary>
    public string Name { get; set; } = null!;
    
    /// <summary>
    /// Formatter for showing human-readable displays.
    /// </summary>
    public string? FormatString { get; set; }

    /// <summary>
    /// Number of arguments accepted for the condition.
    /// </summary>
    public int TotalArguments { get; set; }

    /// <summary>
    /// Evaluates the condition with the Roblox user id and a list of string arguments.
    /// </summary>
    public Func<long, List<string>, bool> Evaluate { get; set; } = null!;
}