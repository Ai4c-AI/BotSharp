namespace BotSharp.Plugin.AgentProtocol.Services;

/// <summary>
/// Implementation of A2A card resolver
/// </summary>
public class A2ACardResolver : IA2ACardResolver
{
    private readonly ILogger<A2ACardResolver> _logger;
    private readonly HttpClient _httpClient;

    public A2ACardResolver(
        ILogger<A2ACardResolver> logger,
        IHttpClientFactory httpClientFactory)
    {
        _logger = logger;
        _httpClient = httpClientFactory.CreateClient();
    }

    public async Task<A2AAgentCard> ResolveCardAsync(string endpoint)
    {
        try
        {
            _logger.LogInformation($"Resolving A2A agent card from endpoint: {endpoint}");

            // Try to fetch the agent card from the endpoint
            var cardEndpoint = endpoint.TrimEnd('/') + "/a2a/card";
            var response = await _httpClient.GetAsync(cardEndpoint);
            
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning($"Failed to fetch agent card from {cardEndpoint}: {response.StatusCode}");
                throw new Exception($"Failed to fetch agent card: {response.StatusCode}");
            }

            var cardJson = await response.Content.ReadAsStringAsync();
            return ParseCard(cardJson);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error resolving agent card from {endpoint}");
            throw;
        }
    }

    public A2AAgentCard ParseCard(string cardJson)
    {
        try
        {
            var card = JsonSerializer.Deserialize<A2AAgentCard>(cardJson, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (card == null)
            {
                throw new Exception("Failed to deserialize agent card");
            }

            return card;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error parsing agent card JSON");
            throw;
        }
    }

    public bool ValidateCard(A2AAgentCard card)
    {
        if (string.IsNullOrWhiteSpace(card.Id))
        {
            _logger.LogWarning("Agent card validation failed: missing ID");
            return false;
        }

        if (string.IsNullOrWhiteSpace(card.Name))
        {
            _logger.LogWarning("Agent card validation failed: missing Name");
            return false;
        }

        if (string.IsNullOrWhiteSpace(card.Endpoint))
        {
            _logger.LogWarning("Agent card validation failed: missing Endpoint");
            return false;
        }

        return true;
    }
}
