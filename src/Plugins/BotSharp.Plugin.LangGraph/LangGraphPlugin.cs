using BotSharp.Abstraction.Plugins;
using BotSharp.Abstraction.Settings;
using BotSharp.Plugin.LangGraph.Services;
using BotSharp.Plugin.LangGraph.Settings;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BotSharp.Plugin.LangGraph;

/// <summary>
/// LangGraph Plugin for BotSharp
/// Enables integration with LangGraph/LangServe for stateful graph-based agent orchestration
/// </summary>
public class LangGraphPlugin : IBotSharpPlugin
{
    public string Id => "f8e3c7d2-4a1b-4e5f-9d3c-8b2a1e6f4d9a";
    
    public string Name => "LangGraph";
    
    public string Description => "Integration with LangGraph/LangServe for stateful graph-based agent orchestration with state persistence and human-in-the-loop support";
    
    public string IconUrl => "https://avatars.githubusercontent.com/u/126733545";

    public void RegisterDI(IServiceCollection services, IConfiguration config)
    {
        // Register settings
        services.AddScoped(provider =>
        {
            var settingService = provider.GetRequiredService<ISettingService>();
            return settingService.Bind<LangGraphSettings>("LangGraph");
        });

        // Register HttpClient for LangServe communication
        services.AddHttpClient<LangServeClient>();

        // Register services
        services.AddScoped<LangServeClient>();
        services.AddScoped<LangGraphStateMapper>();
    }
}
