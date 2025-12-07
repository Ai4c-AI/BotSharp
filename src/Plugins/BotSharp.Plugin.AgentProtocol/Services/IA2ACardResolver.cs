namespace BotSharp.Plugin.AgentProtocol.Services;

/// <summary>
/// Interface for A2A card resolver that parses and understands agent capabilities
/// </summary>
public interface IA2ACardResolver
{
    /// <summary>
    /// Resolve agent card from endpoint
    /// </summary>
    /// <param name="endpoint">Agent endpoint URL</param>
    /// <returns>Resolved agent card</returns>
    Task<A2AAgentCard> ResolveCardAsync(string endpoint);

    /// <summary>
    /// Parse agent card from JSON
    /// </summary>
    /// <param name="cardJson">Agent card JSON</param>
    /// <returns>Parsed agent card</returns>
    A2AAgentCard ParseCard(string cardJson);

    /// <summary>
    /// Validate agent card
    /// </summary>
    /// <param name="card">Agent card to validate</param>
    /// <returns>True if valid</returns>
    bool ValidateCard(A2AAgentCard card);
}
