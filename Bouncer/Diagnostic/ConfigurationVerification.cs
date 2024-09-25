using System;
using Bouncer.Diagnostic.Model;
using Bouncer.Expression;
using Bouncer.Parser;
using Bouncer.State;
using Sprache;

namespace Bouncer.Diagnostic;

public class BaseConfigurationVerification<T> where T : struct, Enum
{
    /// <summary>
    /// Verifies a rule entry.
    /// </summary>
    /// <param name="rule">Rule to check.</param>
    /// <param name="defaultAction">Default action to show when not provided.</param>
    /// <param name="result">Result to add errors to.</param>
    public static void VerifyRule(BaseRuleEntry<T> rule, T defaultAction, VerifyRulesResult result)
    {
        // Add configuration errors for missing fields.
        if (rule.Name == null)
        {
            Logger.Warn("\"Name\" is missing from Groups configuration entry rule.");
        }
        if (rule.Rule == null)
        {
            Logger.Error("\"Rule\" is missing from Groups configuration entry rule.");
            result.TotalRuleConfigurationErrors += 1;
        }
        if (rule.Action == null)
        {
            Logger.Error("\"Action\" is missing from Groups configuration entry rule.");
            result.TotalRuleConfigurationErrors += 1;
        }
        if (rule.Rule == null) return;
                
        // Try to parse the rule.
        var ruleName = $"{rule.Name ?? "[Unnamed rule]"} ({rule.Action ?? defaultAction})";
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
                result.TotalTransformErrors += 1;
            }
        }
        catch (Exception e)
        {
            Logger.Error($"Error parsing rule {ruleName}: {e.Message}");
            result.TotalParseErrors += 1;
        }
    }
}

public class ConfigurationVerification : BaseConfigurationVerification<JoinRequestAction>
{
    /// <summary>
    /// Verifies the rules for the groups.
    /// </summary>
    /// <param name="configuration">Configuration to validate.</param>
    public static VerifyRulesResult VerifyRules(Configuration configuration)
    {
        // Get the configuration and return if there are no rules.
        var result = new VerifyRulesResult();
        if (configuration.Groups == null)
        {
            Logger.Warn("\"Groups\" is missing from the configuration.");
            result.TotalRuleConfigurationErrors += 1;
            return result;
        }
        
        // Iterate over the groups and add any errors.
        foreach (var groupRules in configuration.Groups)
        {
            // Add configuration errors for missing fields.
            Logger.Info($"Configuration for group \"{groupRules.Id}\":");
            if (groupRules.Id == null)
            {
                Logger.Error("\"Id\" is missing from Groups configuration entry.");
                result.TotalRuleConfigurationErrors += 1;
            }
            if (groupRules.OpenCloudApiKey == null)
            {
                Logger.Error("\"OpenCloudApiKey\" is missing from Groups configuration entry.");
                result.TotalRuleConfigurationErrors += 1;
            }
            if (groupRules.Rules == null)
            {
                Logger.Error("\"Rules\" is missing from Groups configuration entry.");
                result.TotalRuleConfigurationErrors += 1;
            }
            if (groupRules.Rules == null) continue;
            
            // Iterate over the rules.
            foreach (var rule in groupRules.Rules)
            {
                VerifyRule(rule, JoinRequestAction.Ignore, result);
            }
        }
        
        // Return the final errors.
        return result;
    }

    /// <summary>
    /// Verifies the rules for the groups.
    /// </summary>
    public static VerifyRulesResult VerifyRules()
    {
        return VerifyRules(Configurations.GetConfiguration<Configuration>());
    }
}