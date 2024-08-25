using System;
using System.Threading;
using System.Threading.Tasks;
using Bouncer.Diagnostic;

namespace Bouncer.State.Loop;

public abstract class BaseLoop
{
    /// <summary>
    /// Name of the loop.
    /// </summary>
    public string Name { get; private set; }

    /// <summary>
    /// Semaphore to ensure a loop only runs once at a time.
    /// </summary>
    private readonly SemaphoreSlim _semaphoreSlim = new SemaphoreSlim(1);

    /// <summary>
    /// Cancellation token for the active loop.
    /// </summary>
    private CancellationTokenSource? _loopCancellationToken;

    /// <summary>
    /// Whether the step of the loop is running.
    /// </summary>
    private bool _stepRunning = false;

    /// <summary>
    /// Creates a base loop.
    /// </summary>
    /// <param name="name">Name of the loop.</param>
    public BaseLoop(string name)
    {
        this.Name = name;
    }

    /// <summary>
    /// Stops the current loop.
    /// </summary>
    public void Stop()
    {
        this._loopCancellationToken?.Cancel();
    }

    /// <summary>
    /// Starts or restarts the loop.
    /// </summary>
    /// <param name="delaySeconds">Delay (in seconds) to perform the loop.</param>
    public void Start(ulong delaySeconds)
    {
        // Stop the current loop.
        this.Stop();
        
        // Run the loop in the background.
        var newLoopCancellationToken = new CancellationTokenSource();
        this._loopCancellationToken = newLoopCancellationToken;
        Task.Run(async () =>
        {
            while (!newLoopCancellationToken.IsCancellationRequested)
            {
                await this.TryRunAsync();
                await Task.Delay(TimeSpan.FromSeconds(delaySeconds), newLoopCancellationToken.Token);
            }
        }, newLoopCancellationToken.Token);
    }

    /// <summary>
    /// Attempts to run a step of the loop.
    /// </summary>
    private async Task TryRunAsync()
    {
        // Return if the loop is running.
        await this._semaphoreSlim.WaitAsync();
        if (this._stepRunning)
        {
            this._semaphoreSlim.Release();
            Logger.Warn($"Loop \"{this.Name}\" is actively running when a new step was requested. The step will be skipped.");
            return;
        }
        
        // Run the step.
        this._stepRunning = true;
        this._semaphoreSlim.Release();
        var _ = Task.Run(async () =>
        {
            Logger.Debug($"Running step in loop \"{this.Name}\"");
            try
            {
                await this.RunAsync();
                Logger.Debug($"Completed step in loop \"{this.Name}\"");
            }
            catch (Exception e)
            {
                Logger.Error($"Error occured in \"{this.Name}\" step.\n{e}");
            }
            await this._semaphoreSlim.WaitAsync();
            this._stepRunning = false;
            this._semaphoreSlim.Release();
        });
    }

    /// <summary>
    /// Runs a step in the loop.
    /// </summary>
    public abstract Task RunAsync();
}