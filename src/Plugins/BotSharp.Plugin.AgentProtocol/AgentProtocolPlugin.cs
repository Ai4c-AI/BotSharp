using BotSharp.Abstraction.Agents;
using BotSharp.Abstraction.Settings;
using Microsoft.Extensions.Configuration;
using BotSharp.Plugin.AgentProtocol.Hooks;

namespace BotSharp.Plugin.AgentProtocol;

/// <summary>
/// Plugin for Agent-to-Agent (A2A) protocol communication
/// Enables BotSharp to act as an A2A client and communicate with remote agents
/// </summary>
public class AgentProtocolPlugin : IBotSharpPlugin
{
    public string Id => "d4c7e8f2-5a3b-4c1d-9e2f-8a7b6c5d4e3f";
    
    public string Name => "Agent Protocol (A2A)";
    
    public string Description => "Enables Agent-to-Agent communication using A2A protocol. " +
        "Supports semantic routing through agent registry, bidirectional communication, " +
        "and async callbacks for long-running tasks.";
    
    public string IconUrl => "https://cdn-icons-png.flaticon.com/512/1087/1087927.png";

    public void RegisterDI(IServiceCollection services, IConfiguration config)
    {
        // Register settings
        services.AddScoped(provider =>
        {
            var settingService = provider.GetRequiredService<ISettingService>();
            return settingService.Bind<AgentProtocolSettings>("AgentProtocol");
        });

        // Register HTTP client factory
        services.AddHttpClient();

        // Register services
        services.AddScoped<IA2ACardResolver, A2ACardResolver>();
        services.AddScoped<IA2AClient, A2AClient>();
        services.AddScoped<IAgentRegistry, AgentRegistry>();

        // Register hooks
        services.AddScoped<IAgentHook, A2AAgentHook>();
    }
}
