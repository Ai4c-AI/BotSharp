using BotSharp.Abstraction.Plugins;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace BotSharp.Plugin.TelegramBots
{
    public class TelegramBotsPlugin : IBotSharpModule
    {
        public string Id => "6cc8c2f0-0958-4ace-adec-1df609ec9e00";
        public string Name => "Telegram Bots";
        public string Description => "Bots are small applications that run entirely within the Telegram app.";

        public void ConfigureServices(IServiceCollection services, IConfiguration config)
        {
            
        }
    }
}
