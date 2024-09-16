using System.Collections.Generic;
using System.Linq;
using Bouncer.Diagnostic;

namespace Bouncer.State.Loop;

public abstract class GenericLoopCollection<TLoop, TConfiguration, TConfigurationEntry> where TLoop : BaseConfigurableLoop<TConfigurationEntry> where TConfiguration : class
{
    /// <summary>
    /// Active loops in the collection.
    /// </summary>
    public readonly Dictionary<string, TLoop> ActiveLoops = new Dictionary<string, TLoop>();

    /// <summary>
    /// Creates a generic loop collection.
    /// </summary>
    public GenericLoopCollection()
    {
        // Connect the configuration changing.
        Configurations.GetConfigurationState<TConfiguration>().ConfigurationChanged += (_) =>
        {
            this.Refresh();
        };
        
        // Start the initial loops.
        this.Refresh();
    }
    
    /// <summary>
    /// Refreshes the loops based on the current configuration.
    /// </summary>
    /// <summary>Configuration to refresh with.</summary>
    public void Refresh(List<TConfigurationEntry> configuration)
    {
        // Add the new loops.
        foreach (var configurationEntry in configuration)
        {
            var keyId = this.GetLoopKeyId(configurationEntry);
            if (this.ActiveLoops.ContainsKey(keyId)) continue;
            this.ActiveLoops[keyId] = this.CreateLoop(configurationEntry);
        }
        
        // Update the loops.
        foreach (var configurationEntry in configuration)
        {
            var keyId = this.GetLoopKeyId(configurationEntry);
            var loop = this.ActiveLoops[keyId];
            loop.Stop();
            loop.SetConfiguration(configurationEntry);
            loop.OnConfigurationSet();
        }
        
        // Stop the loops that don't have configurations.
        foreach (var (keyId, loop) in this.ActiveLoops
                     .Where(loop => configuration.All(configurationEntry => loop.Key != this.GetLoopKeyId(configurationEntry))).ToList())
        {
            Logger.Debug($"Stopping loop {loop.Name}.");
            loop.Stop();
            this.ActiveLoops.Remove(keyId);
        }
    }
    
    /// <summary>
    /// Refreshes the group join request loops based on the current configuration.
    /// </summary>
    public void Refresh()
    {
        this.Refresh(this.GetConfigurationEntries(Configurations.GetConfiguration<TConfiguration>()));
    }

    /// <summary>
    /// Returns the list of configuration entries from the current configuration.
    /// </summary>
    /// <param name="configuration">Configuration to get the entries from.</param>
    /// <returns>List of configuration entries.</returns>
    public abstract List<TConfigurationEntry> GetConfigurationEntries(TConfiguration configuration);

    /// <summary>
    /// Returns the loop id for the configuration.
    /// </summary>
    /// <param name="configuration">Configuration to get the key from.</param>
    /// <returns>Key id for the configuration loop.</returns>
    public abstract string GetLoopKeyId(TConfigurationEntry configuration);

    /// <summary>
    /// Returns the loop instance for a configuration.
    /// </summary>
    /// <param name="configuration">Configuration to get the key from.</param>
    /// <returns>Loop for the configuration.</returns>
    public abstract TLoop CreateLoop(TConfigurationEntry configuration);
}