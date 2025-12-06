using BotSharp.Abstraction.Plugins;
using BotSharp.Abstraction.Settings;
using BotSharp.Plugin.MicrosoftAgentFramework.Functions;
using BotSharp.Plugin.MicrosoftAgentFramework.Settings;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BotSharp.Plugin.MicrosoftAgentFramework;

/// <summary>
/// Microsoft Agent Framework (MAF) Integration Plugin
/// Provides execution plane capabilities for BotSharp's control plane
/// </summary>
public class MicrosoftAgentFrameworkPlugin : IBotSharpPlugin
{
    public string Id => "7d8e5f3a-9b2c-4e1f-a8d6-3c7b4e9f1a2d";
    
    public string Name => "Microsoft Agent Framework";
    
    public string Description => "Integration plugin that enables BotSharp to orchestrate Microsoft Agent Framework workflows as execution plane for enterprise business processes.";
    
    public string IconUrl => "https://raw.githubusercontent.com/microsoft/agents/main/docs/images/maf-icon.png";
    
    public SettingsMeta Settings => new SettingsMeta("MicrosoftAgentFramework");

    public void RegisterDI(IServiceCollection services, IConfiguration config)
    {
        // Register settings
        services.AddScoped(provider =>
        {
            var settingService = provider.GetRequiredService<ISettingService>();
            return settingService.Bind<MicrosoftAgentFrameworkSettings>("MicrosoftAgentFramework");
        });

        // Register MAF workflow functions
        services.AddScoped<IFunctionCallback, MafOrderWorkflowFunction>();
        services.AddScoped<IFunctionCallback, MafWorkflowExecutorFunction>();
    }

    public object GetNewSettingsInstance()
    {
        return new MicrosoftAgentFrameworkSettings();
    }
}
