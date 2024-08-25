using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Bouncer.Diagnostic;
using Bouncer.Expression;
using Bouncer.Parser;
using Bouncer.Web.Client;
using Bouncer.Web.Client.Response;
using Sprache;

namespace Bouncer.State.Loop;

public enum GroupJoinRequestLoopStatus
{
    /// <summary>
    /// The loop has not been started yet.
    /// </summary>
    NotStarted,
    
    /// <summary>
    /// The loop is actively handling join requests.
    /// </summary>
    Running,
    
    /// <summary>
    /// The loop completed with no exceptions.
    /// </summary>
    Complete,
    
    /// <summary>
    /// Too many requests and won't run again until the next step.
    /// </summary>
    TooManyRequests,
    
    /// <summary>
    /// API key is invalid.
    /// </summary>
    InvalidApiKey,
    
    /// <summary>
    /// An error occured during the last step.
    /// </summary>
    Error,
}

public class GroupConditionEntry
{
    /// <summary>
    /// Name of the condition.
    /// </summary>
    public string Name { get; set; } = null!;
    
    /// <summary>
    /// Action to perform for the condition.
    /// </summary>
    public JoinRequestAction Action { get; set; }

    /// <summary>
    /// Rule to evaluate.
    /// </summary>
    public Condition Rule { get; set; } = null!;
}

public class GroupJoinRequestLoop : BaseLoop
{
    /// <summary>
    /// Status of the last step of the loop.
    /// </summary>
    public GroupJoinRequestLoopStatus Status { get; private set; } = GroupJoinRequestLoopStatus.NotStarted;
    
    /// <summary>
    /// Roblox Open Cloud API key.
    /// </summary>
    public string? OpenCloudApiKey {
        get => this._robloxGroupClient.OpenCloudApiKey;
        set => this._robloxGroupClient.OpenCloudApiKey = value;
    }

    /// <summary>
    /// Roblox group id the loop handles join requests for.
    /// </summary>
    public readonly long RobloxGroupId;

    /// <summary>
    /// If true, join requests will be read but not attempted.
    /// </summary>
    public bool DryRun { get; set; } = false;

    /// <summary>
    /// Client used for sending Roblox group requests.
    /// </summary>
    private readonly RobloxGroupClient _robloxGroupClient;

    /// <summary>
    /// List of rules to check.
    /// </summary>
    private List<GroupConditionEntry> _rules = new List<GroupConditionEntry>();
    
    /// <summary>
    /// Creates a group join request loop.
    /// </summary>
    /// <param name="robloxGroupId">Group id to handle join requests for.</param>
    /// <param name="robloxGroupClient">Roblox group client to perform requests with.</param>
    public GroupJoinRequestLoop(long robloxGroupId, RobloxGroupClient robloxGroupClient) : base($"GroupJoinRequestLoop_{robloxGroupId}")
    {
        this.RobloxGroupId = robloxGroupId;
        this._robloxGroupClient = robloxGroupClient;
    }
    
    /// <summary>
    /// Creates a group join request loop.
    /// </summary>
    /// <param name="robloxGroupId">Group id to handle join requests for.</param>
    public GroupJoinRequestLoop(long robloxGroupId) : this(robloxGroupId, new RobloxGroupClient())
    {
        
    }

    /// <summary>
    /// Sets the rules for the join requests.
    /// </summary>
    /// <param name="rules"></param>
    public void SetRules(List<JoinRequestRuleEntry> rules)
    {
        var newRules = new List<GroupConditionEntry>();
        foreach (var rule in rules)
        {
            var parsedRule = Condition.FromParsedCondition(ExpressionParser.FullExpressionParser.Parse(rule.Rule));
            newRules.Add(new GroupConditionEntry()
            {
                Name = rule.Name ?? parsedRule.ToString(),
                Action = rule.Action ?? JoinRequestAction.Ignore,
                Rule = parsedRule,
            });
        }
        this._rules = newRules;
    }

    /// <summary>
    /// Runs a step in the loop.
    /// </summary>
    public override async Task RunAsync()
    {
        // Prepare the stats.
        var acceptedJoinRequests = 0;
        var declinedJoinRequests = 0;
        var ignoredJoinRequests = 0;
        var logPrefix = (this.DryRun ? "[DRY RUN] " : "");
        this.Status = GroupJoinRequestLoopStatus.Running;
        
        try
        {
            // Get the initial page of join requests.
            var joinRequests = await this._robloxGroupClient.GetJoinRequests(this.RobloxGroupId);
            
            // Process pages until the end is reached.
            while (true)
            {
                // Handle the join requests.
                Logger.Info($"{logPrefix}Handling {joinRequests.GroupJoinRequests.Count} join requests for group {this.RobloxGroupId}.");
                foreach (var joinRequest in joinRequests.GroupJoinRequests)
                {
                    // Check the rules.
                    var robloxUserId = joinRequest.UserId;
                    var rulePassed = false;
                    foreach (var rule in this._rules)
                    {
                        if (!rule.Rule.Evaluate(robloxUserId)) continue;
                        if (rule.Action == JoinRequestAction.Accept)
                        {
                            Logger.Info($"{logPrefix}User {robloxUserId} matched rule \"{rule.Name}\" for group {this.RobloxGroupId} and will be accepted.");
                            if (!this.DryRun)
                            {
                                await this._robloxGroupClient.AcceptJoinRequestAsync(robloxUserId, this.RobloxGroupId);
                            }
                            acceptedJoinRequests += 1;
                        }
                        else if (rule.Action == JoinRequestAction.Decline)
                        {
                            Logger.Info($"{logPrefix}User {robloxUserId} matched rule \"{rule.Name}\" for group {this.RobloxGroupId} and will be declined.");
                            if (!this.DryRun)
                            {
                                await this._robloxGroupClient.DeclineJoinRequestAsync(robloxUserId, this.RobloxGroupId);
                            }
                            declinedJoinRequests += 1;
                        }
                        else if (rule.Action == JoinRequestAction.Ignore)
                        {
                            Logger.Info($"{logPrefix}User {robloxUserId} matched rule \"{rule.Name}\" for group {this.RobloxGroupId} and will be ignored.");
                            ignoredJoinRequests += 1;
                        }
                        rulePassed = true;
                        break;
                    }
                    
                    // Ignore the user if they pass no rule.
                    if (!rulePassed)
                    {
                        Logger.Debug($"{logPrefix}User {robloxUserId} did not match any rules for group {this.RobloxGroupId}. The join request will be ignored.");
                        ignoredJoinRequests += 1;
                    }
                }
                
                // Stop the loop if there are no more join requests.
                if (string.IsNullOrEmpty(joinRequests.NextPageToken))
                {
                    break;
                }
                
                // Prepare the next page of join requests.
                var pageToken = joinRequests.NextPageToken;
                joinRequests = await this._robloxGroupClient.GetJoinRequests(this.RobloxGroupId, pageToken: pageToken);
                if (pageToken == joinRequests.NextPageToken) // TODO: Roblox bug? https://devforum.roblox.com/t/open-cloud-groups-api-users-api-beta/2909090/38
                {
                    throw new InvalidDataException($"Duplicate next page token returned for group join requests ({pageToken}).");
                }
            }
            Logger.Info($"Reached end of join requests for {this.RobloxGroupId}.");
            this.Status = GroupJoinRequestLoopStatus.Complete;
        }
        catch (OpenCloudAccessException e)
        {
            // Change the status and throw the exception if it wasn't too many requests.
            if (e.Issue == OpenCloudAccessIssue.TooManyRequests)
            {
                Logger.Warn($"Loop \"{this.Name}\" ran out of requests. Join requests will be continued in the next step.");
                this.Status = GroupJoinRequestLoopStatus.TooManyRequests;
            }
            else if (e.Issue != OpenCloudAccessIssue.Unknown)
            {
                Logger.Error($"Loop \"{this.Name}\" failed due to an invalid or misconfigured API key.");
                this.Status = GroupJoinRequestLoopStatus.InvalidApiKey;
                throw;
            }
            else
            {
                this.Status = GroupJoinRequestLoopStatus.Error;
                throw;
            }
        }
        catch (Exception)
        {
            // Change the status and throw the exception up.
            this.Status = GroupJoinRequestLoopStatus.Error;
            throw;
        }
        finally
        {
            // Log the stats.
            Logger.Info($"{logPrefix}Join requests for {this.RobloxGroupId} summary: {acceptedJoinRequests} accepted, {declinedJoinRequests} declined, {ignoredJoinRequests} ignored.");
        }
    }
}