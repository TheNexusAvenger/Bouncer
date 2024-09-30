using System.Collections.Generic;
using System.Net;
using System.Text.Json.Serialization;

namespace Bouncer.Web.Client.Response;

public class RobloxOpenCloudError
{
    /// <summary>
    /// Message of the error.
    /// </summary>
    [JsonPropertyName("message")]
    public string Message = null!;

    /// <summary>
    /// Code of the error.
    /// </summary>
    [JsonPropertyName("code")]
    public int Code;
}

public class BaseRobloxOpenCloudResponse
{
    /// <summary>
    /// Status code of the response.
    /// </summary>
    public HttpStatusCode StatusCode;

    /// <summary>
    /// Message of the response.
    /// </summary>
    [JsonPropertyName("message")]
    public string? Message;

    /// <summary>
    /// Code of the message.
    /// </summary>
    [JsonPropertyName("code")]
    public string? Code;

    /// <summary>
    /// Errors from the response.
    /// </summary>
    [JsonPropertyName("errors")]
    public List<RobloxOpenCloudError>? Errors;
}

[JsonSerializable(typeof(RobloxOpenCloudError))]
[JsonSerializable(typeof(BaseRobloxOpenCloudResponse))]
[JsonSourceGenerationOptions(WriteIndented = true, IncludeFields = true, DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull)]
public partial class BaseRobloxOpenCloudResponseJsonContext : JsonSerializerContext
{
}