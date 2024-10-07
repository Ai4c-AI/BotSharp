using BotSharp.Abstraction.MLTasks.Settings;
using OpenAI;
using OpenAI.Chat;

namespace BotSharp.Plugin.OneAPI.Providers;

internal class ProviderHelper
{
    public static ChatClient GetClient(LlmModelSetting settings)
    {
        var client = new ChatClient(settings.Name,  new System.ClientModel.ApiKeyCredential(settings.ApiKey), new OpenAIClientOptions() { Endpoint = new Uri(settings.Endpoint)});
        
        return client;
    }
}
