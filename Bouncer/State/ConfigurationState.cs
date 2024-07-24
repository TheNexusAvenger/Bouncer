using System;
using System.IO;
using System.Text.Json;
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
    /// Event for the configuration changing.
    /// </summary>
    public static event Action<Configuration>? ConfigurationChanged;
    
    /// <summary>
    /// Loaded configuration instance of the state.
    /// </summary>
    public Configuration CurrentConfiguration { get; private set; } = null!;

    /// <summary>
    /// Last configuration as JSON.
    /// </summary>
    private string? _lastConfiguration = null;

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
        // Read the configuration.
        this.CurrentConfiguration = await Configuration.ReadConfigurationAsync();
        
        // Invoke the changed event if the contents changed.
        var newConfigurationJson = JsonSerializer.Serialize(this.CurrentConfiguration, ConfigurationJsonContext.Default.Configuration);
        if (this._lastConfiguration != null && this._lastConfiguration != newConfigurationJson)
        {
            ConfigurationChanged?.Invoke(this.CurrentConfiguration);
        }
        this._lastConfiguration = newConfigurationJson;
    }

    /// <summary>
    /// Tries to reload the configuration.
    /// No exception is thrown if it fails.
    /// </summary>
    public async Task TryReloadAsync()
    {
        try
        {
            await this.ReloadAsync();
        }
        catch (Exception)
        {
            // No action.
        }
    }
}