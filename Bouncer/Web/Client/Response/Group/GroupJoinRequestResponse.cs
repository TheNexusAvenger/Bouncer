using System.Collections.Generic;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

namespace Bouncer.Web.Client.Response.Group;

public partial class GroupJoinRequestEntry
{
    /// <summary>
    /// The resource path of the group_join_request. Format: groups/{group}/join-requests/{group_join_request}.
    /// </summary>
    [JsonPropertyName("path")]
    public string Path { get; set; } = null!;
    
    /// <summary>
    /// The timestamp when the group_join_request was created. This string is formatted as a Timestamp.
    /// </summary>
    [JsonPropertyName("createTime")]
    public string CreateTime { get; set; } = null!;
    
    /// <summary>
    /// The resource path of the user.
    /// </summary>
    [JsonPropertyName("user")]
    public string User { get; set; } = null!;

    /// <summary>
    /// User id of the join request from the user resource path.
    /// </summary>
    public long UserId => long.Parse(UserResourcePathRegex().Match(this.User).Groups[1].Value);

    /// <summary>
    /// Regex expression for the user resource path.
    /// </summary>
    [GeneratedRegex("users/(\\d+)")]
    private static partial Regex UserResourcePathRegex();
}

public class GroupJoinRequestResponse : BaseRobloxOpenCloudResponse
{
    /// <summary>
    /// List of join requests for the group.
    /// </summary>
    [JsonPropertyName("groupJoinRequests")]
    public List<GroupJoinRequestEntry> GroupJoinRequests { get; set; } = null!;
    
    /// <summary>
    /// A token that you can send as a pageToken parameter to retrieve the next page.
    /// If this field is omitted, there are no subsequent pages.
    /// </summary>
    [JsonPropertyName("nextPageToken")]
    public string? NextPageToken { get; set; }
}

[JsonSerializable(typeof(GroupJoinRequestResponse))]
[JsonSourceGenerationOptions(WriteIndented = true, IncludeFields = true, DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull)]
public partial class GroupJoinRequestResponseJsonContext : JsonSerializerContext
{
}