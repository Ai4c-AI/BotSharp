using BotSharp.Abstraction.Agents;
using BotSharp.Abstraction.Conversations;
using BotSharp.Abstraction.Plugins;
using BotSharp.Abstraction.Settings;
using BotSharp.Plugin.AgentFramework.Hooks;
using BotSharp.Plugin.AgentFramework.Services;
using BotSharp.Plugin.AgentFramework.Settings;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace BotSharp.Plugin.AgentFramework;

/// <summary>
/// Plugin for integrating BotSharp with Microsoft Agent Framework (MAF) via A2A protocol
/// </summary>
public class AgentFrameworkPlugin : IBotSharpPlugin
{
    public string Id => "3F8E4B9C-7D2A-4E1F-9B3C-5A6D8E2F1C4B";
    
    public string Name => "Microsoft Agent Framework";
    
    public string Description => "Integrates BotSharp with Microsoft Agent Framework (MAF) using Agent2Agent (A2A) protocol based on JSON-RPC 2.0";
    
    public string? IconUrl => "https://learn.microsoft.com/en-us/media/logos/logo-ms-social.png";

    public void RegisterDI(IServiceCollection services, IConfiguration config)
    {
        // Register settings
        services.AddScoped(provider =>
        {
            var settingService = provider.GetRequiredService<ISettingService>();
            return settingService.Bind<AgentFrameworkSettings>("AgentFramework");
        });

        // Register HTTP client for A2A communication with timeout configuration
        services.AddHttpClient("A2AClient", client =>
        {
            client.DefaultRequestHeaders.Add("User-Agent", "BotSharp-A2A-Client/1.0");
            client.DefaultRequestHeaders.Add("Accept", "application/json");
        })
        .ConfigureHttpClient((sp, client) =>
        {
            var settings = sp.GetRequiredService<AgentFrameworkSettings>();
            client.Timeout = TimeSpan.FromSeconds(settings.TimeoutSeconds);
        });

        // Register memory cache for agent card caching (use TryAdd to avoid conflicts)
        services.TryAddSingleton<IMemoryCache, MemoryCache>();

        // Register A2A services
        services.AddSingleton<IA2AClientFactory, A2AClientFactory>();
        services.AddSingleton<A2ACardResolver>();
        services.AddScoped<IA2AAgentService, A2AAgentService>();

        // Register hooks
        services.AddScoped<IAgentHook, AgentFrameworkHook>();
        services.AddScoped<IConversationHook, AgentFrameworkConversationHook>();
    }
}
