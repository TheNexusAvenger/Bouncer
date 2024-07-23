using System;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.Threading.Tasks;
using Bouncer.Diagnostic;

namespace Bouncer;

public class Program
{
    /// <summary>
    /// Command option for verifying the configuration and rules.
    /// </summary>
    public static readonly Option<bool> VerifyOption = new Option<bool>("--verify", "Verifies the current configuration and rules.");
    
    /// <summary>
    /// Runs the program.
    /// </summary>
    /// <param name="args">Arguments from the command line.</param>
    public static async Task Main(string[] args)
    {
        var rootCommand = new RootCommand(description: "Automates Roblox group join requests.");
        rootCommand.AddOption(VerifyOption);
        rootCommand.SetHandler(RunApplicationAsync);
        await rootCommand.InvokeAsync(args);
    }

    /// <summary>
    /// Runs the application with parsed command line arguments.
    /// </summary>
    /// <param name="invocationContext">Context for the command line options.</param>
    private static async Task RunApplicationAsync(InvocationContext invocationContext)
    {
        // Run the verify option and exit based on the result.
        if (invocationContext.ParseResult.GetValueForOption(VerifyOption))
        {
            var result = ConfigurationVerification.VerifyRules();
            await Logger.WaitForCompletionAsync();
            Environment.Exit(-(result.TotalRuleConfigurationErrors + result.TotalParseErrors + result.TotalTransformErrors));
        }
        
        // TODO: Run application.
    }
}