using BotSharp.Plugin.AgentFramework.Settings;
using Microsoft.Extensions.DependencyInjection;

namespace BotSharp.Plugin.AgentFramework.Services;

/// <summary>
/// Factory implementation for creating A2A clients
/// </summary>
public class A2AClientFactory : IA2AClientFactory
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly AgentFrameworkSettings _settings;
    private readonly IServiceProvider _serviceProvider;

    public A2AClientFactory(
        IHttpClientFactory httpClientFactory,
        AgentFrameworkSettings settings,
        IServiceProvider serviceProvider)
    {
        _httpClientFactory = httpClientFactory;
        _settings = settings;
        _serviceProvider = serviceProvider;
    }

    public IA2AClient CreateClient(string baseUrl)
    {
        var httpClient = _httpClientFactory.CreateClient("A2AClient");
        var logger = _serviceProvider.GetRequiredService<ILogger<A2AClient>>();
        
        return new A2AClient(httpClient, baseUrl, _settings, logger);
    }
}
