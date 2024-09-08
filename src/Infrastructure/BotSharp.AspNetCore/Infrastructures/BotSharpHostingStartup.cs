using Microsoft.AspNetCore.Hosting;
using BotSharp.AspNetCore.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;

//[assembly: HostingStartup(typeof(BotSharp.AspNetCore.Infrastructures.BotSharpHostingStartup))]
namespace BotSharp.AspNetCore.Infrastructures;

/// <summary>
/// https://docs.microsoft.com/zh-cn/aspnet/core/fundamentals/host/platform-specific-configuration?view=aspnetcore-5.0
/// </summary>
public class BotSharpHostingStartup : IHostingStartup
{
    public BotSharpHostingStartup()
    {

    }

    public void Configure(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            var config = services.BuildServiceProvider().GetService<IConfiguration>();
            services.AddBotSharpAspNetCore(config);
        });

        builder.Configure(app =>
        {
            app.UseBotSharpCore();
        });
    }
}