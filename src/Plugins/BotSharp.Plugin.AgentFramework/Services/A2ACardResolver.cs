using BotSharp.Plugin.AgentFramework.Models;
using BotSharp.Plugin.AgentFramework.Settings;
using Microsoft.Extensions.Caching.Memory;

namespace BotSharp.Plugin.AgentFramework.Services;

/// <summary>
/// Service for resolving and caching agent cards from remote MAF services
/// </summary>
public class A2ACardResolver
{
    private readonly IA2AClientFactory _clientFactory;
    private readonly IMemoryCache _cache;
    private readonly AgentFrameworkSettings _settings;
    private readonly ILogger<A2ACardResolver> _logger;

    public A2ACardResolver(
        IA2AClientFactory clientFactory,
        IMemoryCache cache,
        AgentFrameworkSettings settings,
        ILogger<A2ACardResolver> logger)
    {
        _clientFactory = clientFactory;
        _cache = cache;
        _settings = settings;
        _logger = logger;
    }

    /// <summary>
    /// Gets the agent card from the specified endpoint, using cache when available
    /// </summary>
    /// <param name="baseUrl">Base URL of the MAF service</param>
    /// <param name="forceRefresh">Force refresh even if cached</param>
    /// <returns>Agent card</returns>
    public async Task<AgentCard> GetCardAsync(string baseUrl, bool forceRefresh = false)
    {
        var cacheKey = $"AgentCard_{baseUrl}";

        if (!forceRefresh && _cache.TryGetValue<AgentCard>(cacheKey, out var cachedCard) && cachedCard != null)
        {
            _logger.LogDebug("Retrieved agent card from cache for {BaseUrl}", baseUrl);
            return cachedCard;
        }

        _logger.LogInformation("Fetching agent card from remote service: {BaseUrl}", baseUrl);
        
        var client = _clientFactory.CreateClient(baseUrl);
        var card = await client.GetAgentCardAsync();

        var cacheOptions = new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(_settings.AgentCardCacheDurationMinutes)
        };

        _cache.Set(cacheKey, card, cacheOptions);
        _logger.LogInformation("Cached agent card for {BaseUrl} with expiration of {Minutes} minutes", 
            baseUrl, _settings.AgentCardCacheDurationMinutes);

        return card;
    }

    /// <summary>
    /// Invalidates the cache for a specific endpoint
    /// </summary>
    /// <param name="baseUrl">Base URL to invalidate</param>
    public void InvalidateCache(string baseUrl)
    {
        var cacheKey = $"AgentCard_{baseUrl}";
        _cache.Remove(cacheKey);
        _logger.LogInformation("Invalidated agent card cache for {BaseUrl}", baseUrl);
    }
}
