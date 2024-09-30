using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;

namespace Bouncer.State;

public abstract class BaseRuleEntry<T> where T : struct, Enum
{
    /// <summary>
    /// Optional name of the role.
    /// </summary>
    public string? Name { get; set; }
    
    /// <summary>
    /// Rule for the action.
    /// </summary>
    public string? Rule { get; set; }
    
    /// <summary>
    /// Action to perform when the rule applies.
    /// Left as abstract to allow implementations to set JsonStringEnumConverter.
    /// </summary>
    public abstract T? Action { get; set; }
}

public class JoinRequestRuleEntry : BaseRuleEntry<JoinRequestAction>
{
    /// <summary>
    /// Action to perform when the rule applies.
    /// </summary>
    [JsonConverter(typeof(JsonStringEnumConverter<JoinRequestAction>))]
    public override JoinRequestAction? Action { get; set; }
}

public class GroupConfiguration
{
    /// <summary>
    /// Id of the group.
    /// </summary>
    public long? Id { get; set; }
    
    /// <summary>
    /// API key for Roblox Open Cloud.
    /// </summary>
    public string? OpenCloudApiKey { get; set; }

    /// <summary>
    /// If true, join requests will be read but not attempted.
    /// </summary>
    public bool DryRun { get; set; } = false;

    /// <summary>
    /// Time (in seconds) between running steps of the join request loop.
    /// </summary>
    public ulong LoopDelaySeconds = 5 * 60;
    
    /// <summary>
    /// Rules to use for accepting or declining users.
    /// </summary>
    public List<JoinRequestRuleEntry>? Rules { get; set; }
}

public class LoggingConfiguration
{
    /// <summary>
    /// Minimum log level to show in the logs.
    /// </summary>
    [JsonConverter(typeof(JsonStringEnumConverter<LogLevel>))]
    public LogLevel MinimumLogLevel { get; set; } = LogLevel.Information;
}

public class Configuration
{
    /// <summary>
    /// List of group configurations managed by the application.
    /// </summary>
    public List<GroupConfiguration>? Groups { get; set; }

    /// <summary>
    /// Configuration for logging.
    /// </summary>
    public LoggingConfiguration Logging { get; set; } = new LoggingConfiguration();

    /// <summary>
    /// Returns the default configuration to use if the configuration file doesn't exist.
    /// </summary>
    /// <returns>Default configuration to store.</returns>
    public static Configuration GetDefaultConfiguration()
    {
        return new Configuration()
        {
            Groups = new List<GroupConfiguration>()
            {
                new GroupConfiguration()
                {
                    Id = 12345L,
                    OpenCloudApiKey = "ReplaceWithOpenCloudKey",
                    DryRun = true,
                    Rules = new List<JoinRequestRuleEntry>
                    {
                        new JoinRequestRuleEntry()
                        {
                            Name = "Test Rule 1",
                            Rule = "IsInGroup(12345) and not IsInGroup(23456)",
                            Action = JoinRequestAction.Accept,
                        },
                        new JoinRequestRuleEntry()
                        {
                            Name = "Test Rule 2",
                            Rule = "IsInGroup(23456)",
                            Action = JoinRequestAction.Ignore,
                        },
                        new JoinRequestRuleEntry()
                        {
                            Name = "Test Rule 3",
                            Rule = "Always()",
                            Action = JoinRequestAction.Decline,
                        },
                    }
                },
            },
        };
    }
}

[JsonSerializable(typeof(Configuration))]
[JsonSerializable(typeof(GroupConfiguration))]
[JsonSerializable(typeof(JoinRequestRuleEntry))]
[JsonSourceGenerationOptions(WriteIndented = true, IncludeFields = true, DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull)]
internal partial class ConfigurationJsonContext : JsonSerializerContext
{
}