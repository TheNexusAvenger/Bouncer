﻿using System;
using System.Threading.Tasks;
using Bouncer.Diagnostic;
using Bouncer.Web.Server.Model;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
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
    /// Instance of the health check state.
    /// </summary>
    private readonly HealthCheckState _healthCheckState = new HealthCheckState();

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
        builder.Logging.AddProvider(Logger.NexusLogger);
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
        await app.RunAsync($"http://localhost:{Port}");
    }
    
    /// <summary>
    /// Starts the web server.
    /// </summary>
    public async Task StartAsync()
    {
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
                var healthCheckResult = _healthCheckState.GetHealthCheckResult();
                var statusCode = (healthCheckResult.Status == HealthCheckResultStatus.Up ? 200 : 503);
                return Results.Json(healthCheckResult, statusCode: statusCode);
            });
        });
    }
}