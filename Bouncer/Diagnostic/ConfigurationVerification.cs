using System;
using Bouncer.Diagnostic.Model;
using Bouncer.Expression;
using Bouncer.Parser;
using Bouncer.State;
using Sprache;

namespace Bouncer.Diagnostic;

public class ConfigurationVerification
{
    /// <summary>
    /// Verifies the rules for the groups.
    /// </summary>
    /// <param name="configuration">Configuration to validate.</param>
    public static VerifyRulesResult VerifyRules(Configuration configuration)
    {
        // Get the configuration and return if there are no rules.
        var ruleConfigurationErrors = 0;
        if (configuration.Groups == null)
        {
            Logger.Warn("\"Groups\" is missing from the configuration.");
            ruleConfigurationErrors += 1;
            return new VerifyRulesResult()
            {
                TotalRuleConfigurationErrors = ruleConfigurationErrors,
            };
        }
        
        // Iterate over the groups and add any errors.
        var parseErrors = 0;
        var transformErrors = 0;
        foreach (var groupRules in configuration.Groups)
        {
            // Add configuration errors for missing fields.
            Logger.Info($"Configuration for group \"{groupRules.Id}\":");
            if (groupRules.Id == null)
            {
                Logger.Error("\"Id\" is missing from Groups configuration entry.");
                ruleConfigurationErrors += 1;
            }
            if (groupRules.OpenCloudApiKey == null)
            {
                Logger.Error("\"OpenCloudApiKey\" is missing from Groups configuration entry.");
                ruleConfigurationErrors += 1;
            }
            if (groupRules.Rules == null)
            {
                Logger.Error("\"Rules\" is missing from Groups configuration entry.");
                ruleConfigurationErrors += 1;
            }
            if (groupRules.Rules == null) continue;
            
            // Iterate over the rules.
            foreach (var rule in groupRules.Rules)
            {
                // Add configuration errors for missing fields.
                if (rule.Name == null)
                {
                    Logger.Warn("\"Name\" is missing from Groups configuration entry rule.");
                }
                if (rule.Rule == null)
                {
                    Logger.Error("\"Rule\" is missing from Groups configuration entry rule.");
                    ruleConfigurationErrors += 1;
                }
                if (rule.Action == null)
                {
                    Logger.Error("\"Action\" is missing from Groups configuration entry rule.");
                    ruleConfigurationErrors += 1;
                }
                if (rule.Rule == null) continue;
                
                // Try to parse the rule.
                var ruleName = $"{rule.Name ?? "[Unnamed rule]"} ({rule.Action ?? JoinRequestAction.Ignore})";
                try
                {
                    var parsedCondition = ExpressionParser.FullExpressionParser.Parse(rule.Rule);
                    try
                    {
                        var condition = Condition.FromParsedCondition(parsedCondition);
                        Logger.Info($"Rule {ruleName}: {condition}");
                    }
                    catch (Exception e)
                    {
                        Logger.Error($"Error transforming rule {ruleName}: {e.Message}");
                        transformErrors += 1;
                    }
                }
                catch (Exception e)
                {
                    Logger.Error($"Error parsing rule {ruleName}: {e.Message}");
                    parseErrors += 1;
                }
            }
        }
        
        // Return the final errors.
        return new VerifyRulesResult()
        {
            TotalRuleConfigurationErrors = ruleConfigurationErrors,
            TotalParseErrors = parseErrors,
            TotalTransformErrors = transformErrors,
        };
    }

    /// <summary>
    /// Verifies the rules for the groups.
    /// </summary>
    public static VerifyRulesResult VerifyRules()
    {
        return VerifyRules(ConfigurationState.Configuration);
    }
}