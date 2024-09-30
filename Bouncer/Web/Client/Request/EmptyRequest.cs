using System.Text.Json.Serialization;

namespace Bouncer.Web.Client.Request;

public class EmptyRequest
{
    
}

[JsonSerializable(typeof(EmptyRequest))]
[JsonSourceGenerationOptions(WriteIndented = true, IncludeFields = true, DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull)]
public partial class EmptyRequestJsonContext : JsonSerializerContext
{
}