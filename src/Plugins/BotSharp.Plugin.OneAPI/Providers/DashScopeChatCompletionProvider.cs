namespace BotSharp.Plugin.OneAPI.Providers;

public class DashScopeChatCompletionProvider : OneAPIChatCompletionProvider
{
    public override string Provider
    {
        get { return "dashscope"; }
    }

    public DashScopeChatCompletionProvider(
        IServiceProvider services,
        OneAPISettings settings,
        ILoggerFactory? loggerFactory = null) 
        :base(services, settings, loggerFactory)        
        {
        
        }
}
