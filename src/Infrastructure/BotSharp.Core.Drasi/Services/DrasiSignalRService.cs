using BotSharp.Core.Drasi.Models;
using BotSharp.Core.Drasi.Settings;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace BotSharp.Core.Drasi.Services;

public class DrasiSignalRService : BackgroundService
{
    private readonly ILogger<DrasiSignalRService> _logger;
    private readonly DrasiPluginSettings _settings;
    private readonly IServiceProvider _serviceProvider;
    private HubConnection _hubConnection;

    public DrasiSignalRService(
        ILogger<DrasiSignalRService> logger,
        IOptions<DrasiPluginSettings> settings,
        IServiceProvider serviceProvider)
    {
        _logger = logger;
        _settings = settings.Value;
        _serviceProvider = serviceProvider;
    }

    public override async Task StartAsync(CancellationToken cancellationToken)
    {
        // 构建连接，启用自动重连机制
        _hubConnection = new HubConnectionBuilder()
           .WithUrl(_settings.SignalREndpoint) // 例如: http://drasi-reaction-gateway:8080/hub
           .WithAutomaticReconnect(new[] { TimeSpan.Zero, TimeSpan.FromSeconds(2), TimeSpan.FromSeconds(10), TimeSpan.FromSeconds(30) })
           .Build();

        // 注册断线重连日志
        _hubConnection.Reconnecting += error =>
        {
            _logger.LogWarning($"Drasi Connection lost. Reconnecting... Error: {error?.Message}");
            return Task.CompletedTask;
        };

        // 监听 "change" 事件 (Drasi 标准 SignalR 协议)
        _hubConnection.On<JsonElement>("change", async (payload) =>
        {
            await ProcessChangeAsync(payload);
        });

        try
        {
            await _hubConnection.StartAsync(cancellationToken);
            _logger.LogInformation($"Drasi SignalR Connected to {_settings.SignalREndpoint}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to connect to Drasi SignalR Hub.");
            // 在生产环境中，这里应该有更复杂的重试逻辑或断路器
        }

        await base.StartAsync(cancellationToken);
    }

    private async Task ProcessChangeAsync(JsonElement rawPayload)
    {
        try
        {
            // 1. 反序列化
            var message = JsonSerializer.Deserialize<DrasiChangeMessage>(rawPayload.GetRawText());

            if (message?.Payload?.Source == null) return;

            _logger.LogDebug($"Received Drasi Event: {message.Operation} from {message.Payload.Source.QueryId}");

            // 2. 创建 Scope 以解析 Scoped Services
            using (var scope = _serviceProvider.CreateScope())
            {
                var dispatcher = scope.ServiceProvider.GetRequiredService<IDrasiEventDispatcher>();
                await dispatcher.DispatchAsync(message);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing Drasi payload.");
        }
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // 保持服务运行，直到取消
        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(1000, stoppingToken);
        }
    }
}