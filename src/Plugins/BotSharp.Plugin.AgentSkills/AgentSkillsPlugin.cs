using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using BotSharp.Abstraction.Plugins;

namespace BotSharp.Plugin.AgentSkills;

public class AgentSkillsPlugin : IBotSharpPlugin
{
    public string Id => "b20234a8-5c12-4e55-9592-2c7563121536";
    public string Name => "Agent Skills";
    public string Description => "Provides agent skills loading and management.";

    public void RegisterDI(IServiceCollection services, IConfiguration config)
    {
        var settings = new AgentSkillsSettings();
        config.Bind("AgentSkills", settings);
        services.AddSingleton(settings);

        services.AddSingleton<SkillLoaderService>();
    }
}
