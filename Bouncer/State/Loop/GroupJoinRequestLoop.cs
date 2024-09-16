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

public class GroupJoinRequestLoop : BaseConfigurableLoop<GroupConfiguration>
{
    /// <summary>
    /// Status of the last step of the loop.
    /// </summary>
    public GroupJoinRequestLoopStatus Status { get; private set; } = GroupJoinRequestLoopStatus.NotStarted;

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
    /// <param name="initialConfiguration">Initial configuration of the loop.</param>
    /// <param name="robloxGroupClient">Roblox group client to perform requests with.</param>
    public GroupJoinRequestLoop(GroupConfiguration initialConfiguration, RobloxGroupClient robloxGroupClient) : base($"GroupJoinRequestLoop_{initialConfiguration.Id}", initialConfiguration)
    {
        this._robloxGroupClient = robloxGroupClient;
    }
    
    /// <summary>
    /// Creates a group join request loop.
    /// </summary>
    /// <param name="initialConfiguration">Initial configuration of the loop.</param>
    public GroupJoinRequestLoop(GroupConfiguration initialConfiguration) : this(initialConfiguration, new RobloxGroupClient())
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
        var robloxGroupId = this.Configuration.Id!.Value;
        var dryRun = this.Configuration.DryRun;
        var acceptedJoinRequests = 0;
        var declinedJoinRequests = 0;
        var ignoredJoinRequests = 0;
        var logPrefix = (dryRun ? "[DRY RUN] " : "");
        this.Status = GroupJoinRequestLoopStatus.Running;
        
        try
        {
            // Get the initial page of join requests.
            var joinRequests = await this._robloxGroupClient.GetJoinRequests(robloxGroupId);
            
            // Process pages until the end is reached.
            while (true)
            {
                // Handle the join requests.
                Logger.Info($"{logPrefix}Handling {joinRequests.GroupJoinRequests.Count} join requests for group {robloxGroupId}.");
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
                            Logger.Info($"{logPrefix}User {robloxUserId} matched rule \"{rule.Name}\" for group {robloxGroupId} and will be accepted.");
                            if (dryRun)
                            {
                                await this._robloxGroupClient.AcceptJoinRequestAsync(robloxGroupId, robloxUserId);
                            }
                            acceptedJoinRequests += 1;
                        }
                        else if (rule.Action == JoinRequestAction.Decline)
                        {
                            Logger.Info($"{logPrefix}User {robloxUserId} matched rule \"{rule.Name}\" for group {robloxGroupId} and will be declined.");
                            if (dryRun)
                            {
                                await this._robloxGroupClient.DeclineJoinRequestAsync(robloxGroupId, robloxUserId);
                            }
                            declinedJoinRequests += 1;
                        }
                        else if (rule.Action == JoinRequestAction.Ignore)
                        {
                            Logger.Info($"{logPrefix}User {robloxUserId} matched rule \"{rule.Name}\" for group {robloxGroupId} and will be ignored.");
                            ignoredJoinRequests += 1;
                        }
                        rulePassed = true;
                        break;
                    }
                    
                    // Ignore the user if they pass no rule.
                    if (!rulePassed)
                    {
                        Logger.Debug($"{logPrefix}User {robloxUserId} did not match any rules for group {robloxGroupId}. The join request will be ignored.");
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
                joinRequests = await this._robloxGroupClient.GetJoinRequests(robloxGroupId, pageToken: pageToken);
                if (pageToken == joinRequests.NextPageToken) // Original bug: https://devforum.roblox.com/t/open-cloud-groups-api-users-api-beta/2909090/38
                {
                    throw new InvalidDataException($"Duplicate next page token returned for group join requests ({pageToken}).");
                }
            }
            Logger.Info($"Reached end of join requests for {robloxGroupId}.");
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
            Logger.Info($"{logPrefix}Join requests for {robloxGroupId} summary: {acceptedJoinRequests} accepted, {declinedJoinRequests} declined, {ignoredJoinRequests} ignored.");
        }
    }

    /// <summary>
    /// Handles the configuration being set.
    /// This must handle starting the loop.
    /// </summary>
    public override void OnConfigurationSet()
    {
        var groupId = this.Configuration.Id!.Value;
        try
        {
            this.SetRules(this.Configuration.Rules!);
            this._robloxGroupClient.OpenCloudApiKey = this.Configuration.OpenCloudApiKey;
            this.Start(this.Configuration.LoopDelaySeconds);
        }
        catch (Exception e)
        {
            Logger.Error($"Error updating loop for group {groupId}.\n{e}");
        }
    }
}