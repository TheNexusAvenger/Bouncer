using System;
using System.IO;
using System.Threading.Tasks;

namespace Bouncer.State;

public class ConfigurationState
{
    /// <summary>
    /// Static instance of the configuration state.
    /// </summary>
    public static readonly ConfigurationState Instance = new ConfigurationState();

    /// <summary>
    /// Loaded configuration instance.
    /// </summary>
    public static Configuration Configuration => Instance.CurrentConfiguration;
    
    /// <summary>
    /// Loaded configuration instance of the state.
    /// </summary>
    public Configuration CurrentConfiguration { get; private set; } = null!;

    /// <summary>
    /// Creates a configuration state.
    /// </summary>
    private ConfigurationState()
    {
        // Load the initial configuration.
        this.ReloadAsync().Wait();
        
        // Set up file change notifications.
        var configurationPath = GetConfigurationPath();
        var fileSystemWatcher = new FileSystemWatcher(Directory.GetParent(configurationPath)!.FullName);
        fileSystemWatcher.NotifyFilter = NotifyFilters.LastWrite;
        fileSystemWatcher.Changed += async (_, _) => await this.TryReloadAsync();
        fileSystemWatcher.EnableRaisingEvents = true;
        
        // Occasionally reload the file in a loop.
        // File change notifications don't seem to work in Docker with volumes.
        Task.Run(async () =>
        {
            while (true)
            {
                await Task.Delay(10000);
                await this.TryReloadAsync();
            }
        });
    }
    
    /// <summary>
    /// Returns the configuration file path.
    /// </summary>
    /// <returns>Path of the configuration file.</returns>
    public static string GetConfigurationPath()
    {
        return Environment.GetEnvironmentVariable("CONFIGURATION_FILE_LOCATION") ?? "configuration.json";
    }

    /// <summary>
    /// Reloads the configuration.
    /// </summary>
    public async Task ReloadAsync()
    {
        this.CurrentConfiguration = await Configuration.ReadConfigurationAsync();
    }

    /// <summary>
    /// Tries to reload the configuration.
    /// No exception is thrown if it fails.
    /// </summary>
    public async Task TryReloadAsync()
    {
        try
        {
            this.CurrentConfiguration = await Configuration.ReadConfigurationAsync();
        }
        catch (Exception)
        {
            // No action.
        }
    }
}