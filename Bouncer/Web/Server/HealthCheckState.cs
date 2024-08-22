using Bouncer.Diagnostic;
using Bouncer.Diagnostic.Model;
using Bouncer.State;
using Bouncer.Web.Server.Model;

namespace Bouncer.Web.Server;

public class HealthCheckState
{
    /// <summary>
    /// Last result for the rules being verified.
    /// </summary>
    private VerifyRulesResult _lastVerifyRulesResult;

    /// <summary>
    /// Creates a health check state.
    /// </summary>
    public HealthCheckState()
    {
        // Set the initial health check values.
        this._lastVerifyRulesResult = ConfigurationVerification.VerifyRules();
        
        // Connect the configuration changing.
        ConfigurationState.ConfigurationChanged += (_) => this.UpdateVerifyRulesResult();
    }

    /// <summary>
    /// Determines the current health check result.
    /// </summary>
    /// <param name="verifyRulesResult">Rules result to create the health check for.</param>
    /// <returns>The current health check result.</returns>
    public HealthCheckResult GetHealthCheckResult(VerifyRulesResult verifyRulesResult)
    {
        var hasConfigurationIssues = (verifyRulesResult.TotalRuleConfigurationErrors != 0 ||
                                      verifyRulesResult.TotalParseErrors != 0 ||
                                      verifyRulesResult.TotalTransformErrors != 0);
        return new HealthCheckResult()
        {
            Status = (hasConfigurationIssues ? HealthCheckResultStatus.Down : HealthCheckResultStatus.Up),
            Configuration = new HealthCheckConfigurationProblems()
            {
                Status = (hasConfigurationIssues ? HealthCheckResultStatus.Down : HealthCheckResultStatus.Up),
                TotalRuleConfigurationErrors = verifyRulesResult.TotalRuleConfigurationErrors,
                TotalRuleParseErrors = verifyRulesResult.TotalParseErrors,
                TotalRuleTransformErrors = verifyRulesResult.TotalTransformErrors,
            },
        };
    }

    /// <summary>
    /// Determines the current health check result.
    /// </summary>
    /// <returns>The current health check result.</returns>
    public HealthCheckResult GetHealthCheckResult()
    {
        return this.GetHealthCheckResult(this._lastVerifyRulesResult);
    }
    
    /// <summary>
    /// Updates the verify rules result.
    /// </summary>
    private void UpdateVerifyRulesResult()
    {
        this._lastVerifyRulesResult = ConfigurationVerification.VerifyRules();
    }
}