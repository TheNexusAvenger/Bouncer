using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Bouncer.State;

public class JoinRequestRuleEntry
{
    /// <summary>
    /// Optional name of the role.
    /// </summary>
    public string? Name { get; set; }
    
    /// <summary>
    /// Rule for the 
    /// </summary>
    public string? Rule { get; set; }
    
    /// <summary>
    /// Action to perform when the rule applies.
    /// </summary>
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public JoinRequestAction? Action { get; set; }
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
    /// Rules to use for accepting or declining users.
    /// </summary>
    public List<JoinRequestRuleEntry>? Rules { get; set; }
}

public class Configuration
{
    /// <summary>
    /// List of group configurations managed by the application.
    /// </summary>
    public List<GroupConfiguration>? Groups { get; set; }

    /// <summary>
    /// Reads the configuration from the file system.
    /// </summary>
    /// <returns>Configuration that was read, or the default one.</returns>
    public static async Task<Configuration> ReadConfigurationAsync()
    {
        // Get the configuration path.
        var path = ConfigurationState.GetConfigurationPath();
        
        // Write the configuration if it doesn't exist and then load the configuration.
        if (!File.Exists(path))
        {
            await File.WriteAllTextAsync(path, JsonSerializer.Serialize(new Configuration()
            {
                Groups = new List<GroupConfiguration>()
                {
                    new GroupConfiguration()
                    {
                        Id = 12345L,
                        OpenCloudApiKey = "ReplaceWithOpenCloudKey",
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
                                Rule = "Default()",
                                Action = JoinRequestAction.Decline,
                            },
                        }
                    },
                },
            }, ConfigurationJsonContext.Default.Configuration));
        }
        return JsonSerializer.Deserialize<Configuration>(await File.ReadAllTextAsync(path), ConfigurationJsonContext.Default.Configuration)!;
    }
}

[JsonSerializable(typeof(Configuration))]
[JsonSerializable(typeof(GroupConfiguration))]
[JsonSerializable(typeof(JoinRequestRuleEntry))]
[JsonSourceGenerationOptions(WriteIndented=true, IncludeFields = true)]
internal partial class ConfigurationJsonContext : JsonSerializerContext
{
}