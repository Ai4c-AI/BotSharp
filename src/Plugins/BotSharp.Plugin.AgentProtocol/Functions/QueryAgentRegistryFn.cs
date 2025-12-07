namespace BotSharp.Plugin.AgentProtocol.Functions;

/// <summary>
/// Function to query agent registry for semantic routing
/// </summary>
public class QueryAgentRegistryFn : IFunctionCallback
{
    public string Name => "a2a-query_agent_registry";
    public string Indication => "Searching for capable agents...";

    private readonly ILogger<QueryAgentRegistryFn> _logger;
    private readonly IAgentRegistry _registry;

    public QueryAgentRegistryFn(
        ILogger<QueryAgentRegistryFn> logger,
        IAgentRegistry registry)
    {
        _logger = logger;
        _registry = registry;
    }

    public async Task<bool> Execute(RoleDialogModel message)
    {
        try
        {
            var args = JsonSerializer.Deserialize<AgentRegistryQuery>(message.FunctionArgs ?? "{}");
            
            if (args == null || string.IsNullOrEmpty(args.Query))
            {
                message.Content = "Query parameter is required";
                return false;
            }

            _logger.LogInformation($"Querying agent registry: {args.Query}");

            var result = await _registry.QueryAgentsAsync(args);

            if (result.Agents.Any())
            {
                var agentList = result.Agents.Select(a => new
                {
                    id = a.Id,
                    name = a.Name,
                    description = a.Description,
                    capabilities = a.Capabilities,
                    confidence = result.ConfidenceScores.GetValueOrDefault(a.Id, 0)
                }).ToList();

                message.Content = JsonSerializer.Serialize(new
                {
                    success = true,
                    count = agentList.Count,
                    agents = agentList
                });

                _logger.LogInformation($"Found {agentList.Count} matching agents");
            }
            else
            {
                message.Content = JsonSerializer.Serialize(new
                {
                    success = true,
                    count = 0,
                    message = "No matching agents found"
                });

                _logger.LogInformation("No matching agents found");
            }

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error querying agent registry");
            message.Content = $"Error querying agent registry: {ex.Message}";
            return false;
        }
    }
}
