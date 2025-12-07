namespace BotSharp.Plugin.AgentProtocol.Services;

/// <summary>
/// Implementation of agent registry for semantic routing
/// </summary>
public class AgentRegistry : IAgentRegistry
{
    private readonly ILogger<AgentRegistry> _logger;
    private readonly HttpClient _httpClient;
    private readonly AgentProtocolSettings _settings;
    private readonly Dictionary<string, A2AAgentCard> _localRegistry = new();

    public AgentRegistry(
        ILogger<AgentRegistry> logger,
        IHttpClientFactory httpClientFactory,
        AgentProtocolSettings settings)
    {
        _logger = logger;
        _httpClient = httpClientFactory.CreateClient();
        _settings = settings;
    }

    public async Task<AgentRegistryResult> QueryAgentsAsync(AgentRegistryQuery query)
    {
        try
        {
            _logger.LogInformation($"Querying agents with query: {query.Query}");

            // If registry endpoint is configured, query the remote registry
            if (!string.IsNullOrEmpty(_settings.RegistryEndpoint))
            {
                return await QueryRemoteRegistryAsync(query);
            }

            // Otherwise, use local registry
            return QueryLocalRegistry(query);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error querying agents");
            return new AgentRegistryResult();
        }
    }

    public async Task<bool> RegisterAgentAsync(A2AAgentCard card)
    {
        try
        {
            _logger.LogInformation($"Registering agent: {card.Name} ({card.Id})");

            // Register in local cache
            _localRegistry[card.Id] = card;

            // If registry endpoint is configured, register remotely
            if (!string.IsNullOrEmpty(_settings.RegistryEndpoint))
            {
                var endpoint = _settings.RegistryEndpoint.TrimEnd('/') + "/register";
                var json = JsonSerializer.Serialize(card);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                
                var response = await _httpClient.PostAsync(endpoint, content);
                return response.IsSuccessStatusCode;
            }

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error registering agent {card.Id}");
            return false;
        }
    }

    public async Task<bool> UnregisterAgentAsync(string agentId)
    {
        try
        {
            _logger.LogInformation($"Unregistering agent: {agentId}");

            // Remove from local cache
            _localRegistry.Remove(agentId);

            // If registry endpoint is configured, unregister remotely
            if (!string.IsNullOrEmpty(_settings.RegistryEndpoint))
            {
                var endpoint = _settings.RegistryEndpoint.TrimEnd('/') + $"/unregister/{agentId}";
                var response = await _httpClient.DeleteAsync(endpoint);
                return response.IsSuccessStatusCode;
            }

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error unregistering agent {agentId}");
            return false;
        }
    }

    public async Task<List<A2AAgentCard>> GetAllAgentsAsync()
    {
        try
        {
            // If registry endpoint is configured, fetch from remote
            if (!string.IsNullOrEmpty(_settings.RegistryEndpoint))
            {
                var endpoint = _settings.RegistryEndpoint.TrimEnd('/') + "/agents";
                var response = await _httpClient.GetAsync(endpoint);
                
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var agents = JsonSerializer.Deserialize<List<A2AAgentCard>>(json, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
                    return agents ?? new List<A2AAgentCard>();
                }
            }

            // Return local registry
            return _localRegistry.Values.ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all agents");
            return new List<A2AAgentCard>();
        }
    }

    public async Task<A2AAgentCard?> GetAgentByIdAsync(string agentId)
    {
        try
        {
            // Check local registry first
            if (_localRegistry.TryGetValue(agentId, out var card))
            {
                return card;
            }

            // If registry endpoint is configured, query remote
            if (!string.IsNullOrEmpty(_settings.RegistryEndpoint))
            {
                var endpoint = _settings.RegistryEndpoint.TrimEnd('/') + $"/agents/{agentId}";
                var response = await _httpClient.GetAsync(endpoint);
                
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var agent = JsonSerializer.Deserialize<A2AAgentCard>(json, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
                    
                    // Cache locally
                    if (agent != null)
                    {
                        _localRegistry[agentId] = agent;
                    }
                    
                    return agent;
                }
            }

            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error getting agent {agentId}");
            return null;
        }
    }

    private async Task<AgentRegistryResult> QueryRemoteRegistryAsync(AgentRegistryQuery query)
    {
        var endpoint = _settings.RegistryEndpoint!.TrimEnd('/') + "/query";
        var json = JsonSerializer.Serialize(query);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        
        var response = await _httpClient.PostAsync(endpoint, content);
        
        if (!response.IsSuccessStatusCode)
        {
            _logger.LogWarning($"Failed to query remote registry: {response.StatusCode}");
            return new AgentRegistryResult();
        }

        var resultJson = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<AgentRegistryResult>(resultJson, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        return result ?? new AgentRegistryResult();
    }

    private AgentRegistryResult QueryLocalRegistry(AgentRegistryQuery query)
    {
        var agents = _localRegistry.Values.ToList();
        var result = new AgentRegistryResult();

        // Simple capability matching
        if (query.RequiredCapabilities.Any())
        {
            agents = agents.Where(a => 
                query.RequiredCapabilities.All(cap => 
                    a.Capabilities.Any(c => c.Contains(cap, StringComparison.OrdinalIgnoreCase))
                )
            ).ToList();
        }

        // Simple semantic search on description and name
        if (!string.IsNullOrWhiteSpace(query.Query))
        {
            var queryLower = query.Query.ToLower();
            agents = agents.Where(a =>
                a.Name.ToLower().Contains(queryLower) ||
                a.Description.ToLower().Contains(queryLower) ||
                a.Capabilities.Any(c => c.ToLower().Contains(queryLower))
            ).ToList();
        }

        // Apply max results
        agents = agents.Take(query.MaxResults).ToList();

        result.Agents = agents;
        
        // Simple confidence scoring based on query match
        foreach (var agent in agents)
        {
            result.ConfidenceScores[agent.Id] = CalculateConfidence(agent, query);
        }

        return result;
    }

    private double CalculateConfidence(A2AAgentCard agent, AgentRegistryQuery query)
    {
        double score = 0.5; // Base score

        if (string.IsNullOrWhiteSpace(query.Query))
        {
            return score;
        }

        var queryLower = query.Query.ToLower();
        
        // Increase score if name matches
        if (agent.Name.ToLower().Contains(queryLower))
        {
            score += 0.3;
        }

        // Increase score if description matches
        if (agent.Description.ToLower().Contains(queryLower))
        {
            score += 0.2;
        }

        // Increase score if capabilities match
        if (agent.Capabilities.Any(c => c.ToLower().Contains(queryLower)))
        {
            score += 0.2;
        }

        return Math.Min(score, 1.0);
    }
}
