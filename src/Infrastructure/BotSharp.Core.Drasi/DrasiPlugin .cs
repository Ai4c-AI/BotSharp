using BotSharp.Abstraction.Plugins;
using BotSharp.Core.Drasi.Services;
using BotSharp.Core.Drasi.Settings;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BotSharp.Core.Drasi;

public class DrasiPlugin : IBotSharpPlugin
{
    public string Id => "6e5246f6-8c46-4c65-8ee4-032324c26b81"; // 唯一 GUID
    public string Name => "Drasi Reactive Connector";
    public string Description => "Enables real-time data reaction capabilities via Project Drasi.";

    // 插件加载时注册服务
    public void RegisterDI(IServiceCollection services, IConfiguration config)
    {
        // 注册配置对象
        services.Configure<DrasiPluginSettings>(config.GetSection("Drasi"));

        // 注册 SignalR 监听服务为 HostedService (后台长期运行)
        services.AddHostedService<DrasiSignalRService>();

        // 注册事件分发逻辑
        services.AddScoped<IDrasiEventDispatcher, DrasiEventDispatcher>();

        // 注册管理工具 (Function Calling)
        // BotSharp 会自动扫描实现了 IToolHandler 的类，但在某些版本需手动注册
    }
}
