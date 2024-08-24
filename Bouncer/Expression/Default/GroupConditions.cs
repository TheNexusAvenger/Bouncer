﻿using System.Collections.Generic;
using System.IO;
using Bouncer.Web.Client;

namespace Bouncer.Expression.Default;

public class GroupConditions
{
    /// <summary>
    /// Condition for the Roblox user having a rank compared to the given rank in a Roblox group.
    /// </summary>
    public static bool GroupRankIsCondition(long robloxUserId, List<string> arguments) {
        var robloxGroupId = long.Parse(arguments[1]);
        var condition = arguments[2].ToLower();
        var rank = int.Parse(arguments[3]);
        var groupRank = RobloxGroupClient.Instance.GetRankInGroupAsync(robloxUserId, robloxGroupId).Result;
        if (condition == "equalto")
        {
            return groupRank == rank;
        }
        else if (condition == "lessthan")
        {
            return groupRank < rank;
        }
        else if (condition == "greaterthan")
        {
            return groupRank > rank;
        }
        throw new InvalidDataException($"Unsupported condition \"{condition}\". Must be EqualTo, LessThan, or GreaterThan.");
    }
    
    /// <summary>
    /// Condition for the Roblox user being in a Roblox group.
    /// </summary>
    public static bool IsInGroupCondition(long robloxUserId, List<string> arguments)
    {
        var robloxGroupId = long.Parse(arguments[1]);
        return RobloxGroupClient.Instance.GetRankInGroupAsync(robloxUserId, robloxGroupId).Result > 0;
    }
}