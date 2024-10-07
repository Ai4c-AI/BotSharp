using BotSharp.Plugin.OneAPI.Providers;

namespace BotSharp.Plugin.OneAPI;

public class OneAPIPlugin : IBotSharpPlugin
{
    public string Id => "67e3813c-1db9-b62c-a281-74c04f2c44d1";

    public string Name => "Compatible OpenAI AI";

    public string Description => "OneAPI Service including text generation and embedding services.";

    public SettingsMeta Settings => new SettingsMeta("OneAPI");

    public object GetNewSettingsInstance()
    {
        return new OneAPISettings();
    }


    public void RegisterDI(IServiceCollection services, IConfiguration config)
    {
        services.AddScoped(provider =>
        {
            var settingService = provider.GetRequiredService<ISettingService>();
            return settingService.Bind<OneAPISettings>("OneAPI");
        });
 
        services.AddScoped<IChatCompletion, DashScopeChatCompletionProvider>();
        services.AddScoped<IChatCompletion, MetaGLMChatCompletionProvider>();
        services.AddScoped<IChatCompletion, DoubaoChatCompletionProvider>();
    }

}
