namespace BotSharp.Plugin.Sandbox;

public class SandboxPlugin : IBotSharpPlugin
{
    public string Id => "e0c83b27-3d7a-4c77-9e22-9a626e63ef2b";
    public string Name => "AIO Sandbox";
    public string Description => "Sidecar sandbox utilities exposed as agent tools.";
    public string? IconUrl => "https://avatars.githubusercontent.com/u/173528136?s=200&v=4";
    public string[] AgentIds => new[] { BuiltInAgentId.UtilityAssistant };

    public SettingsMeta Settings => new("Sandbox");
    public object GetNewSettingsInstance() => new SandboxSettings();

    public void RegisterDI(IServiceCollection services, IConfiguration config)
    {
        services.AddScoped(provider =>
        {
            var settingService = provider.GetRequiredService<ISettingService>();
            return settingService.Bind<SandboxSettings>("Sandbox");
        });

        services.AddHttpClient<SandboxApiClient>();
        services.AddScoped<IAgentUtilityHook, SandboxUtilityHook>();
    }
}
