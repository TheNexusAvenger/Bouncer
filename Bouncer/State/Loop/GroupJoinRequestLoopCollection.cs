using System;
using System.Collections.Generic;
using System.Linq;
using Bouncer.Diagnostic;

namespace Bouncer.State.Loop;

public class GroupJoinRequestLoopCollection
{
    /// <summary>
    /// Active loops for handling join requests.
    /// </summary>
    private readonly Dictionary<long, GroupJoinRequestLoop> _groupJoinRequestLoops = new Dictionary<long, GroupJoinRequestLoop>();

    /// <summary>
    /// Creates a group join request loop collection.
    /// </summary>
    public GroupJoinRequestLoopCollection()
    {
        // Connect the configuration changing.
        ConfigurationState.ConfigurationChanged += (_) =>
        {
            this.Refresh();
        };
        
        // Start the initial loops.
        this.Refresh();
    }
    
    /// <summary>
    /// Refreshes the group join request loops based on the current configuration.
    /// </summary>
    /// <summary>Configuration to refresh with.</summary>
    public void Refresh(Configuration configuration)
    {
        // Add the new loops.
        foreach (var group in configuration.Groups!)
        {
            var groupId = group.Id!.Value;
            if (this._groupJoinRequestLoops.ContainsKey(groupId)) continue;
            this._groupJoinRequestLoops[groupId] = new GroupJoinRequestLoop(groupId);
        }
        
        // Update the loops.
        foreach (var group in configuration.Groups!)
        {
            var groupId = group.Id!.Value;
            try
            {
                var loop = this._groupJoinRequestLoops[groupId];
                loop.Stop();
                loop.DryRun = group.DryRun;
                loop.OpenCloudApiKey = group.OpenCloudApiKey!;
                loop.SetRules(group.Rules!);
                loop.Start(group.LoopDelaySeconds);
            }
            catch (Exception e)
            {
                Logger.Error($"Error updating loop for group {groupId}.\n{e}");
            }
        }
        
        // Stop the loops that don't have configurations.
        foreach (var (robloxGroupId, loop) in this._groupJoinRequestLoops
                     .Where(loop => configuration.Groups!.All(group => loop.Value.RobloxGroupId != group.Id)).ToList())
        {
            Logger.Debug($"Stopping join requests for group {robloxGroupId}.");
            loop.Stop();
            this._groupJoinRequestLoops.Remove(robloxGroupId);
        }
    }
    
    /// <summary>
    /// Refreshes the group join request loops based on the current configuration.
    /// </summary>
    public void Refresh()
    {
        this.Refresh(ConfigurationState.Configuration);
    }
}