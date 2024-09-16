namespace Bouncer.State.Loop;

public abstract class BaseConfigurableLoop<T> : BaseLoop
{
    /// <summary>
    /// Configuration of the loop.
    /// </summary>
    public T Configuration { get; private set; }
    
    /// <summary>
    /// Creates a base configurable loop loop.
    /// </summary>
    /// <param name="name">Name of the loop.</param>
    /// <param name="initialConfiguration">Initial configuration of the loop.</param>
    public BaseConfigurableLoop(string name, T initialConfiguration) : base(name)
    {
        this.Configuration = initialConfiguration;
    }

    /// <summary>
    /// Sets the configuration of the loop.
    /// It will stop the loop, but not restart it.
    /// </summary>
    /// <param name="configuration">Configuration of the loop.</param>
    public void SetConfiguration(T configuration)
    {
        this.Stop();
        this.Configuration = configuration;
    }

    /// <summary>
    /// Handles the configuration being set.
    /// This must handle starting the loop.
    /// </summary>
    public abstract void OnConfigurationSet();
}