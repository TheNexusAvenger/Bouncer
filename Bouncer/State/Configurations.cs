using System;
using System.Collections.Generic;
using System.Text.Json.Serialization.Metadata;

namespace Bouncer.State;

public static class Configurations
{
    /// <summary>
    /// Static configuration state instances.
    /// </summary>
    public static readonly Dictionary<Type, object> ConfigurationStates = new Dictionary<Type, object>();
    
    /// <summary>
    /// Prepares a configuration state.
    /// </summary>
    /// <param name="defaultConfiguration">Default configuration to store when no configuration file exists.</param>
    /// <param name="configurationJsonType">JSON type information for the configuration.</param>
    /// <typeparam name="T">Type of the configuration to read.</typeparam>
    public static void PrepareConfiguration<T>(T defaultConfiguration, JsonTypeInfo<T> configurationJsonType)
    {
        ConfigurationStates[typeof(T)] = new ConfigurationState<T>(defaultConfiguration, configurationJsonType);
    }
    
    /// <summary>
    /// Returns the current stored instance of the configuration state for a given type.
    /// </summary>
    /// <typeparam name="T">Type of the configuration.</typeparam>
    /// <returns>Configuration state instance that is currently stored.</returns>
    public static ConfigurationState<T> GetConfigurationState<T>()
    {
        // Throw an exception if the configuration was not prepared.
        if (!ConfigurationStates.TryGetValue(typeof(T), out var configurationState))
        {
            throw new ArgumentException($"Configuration of type {typeof(T).FullName} not prepared with PrepareConfiguration.");
        }
        
        // Return the configuration.
        return (configurationState as ConfigurationState<T>)!;
    }

    /// <summary>
    /// Returns the current stored instance of the configuration for a given type.
    /// </summary>
    /// <typeparam name="T">Type of the configuration.</typeparam>
    /// <returns>Configuration instance that is currently stored.</returns>
    public static T GetConfiguration<T>()
    {
        return GetConfigurationState<T>().CurrentConfiguration!;
    }
}