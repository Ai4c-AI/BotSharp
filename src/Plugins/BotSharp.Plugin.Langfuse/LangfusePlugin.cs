namespace BotSharp.Plugin.Langfuse;

public class LangfusePlugin : IBotSharpPlugin
{
    public string Id => "1a2b3c4d-5e6f-7g8h-9i0j-1k2l3m4n5o6p";
    public string Name => "Langfuse Observability";
    public string Description => "Integration with Langfuse for comprehensive observability of multi-agent conversations, LLM calls, and function executions.";
    public string IconUrl => "https://langfuse.com/images/favicon.png";

    public SettingsMeta Settings => new SettingsMeta("Langfuse");

    public object GetNewSettingsInstance()
    {
        return new LangfuseSettings();
    }

    public void RegisterDI(IServiceCollection services, IConfiguration config)
    {
        // Register settings
        services.AddScoped(provider =>
        {
            var settingService = provider.GetRequiredService<ISettingService>();
            return settingService.Bind<LangfuseSettings>("Langfuse");
        });

        // Register Langfuse client using the package's DI extension
        try
        {
            services.AddLangfuse(config);
        }
        catch
        {
            // Langfuse configuration may not be present, that's ok
        }

        // Register Langfuse service as singleton to maintain trace/span state
        services.AddSingleton<LangfuseService>();

        // Register hooks
        services.AddScoped<IContentGeneratingHook, LangfuseContentGeneratingHook>();
        services.AddScoped<IConversationHook, LangfuseConversationHook>();
        services.AddScoped<IConversationHook, LangfuseRoutingHook>();
        services.AddScoped<IAgentHook, LangfuseAgentHook>();
    }
}
