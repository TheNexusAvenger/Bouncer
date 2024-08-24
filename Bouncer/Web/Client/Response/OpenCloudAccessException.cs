using System;

namespace Bouncer.Web.Client.Response;

public enum OpenCloudAccessIssue
{
    /// <summary>
    /// API key is empty.
    /// </summary>
    MissingApiKey,
    
    /// <summary>
    /// API key is invalid (malformed, disabled, not in CIDR).
    /// </summary>
    InvalidApiKey,
    
    /// <summary>
    /// Scope not granted.
    /// </summary>
    PermissionDenied,
    
    /// <summary>
    /// Authentication method is invalid (such as group API that requires a user API key).
    /// </summary>
    Unauthenticated,
    
    /// <summary>
    /// Too many requests were sent.
    /// </summary>
    TooManyRequests,
    
    /// <summary>
    /// An invalid Roblox user id was used.
    /// </summary>
    InvalidUser,
    
    /// <summary>
    /// Unknown exception.
    /// </summary>
    Unknown,
}

public class OpenCloudAccessException<T> : Exception where T : BaseRobloxOpenCloudResponse
{
    /// <summary>
    /// Issue that caused the exception.
    /// </summary>
    public OpenCloudAccessIssue Issue;

    /// <summary>
    /// Response for the request.
    /// </summary>
    public T Response = null!;
}