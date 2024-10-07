namespace BotSharp.Plugin.OneAPI.Providers;

public class DoubaoChatCompletionProvider : OneAPIChatCompletionProvider
{
    public override string Provider
    {
        get { return "doubao"; }
    }

    public DoubaoChatCompletionProvider(
        IServiceProvider services,
        OneAPISettings settings,
        ILoggerFactory? loggerFactory = null) 
        :base(services, settings, loggerFactory)        
        {
        
        }
}
