namespace BotSharp.Plugin.AgentProtocol.Services;

/// <summary>
/// Interface for agent registry that enables semantic routing
/// </summary>
public interface IAgentRegistry
{
    /// <summary>
    /// Query agents by semantic query (e.g., "who handles tax calculations?")
    /// </summary>
    /// <param name="query">Query request</param>
    /// <returns>Matching agents</returns>
    Task<AgentRegistryResult> QueryAgentsAsync(AgentRegistryQuery query);

    /// <summary>
    /// Register an agent in the registry
    /// </summary>
    /// <param name="card">Agent card</param>
    /// <returns>True if registered successfully</returns>
    Task<bool> RegisterAgentAsync(A2AAgentCard card);

    /// <summary>
    /// Unregister an agent from the registry
    /// </summary>
    /// <param name="agentId">Agent ID</param>
    /// <returns>True if unregistered successfully</returns>
    Task<bool> UnregisterAgentAsync(string agentId);

    /// <summary>
    /// Get all registered agents
    /// </summary>
    /// <returns>List of registered agents</returns>
    Task<List<A2AAgentCard>> GetAllAgentsAsync();

    /// <summary>
    /// Get agent by ID
    /// </summary>
    /// <param name="agentId">Agent ID</param>
    /// <returns>Agent card or null</returns>
    Task<A2AAgentCard?> GetAgentByIdAsync(string agentId);
}
