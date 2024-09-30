using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bouncer.State.Loop;

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
    [JsonConverter(typeof(JsonStringEnumConverter<HealthCheckResultStatus>))]
    [JsonPropertyName("status")]
    public HealthCheckResultStatus Status { get; set; } = HealthCheckResultStatus.Up;
    
    /// <summary>
    /// Total number of rule configuration errors found.
    /// </summary>
    [JsonPropertyName("totalRuleConfigurationErrors")]
    public int TotalRuleConfigurationErrors { get; set; } = 0;
    
    /// <summary>
    /// Total number of parse errors found with the rules.
    /// </summary>
    [JsonPropertyName("totalRuleParseErrors")]
    public int TotalRuleParseErrors { get; set; } = 0;
    
    /// <summary>
    /// Total number of transform errors (valid grammar but missing/incorrect conditions/operators) found with the rules.
    /// </summary>
    [JsonPropertyName("totalRuleTransformErrors")]
    public int TotalRuleTransformErrors { get; set; } = 0;
}

public class HealthCheckGroupLoopStatus
{
    /// <summary>
    /// Status of the health check for the loop.
    /// </summary>
    [JsonConverter(typeof(JsonStringEnumConverter<HealthCheckResultStatus>))]
    [JsonPropertyName("status")]
    public HealthCheckResultStatus Status { get; set; } = HealthCheckResultStatus.Up;
    
    /// <summary>
    /// Id of the group.
    /// </summary>
    [JsonPropertyName("groupId")]
    public long GroupId { get; set; }
    
    /// <summary>
    /// Status of the last step of the group loop.
    /// </summary>
    [JsonConverter(typeof(JsonStringEnumConverter<GroupJoinRequestLoopStatus>))]
    [JsonPropertyName("lastStepStatus")]
    public GroupJoinRequestLoopStatus LastStepStatus { get; set; } = GroupJoinRequestLoopStatus.NotStarted;
}

public class HealthCheckResult
{
    /// <summary>
    /// Status of the combined health check.
    /// </summary>
    [JsonConverter(typeof(JsonStringEnumConverter<HealthCheckResultStatus>))]
    [JsonPropertyName("status")]
    public HealthCheckResultStatus Status { get; set; } = HealthCheckResultStatus.Up;

    /// <summary>
    /// Summary of the health check for the configuration.
    /// </summary>
    [JsonPropertyName("configuration")]
    public HealthCheckConfigurationProblems Configuration { get; set; } = new HealthCheckConfigurationProblems();
    
    /// <summary>
    /// Summary of the health check for the loops.
    /// </summary>
    [JsonPropertyName("groupJoinRequestLoops")]
    public List<HealthCheckGroupLoopStatus>? GroupJoinRequestLoops { get; set; } = new List<HealthCheckGroupLoopStatus>();
}

[JsonSerializable(typeof(HealthCheckResult))]
[JsonSourceGenerationOptions(WriteIndented = true, IncludeFields = true, DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull)]
internal partial class HealthCheckResultJsonContext : JsonSerializerContext
{
}