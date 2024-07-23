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
            // TODO: Log no groups.
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
            if (groupRules.Id == null)
            {
                // TODO: Log no id.
                ruleConfigurationErrors += 1;
            }
            if (groupRules.OpenCloudApiKey == null)
            {
                // TODO: Log no Open Cloud API key.
                ruleConfigurationErrors += 1;
            }
            if (groupRules.Rules == null)
            {
                // TODO: Log no Rules.
                ruleConfigurationErrors += 1;
            }
            if (groupRules.Rules == null) continue;
            
            // Iterate over the rules.
            foreach (var rule in groupRules.Rules)
            {
                // Add configuration errors for missing fields.
                if (rule.Name == null)
                {
                    // TODO: Warn about no name.
                }
                if (rule.Rule == null)
                {
                    // TODO: Log no Rule.
                    ruleConfigurationErrors += 1;
                }
                if (rule.Action == null)
                {
                    // TODO: Log no Action.
                    ruleConfigurationErrors += 1;
                }
                if (rule.Rule == null) continue;
                
                // Try to parse the rule.
                try
                {
                    var parsedCondition = ExpressionParser.FullExpressionParser.Parse(rule.Rule);
                    try
                    {
                        var condition = Condition.FromParsedCondition(parsedCondition);
                        // TODO: Log rule.
                    }
                    catch (Exception e)
                    {
                        // TODO: Log exception.
                        transformErrors += 1;
                    }
                }
                catch (Exception e)
                {
                    // TODO: Log exception.
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