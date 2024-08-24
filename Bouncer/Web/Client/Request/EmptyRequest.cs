using System.Text.Json.Serialization;

namespace Bouncer.Web.Client.Request;

public class EmptyRequest
{
    
}

[JsonSerializable(typeof(EmptyRequest))]
[JsonSourceGenerationOptions(WriteIndented = true, IncludeFields = true)]
public partial class EmptyRequestJsonContext : JsonSerializerContext
{
}