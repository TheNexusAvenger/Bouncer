using System.Collections.Generic;
using Bouncer.Web.Server.Model;

namespace Bouncer.State.Loop;

public class GroupJoinRequestLoopCollection : GenericLoopCollection<GroupJoinRequestLoop, Configuration, GroupConfiguration>
{
    /// <summary>
    /// Returns the status of the loops.
    /// </summary>
    /// <returns>The status of the loops.</returns>
    public List<HealthCheckGroupLoopStatus> GetStatus()
    {
        var loopStatuses = new List<HealthCheckGroupLoopStatus>();
        foreach (var (groupId, loop) in this.ActiveLoops)
        {
            var healthCheckStatus = HealthCheckResultStatus.Up;
            if (loop.Status == GroupJoinRequestLoopStatus.InvalidApiKey || loop.Status == GroupJoinRequestLoopStatus.Error)
            {
                healthCheckStatus = HealthCheckResultStatus.Down;
            }
            loopStatuses.Add(new HealthCheckGroupLoopStatus()
            {
                Status = healthCheckStatus,
                GroupId = long.Parse(groupId),
                LastStepStatus = loop.Status,
            });
        }
        return loopStatuses;
    }

    /// <summary>
    /// Returns the list of configuration entries from the current configuration.
    /// </summary>
    /// <param name="configuration">Configuration to get the entries from.</param>
    /// <returns>List of configuration entries.</returns>
    public override List<GroupConfiguration> GetConfigurationEntries(Configuration configuration)
    {
        return configuration.Groups!;
    }

    /// <summary>
    /// Returns the loop id for the configuration.
    /// </summary>
    /// <param name="configuration">Configuration to get the key from.</param>
    /// <returns>Key id for the configuration loop.</returns>
    public override string GetLoopKeyId(GroupConfiguration configuration)
    {
        return configuration.Id?.ToString() ?? "0";
    }

    /// <summary>
    /// Returns the loop instance for a configuration.
    /// </summary>
    /// <param name="configuration">Configuration to get the key from.</param>
    /// <returns>Loop for the configuration.</returns>
    public override GroupJoinRequestLoop CreateLoop(GroupConfiguration configuration)
    {
        return new GroupJoinRequestLoop(configuration);
    }
}