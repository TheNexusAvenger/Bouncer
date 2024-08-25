using System.Collections.Generic;

namespace Bouncer.Expression.Default;

public class UserConditions
{
    /// <summary>
    /// Condition for the Roblox user a given user.
    /// </summary>
    public static bool IsInGroupCondition(long robloxUserId, List<string> arguments)
    {
        return robloxUserId == long.Parse(arguments[0]);
    }
}