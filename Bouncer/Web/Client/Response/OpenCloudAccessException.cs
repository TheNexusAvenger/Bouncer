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

public class OpenCloudAccessException : Exception
{
    /// <summary>
    /// Issue that caused the exception.
    /// </summary>
    public readonly OpenCloudAccessIssue Issue;
    
    /// <summary>
    /// Creates an Open Cloud access exception.
    /// </summary>
    /// <param name="issue">Issue for the exception.</param>
    public OpenCloudAccessException(OpenCloudAccessIssue issue) : base(issue.ToString())
    {
        this.Issue = issue;
    }
}

public class OpenCloudAccessException<T> : OpenCloudAccessException where T : BaseRobloxOpenCloudResponse
{
    /// <summary>
    /// Response for the request.
    /// </summary>
    public readonly T Response;
    
    /// <summary>
    /// Creates an Open Cloud access exception.
    /// </summary>
    /// <param name="issue">Issue for the exception.</param>
    /// <param name="response">Response with the exception.</param>
    /// <typeparam name="T">Type of the response.</typeparam>
    public OpenCloudAccessException(OpenCloudAccessIssue issue, T response) : base(issue)
    {
        this.Response = response;
    }
}