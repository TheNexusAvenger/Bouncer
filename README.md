# Bouncer
Bouncer is a rule-based application for automating Roblox group join
requests.

## Rules
Rules are made up of condition functions with boolean operations.
These operations can also be grouped together with parentheses.

The supported conditions include:
- `Always()` - Always true.
- `Never()` - Never true (always false).
- `IsInGroup(GroupId)` - True if the Roblox user is the given group id.
  - Ex: `IsInGroup(12345)` checks if the user is in group 12345
    (any rank).
- `GroupRankIs(GroupId, Equality, Rank)` - True if the Roblox user
  matches the rank in the group.
  - Equality can be `"LessThan"`, `"GreaterThan"`, `"EqualTo"`,
    `"NoGreaterThan"` (or `"LessThanOrEqualTo"`), and `"AtLeast"`
    (or `"GreaterThanOrEqualTo"`). These are all case-insensitive.
    Note: `LessThan` and `NoGreaterThan` will NOT return true if
    the user is not in the group.
  - Ex: `GroupRankIs(12345, "LessThan", 200)` returns true if the user
    is in group 12345 and has a rank less than 200.
  - Ex: `GroupRankIs(12345, "GreaterThan", 200)` returns true if the user
    has a rank greater than 200 in group 12345.
  - Ex: `GroupRankIs(12345, "EqualTo", 200)` returns true if the user
    has rank 200 in the group.
  - Ex: `GroupRankIs(12345, "AtLeast", 200)` returns true if the user
    does not have a rank less than 200 (has a rank greater than or equal
    to rank 200).
  - Ex: `GroupRankIs(12345, "NoGreaterThan", 200)` returns true if the user
    does not have a rank greater than 200 (has a rank less than or
    equal to rank 200).
- `IsUser(UserId)` - True if the Roblox user is the given user.
  - Ex: `IsUser(12345)` returns true if the user is user 12345.

For binary operations between conditions, `and`, `or`, `==`, and `!=`
are provided. Only `not` is provided for unary operations.

Examples:
- `IsInGroup(12345)` checks if the user is in group 12345n (any rank).
- `IsInGroup(12345) and not IsInGroup(23456)` checks if the user is in
  group 12345 and not group 23456.
- `IsInGroup(12345) and not (not IsInGroup(23456) or GroupRankIs(34567, "GreaterThan", 200))`
    checks if a user is in group 12345, but not (not in group 23456 or
    has a group rank greater than 200 in group 34567).

### Custom Conditions
There is no current support for custom conditions besides forking.
Pull requests to add this functionality, such as loading DLLs with
more conditions, would be considered.

## Setup
### Roblox Open Cloud API Key
In [Roblox Open Cloud's API Keys](https://create.roblox.com/dashboard/credentials?activeTab=ApiKeysTab),
an Open Cloud API key needs to be created **under a user** (not group)
with the `Access Permissions` of `groups` with `group:read` and
`group:write`. Due to the user requirement and no way to limit the
access to specific groups, an alternative account with only join
request permissions is recommended.

### Configuration
A configuration file named `configuration.json` is required to be set up.
When the application is started, it will create a default one that can
be modified. Specifically for Docker, it will be in `configuration/configuration.json`.

The configuration file can include:
- `Logging`:
  - `MinimumLogLevel` *(LogLevel)*: Minimum log level to include in the logs.
    Must be `Trace`, `Debug`, `Information`, `Warning`, `Error`, `Critical`,
    or `None`.
- `Groups` *(List)*:
  - `Id` *(Number)*: Id of the Roblox group.
  - `OpenCloudApiKey` *(String)*: API key that can read join requests.
  - `DryRun` *(Boolean)*: If true, join request will be checked, but no
    actions will be performed. This is recommended to try new rules.
  - `LoopDelaySeconds` *(Number)*: Seconds between checking the for
    join requests.
  - `Rules` *(List)*:
    - `Name` *(String)*: Optional name for the rule to show in the logs.
      If not provided, the `Rule` will be used instead.
    - `Rule` *(String)*: Rule to check. If true, the action will be
      performed.
    - `Action` *(JoinRequestAction)*: Action to perform if the rule
      matches. Must be `Accept`, `Decline`, or `Ignore`.

Rules will be evaluated for a user starting at the top of the list and
go down until a rule matches. If no rule matches, it will default.

Below is an example configuration to accept people in group 23456 with
a rank at least 200, ignore people in group 23456 (will be <200), and
decline everyone else by default.

```json
{
  "Groups": [
    {
      "Id": 12345,
      "OpenCloudApiKey": "MyRobloxOpenCloudApi",
      "DryRun": true,
      "LoopDelaySeconds": 300,
      "Rules": [
        {
          "Name": "Allow Main Group High Ranks",
          "Rule": "GroupRankIs(23456, \"AtLeast\", 200)",
          "Action": "Accept"
        },
        {
          "Name": "Ignore Main Group Low Ranks (Manual Approval)",
          "Rule": "IsInGroup(23456)",
          "Action": "Ignore"
        },
        {
          "Name": "Deny Other Users",
          "Rule": "Always()",
          "Action": "Decline"
        }
      ]
    }
  ],
  "Logging": {
    "MinimumLogLevel": "Debug"
  }
}
```

The groups configuration supports being reloaded while the application
is running. Anything else, such as log levels, requires a restart.

### Running
Bouncer uses Microsoft .NET to run. However, it is recommended to run it
in Docker.

```bash
docker compose up -d
```

Port 8000 will be opened by Bouncer, but it is only used for health checks
with the `/health` endpoint. It does not need to be exposed to the internet.

## License
Bouncer is available under the terms of the GNU Lesser General Public
License. See [LICENSE](LICENSE) for details.