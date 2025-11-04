using BotSharp.Abstraction.AIContext;
using BotSharp.Abstraction.Plugins;
using BotSharp.Plugin.AIMemory.Providers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BotSharp.Plugin.AIMemory;

/// <summary>
/// Plugin for AI Memory management.
/// Provides context providers for managing conversation memory and context.
/// </summary>
public class AIMemoryPlugin : IBotSharpPlugin
{
    public string Id => "8c6f9e42-5a3b-4d1e-9f2a-7b8c9d0e1f2a";
    public string Name => "AI Memory Plugin";
    public string Description => "Provides AI memory management similar to Microsoft Agent Framework's AIContextProvider pattern.";
    public string IconUrl => "https://cdn-icons-png.flaticon.com/512/2103/2103633.png";

    public void RegisterDI(IServiceCollection services, IConfiguration config)
    {
        // Register the conversation memory context provider
        services.AddScoped<IAIContextProvider, ConversationMemoryProvider>();
    }
}
