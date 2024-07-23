namespace Bouncer.Diagnostic.Model;

public class VerifyRulesResult
{
    /// <summary>
    /// Total number of rule configuration errors found.
    /// </summary>
    public int TotalRuleConfigurationErrors { get; set; } = 0;
    
    /// <summary>
    /// Total number of parse errors found with the rules.
    /// </summary>
    public int TotalParseErrors { get; set; } = 0;
    
    /// <summary>
    /// Total number of transform errors (valid grammar but missing/incorrect conditions/operators) found with the rules.
    /// </summary>
    public int TotalTransformErrors { get; set; } = 0;
}