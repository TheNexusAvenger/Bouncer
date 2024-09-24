using System;
using System.Threading.Tasks;
using Bouncer.Diagnostic;
using Bouncer.State.Loop;
using Bouncer.Web.Server.Model;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Bouncer.Web.Server;

public class WebServer
{
    /// <summary>
    /// Port used for the web server.
    /// </summary>
    public ushort Port { get; set; } = 8000;

    /// <summary>
    /// If true, logging will be added for ASP.NET.
    /// </summary>
    public bool AddAspNetLogging { get; set; } = false;

    /// <summary>
    /// Starts the web server.
    /// </summary>
    /// <param name="prepareApplication">Action to prepare the application (such as JSON serializer setup).</param>
    /// <param name="prepareApi">Action to add the API.</param>
    public async Task StartAsync(Action<WebApplicationBuilder> prepareApplication, Action<WebApplication> prepareApi)
    {
        // Create the app builder with custom logging.
        var builder = WebApplication.CreateSlimBuilder();
        builder.Logging.ClearProviders();
        if (this.AddAspNetLogging)
        {
            builder.Logging.AddProvider(Logger.NexusLogger);
        }
        builder.WebHost.UseKestrel(options => options.AddServerHeader = false);
        prepareApplication.Invoke(builder);
        
        // Set up custom exception handling.
        var app = builder.Build();
        app.UseExceptionHandler(exceptionHandlerApp =>
        {
            exceptionHandlerApp.Run(context =>
            {
                var exceptionHandlerPathFeature = context.Features.Get<IExceptionHandlerPathFeature>();
                if (exceptionHandlerPathFeature != null)
                {
                    Logger.Error($"An exception occurred processing {context.Request.Method} {context.Request.Path}\n{exceptionHandlerPathFeature.Error}");
                }
                return Task.CompletedTask;
            });
        });
        
        // Build the API.
        prepareApi.Invoke(app);
        
        // Run the server.
        Logger.Info($"Serving on port {Port}.");
        await app.RunAsync($"http://localhost:{Port}");
    }
    
    /// <summary>
    /// Starts the web server.
    /// </summary>
    /// <param name="groupJoinRequestLoopCollection">Group join request loop collection to include in the health check.</param>
    public async Task StartAsync(GroupJoinRequestLoopCollection groupJoinRequestLoopCollection)
    {
        var healthCheckState = new HealthCheckState()
        {
            GroupJoinRequestLoopCollection = groupJoinRequestLoopCollection,
        };
        healthCheckState.ConnectConfigurationChanges();
        await this.StartAsync((builder) =>
        {
            // Add the JSON serializers.
            builder.Services.ConfigureHttpJsonOptions(options => {
                options.SerializerOptions.TypeInfoResolverChain.Insert(0, HealthCheckResultJsonContext.Default);
            });
        }, (app) =>
        {
            // Build the API.
            app.MapGet("/health", () =>
            {
                var healthCheckResult = healthCheckState.GetHealthCheckResult();
                var statusCode = (healthCheckResult.Status == HealthCheckResultStatus.Up ? 200 : 503);
                return Results.Json(healthCheckResult, statusCode: statusCode);
            });
        });
    }
}