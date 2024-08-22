using System.Text.Json.Serialization;

namespace Bouncer.Web.Server.Model;

public enum HealthCheckResultStatus
{
    Up,
    Down,
}

public class HealthCheckConfigurationProblems
{
    /// <summary>
    /// Status of the health check for the configuration.
    /// </summary>
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public HealthCheckResultStatus Status { get; set; } = HealthCheckResultStatus.Up;
    
    /// <summary>
    /// Total number of rule configuration errors found.
    /// </summary>
    public int TotalRuleConfigurationErrors { get; set; } = 0;
    
    /// <summary>
    /// Total number of parse errors found with the rules.
    /// </summary>
    public int TotalRuleParseErrors { get; set; } = 0;
    
    /// <summary>
    /// Total number of transform errors (valid grammar but missing/incorrect conditions/operators) found with the rules.
    /// </summary>
    public int TotalRuleTransformErrors { get; set; } = 0;
}

public class HealthCheckResult
{
    /// <summary>
    /// Status of the combined health check.
    /// </summary>
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public HealthCheckResultStatus Status { get; set; } = HealthCheckResultStatus.Up;

    /// <summary>
    /// Summary of the health check for the configuration.
    /// </summary>
    public HealthCheckConfigurationProblems Configuration { get; set; } = new HealthCheckConfigurationProblems();
}

[JsonSerializable(typeof(HealthCheckResult))]
[JsonSerializable(typeof(HealthCheckConfigurationProblems))]
[JsonSourceGenerationOptions(WriteIndented=true, IncludeFields = true)]
internal partial class HealthCheckResultJsonContext : JsonSerializerContext
{
}