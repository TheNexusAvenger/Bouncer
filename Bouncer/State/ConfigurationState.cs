using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using System.Threading.Tasks;
using Bouncer.Diagnostic;

namespace Bouncer.State;

public class ConfigurationState<T>
{
    /// <summary>
    /// Event for the configuration changing.
    /// </summary>
    public event Action<T>? ConfigurationChanged;
    
    /// <summary>
    /// Loaded configuration instance of the state.
    /// </summary>
    public T? CurrentConfiguration { get; private set; }

    /// <summary>
    /// JSON type information for the configuration.
    /// </summary>
    private readonly JsonTypeInfo<T> _configurationJsonType;

    /// <summary>
    /// Default configuration to store when no configuration file exists.
    /// </summary>
    private readonly T _defaultConfiguration;

    /// <summary>
    /// Last configuration as JSON.
    /// </summary>
    private string? _lastConfiguration = null;

    /// <summary>
    /// Creates a configuration state.
    /// </summary>
    /// <param name="defaultConfiguration">Default configuration to store when no configuration file exists.</param>
    /// <param name="configurationJsonType">JSON type information for the configuration.</param>
    public ConfigurationState(T defaultConfiguration, JsonTypeInfo<T> configurationJsonType)
    {
        // Load the initial configuration.
        this._defaultConfiguration = defaultConfiguration;
        this._configurationJsonType = configurationJsonType;
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
        // Prepare the configuration if it doesn't exist.
        var path = GetConfigurationPath();
        if (!File.Exists(path))
        {
            await File.WriteAllTextAsync(path, JsonSerializer.Serialize(this._defaultConfiguration, this._configurationJsonType));
        }
        
        // Read the configuration.
        Logger.Trace("Attempting to read new configuration.");
        var configurationContents = await File.ReadAllTextAsync(path);
        this.CurrentConfiguration = JsonSerializer.Deserialize<T>(configurationContents, this._configurationJsonType)!;
        Logger.Trace("Read new configuration.");
        
        // Invoke the changed event if the contents changed.
        if (this._lastConfiguration != null && this._lastConfiguration != configurationContents)
        {
            Logger.Debug("Configuration updated.");
            ConfigurationChanged?.Invoke(this.CurrentConfiguration);
        }
        this._lastConfiguration = configurationContents;
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
        catch (Exception e)
        {
            Logger.Trace($"An error occured trying to update the configuration. This might be due to a text editor writing the file.\n{e}");
        }
    }
}