using System.Net;

namespace Bouncer.Web.Client.Shim;

public class HttpStringResponseMessage
{
    /// <summary>
    /// Status code of the response.
    /// </summary>
    public HttpStatusCode StatusCode { get; set; }

    /// <summary>
    /// Body of the response.
    /// </summary>
    public string Content { get; set; } = null!;
}