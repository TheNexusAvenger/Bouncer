using System.Linq;
using System.Threading.Tasks;
using Bouncer.Web.Client.Response.Group;
using Bouncer.Web.Client.Shim;

namespace Bouncer.Web.Client;

public class RobloxGroupClient
{
    /// <summary>
    /// Roblox Open Cloud client to non-caching send requests.
    /// </summary>
    private readonly RobloxOpenCloudClient _robloxClient;
    
    /// <summary>
    /// Roblox Open Cloud client to send caching requests.
    /// </summary>
    private readonly RobloxOpenCloudClient _cachingRobloxClient;
    
    /// <summary>
    /// Roblox Open Cloud API key.
    /// </summary>
    public string? OpenCloudApiKey {
        get => this._robloxClient.OpenCloudApiKey;
        set => this._robloxClient.OpenCloudApiKey = value;
    }
    
    /// <summary>
    /// Creates a Roblox Group API client.
    /// </summary>
    /// <param name="httpClient">HTTP client to send requests with.</param>
    /// <param name="cachingHttpClient">Caching HTTP client to send requests with.</param>
    public RobloxGroupClient(IHttpClient httpClient, IHttpClient cachingHttpClient)
    {
        this._robloxClient = new RobloxOpenCloudClient(httpClient);
        this._cachingRobloxClient = new RobloxOpenCloudClient(cachingHttpClient);
    }
    
    /// <summary>
    /// Creates a Roblox Group API client.
    /// </summary>
    public RobloxGroupClient() : this(new HttpClientImpl(), CachingHttpClient.Instance)
    {
        
    }
    
    /// <summary>
    /// Clears the cache.
    /// </summary>
    public async Task ClearCacheAsync()
    {
        await this._cachingRobloxClient.ClearCacheAsync();
    }

    /// <summary>
    /// Returns the rank in a group for the user.
    /// Invalid users will throw an exception.
    /// </summary>
    /// <param name="robloxUserId">Roblox user id to find.</param>
    /// <param name="robloxGroupId">Roblox group id to find.</param>
    /// <returns>The rank in the group (or 0 if not in the group).</returns>
    public async Task<int> GetRankInGroupAsync(long robloxUserId, long robloxGroupId)
    {
        var groupRoles = await this._cachingRobloxClient.GetAsync($"https://groups.roblox.com/v2/users/{robloxUserId}/groups/roles", GroupRolesResponseJsonContext.Default.GroupRolesResponse);
        return groupRoles.Data.FirstOrDefault(group => group.Group.Id == robloxGroupId)?.Role.Rank ?? 0;
    }

    /// <summary>
    /// Returns the join requests for a group.
    /// </summary>
    /// <param name="robloxGroupId">Roblox group id to get the group requests of.</param>
    /// <param name="pageToken">Optional token for the join request page to get.</param>
    /// <param name="maxPageSize">Optional max amount of join requests to get.</param>
    /// <param name="filter">Optional filter for the join requests.</param>
    /// <returns>Join requests for the group.</returns>
    public async Task<GroupJoinRequestResponse> GetJoinRequests(long robloxGroupId, string? pageToken = null, int maxPageSize = 20, string? filter = null)
    {
        var url = $"https://apis.roblox.com/cloud/v2/groups/{robloxGroupId}/join-requests?maxPageSize={maxPageSize}";
        if (pageToken != null)
        {
            url = $"{url}&pageToken={pageToken}";
        }
        if (filter != null)
        {
            url = $"{url}&filter={filter}";
        }
        return await this._robloxClient.GetAsync(url, GroupJoinRequestResponseJsonContext.Default.GroupJoinRequestResponse);
    }
    
    /// <summary>
    /// Accept a group join request.
    /// </summary>
    /// <param name="robloxGroupId">Roblox group id to accept the join request of.</param>
    /// <param name="robloxUserId">Roblox user id of the join request to accept.</param>
    public async Task AcceptJoinRequestAsync(long robloxGroupId, long robloxUserId)
    {
        var url = $"https://apis.roblox.com/cloud/v2/groups/{robloxGroupId}/join-requests/{robloxUserId}:accept";
        await this._robloxClient.PostAsync(url);
    }
    
    /// <summary>
    /// Declines a group join request.
    /// </summary>
    /// <param name="robloxGroupId">Roblox group id to decline the join request of.</param>
    /// <param name="robloxUserId">Roblox user id of the join request to decline.</param>
    public async Task DeclineJoinRequestAsync(long robloxGroupId, long robloxUserId)
    {
        var url = $"https://apis.roblox.com/cloud/v2/groups/{robloxGroupId}/join-requests/{robloxUserId}:decline";
        await this._robloxClient.PostAsync(url);
    }
}