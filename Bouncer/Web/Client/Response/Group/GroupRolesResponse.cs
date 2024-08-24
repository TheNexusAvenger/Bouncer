using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Bouncer.Web.Client.Response.Group;

public class GroupData
{
    /// <summary>
    /// Id of the group.
    /// </summary>
    [JsonPropertyName("id")]
    public long Id { get; set; }

    /// <summary>
    /// Name of the group.
    /// </summary>
    [JsonPropertyName("name")]
    public string Name { get; set; } = null!;
    
    /// <summary>
    /// Number of members in the group.
    /// </summary>
    [JsonPropertyName("memberCount")]
    public int MemberCount { get; set; }
    
    /// <summary>
    /// Whether the group has the verified badge.
    /// </summary>
    [JsonPropertyName("hasVerifiedBadge")]
    public bool HasVerifiedBadge { get; set; }
}

public class GroupRoleData
{
    /// <summary>
    /// Id of the role.
    /// </summary>
    [JsonPropertyName("id")]
    public long Id { get; set; }

    /// <summary>
    /// Name of the role.
    /// </summary>
    [JsonPropertyName("name")]
    public string Name { get; set; } = null!;
    
    /// <summary>
    /// Rank in the group.
    /// </summary>
    [JsonPropertyName("rank")]
    public int Rank { get; set; }
}

public class GroupRolesResponseDataEntry
{
    /// <summary>
    /// Group that the user is in.
    /// </summary>
    [JsonPropertyName("group")]
    public GroupData Group { get; set; } = null!;
    
    /// <summary>
    /// Role that the user has in the group.
    /// </summary>
    [JsonPropertyName("role")]
    public GroupRoleData Role { get; set; } = null!;
}

public class GroupRolesResponse : BaseRobloxOpenCloudResponse
{
    /// <summary>
    /// List of the groups the user is in.
    /// </summary>
    [JsonPropertyName("data")]
    public List<GroupRolesResponseDataEntry> Data { get; set; } = null!;
}

[JsonSerializable(typeof(GroupRolesResponse))]
[JsonSourceGenerationOptions(WriteIndented = true, IncludeFields = true)]
public partial class GroupRolesResponseJsonContext : JsonSerializerContext
{
}