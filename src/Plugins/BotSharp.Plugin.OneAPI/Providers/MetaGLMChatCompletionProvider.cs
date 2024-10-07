namespace BotSharp.Plugin.OneAPI.Providers;

public class MetaGLMChatCompletionProvider : OneAPIChatCompletionProvider
{
    public override string Provider
    {
        get { return "chatglm"; }
    }

    public MetaGLMChatCompletionProvider(
        IServiceProvider services,
        OneAPISettings settings,
        ILoggerFactory? loggerFactory = null) 
        :base(services, settings, loggerFactory)        
        {
        
        }
}
