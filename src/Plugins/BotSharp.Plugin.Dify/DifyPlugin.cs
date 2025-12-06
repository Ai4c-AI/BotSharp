using BotSharp.Abstraction.Plugins;
using BotSharp.Abstraction.Settings;
using BotSharp.Plugin.Dify.Functions;
using BotSharp.Plugin.Dify.HostedServices;
using BotSharp.Plugin.Dify.Services;
using BotSharp.Plugin.Dify.Settings;
using Microsoft.Extensions.Configuration;

namespace BotSharp.Plugin.Dify;

/// <summary>
/// Plugin for integrating BotSharp with Dify workflows
/// This plugin enables BotSharp to act as a router/orchestrator
/// while delegating workflow execution to Dify for flexible content generation
/// </summary>
public class DifyPlugin : IBotSharpPlugin
{
    public string Id => "f8a5c2d1-3e4b-4c5d-8f9a-1b2c3d4e5f6a";
    public string Name => "Dify Workflow Integration";
    public string Description => "Enables BotSharp to execute Dify workflows with async task polling support";
    public string IconUrl => "https://docs.dify.ai/logo.png";

    public SettingsMeta Settings => new SettingsMeta("Dify");

    public object GetNewSettingsInstance() => new DifySettings();

    public void RegisterDI(IServiceCollection services, IConfiguration config)
    {
        // Register settings
        services.AddScoped(provider =>
        {
            var settingService = provider.GetRequiredService<ISettingService>();
            return settingService.Bind<DifySettings>("Dify");
        });

        // Register services
        services.AddScoped<DifyWorkflowService>();
        services.AddSingleton<DifyTaskStorageService>();

        // Register function callback
        services.AddScoped<IFunctionCallback, CallDifyWorkflowFn>();

        // Register background service for polling
        services.AddHostedService<DifyTaskPollingService>();
    }
}
