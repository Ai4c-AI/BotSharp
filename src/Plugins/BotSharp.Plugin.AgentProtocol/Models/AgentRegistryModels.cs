namespace BotSharp.Plugin.AgentProtocol.Models;

/// <summary>
/// Query for agent registry
/// </summary>
public class AgentRegistryQuery
{
    /// <summary>
    /// Search query text (e.g., "who handles tax calculations?")
    /// </summary>
    public string Query { get; set; } = string.Empty;

    /// <summary>
    /// Required capabilities
    /// </summary>
    public List<string> RequiredCapabilities { get; set; } = new();

    /// <summary>
    /// Optional metadata filters
    /// </summary>
    public Dictionary<string, object> Filters { get; set; } = new();

    /// <summary>
    /// Maximum number of results
    /// </summary>
    public int MaxResults { get; set; } = 10;
}

/// <summary>
/// Agent registry query result
/// </summary>
public class AgentRegistryResult
{
    /// <summary>
    /// Matching agents
    /// </summary>
    public List<A2AAgentCard> Agents { get; set; } = new();

    /// <summary>
    /// Confidence scores for each agent (0-1)
    /// </summary>
    public Dictionary<string, double> ConfidenceScores { get; set; } = new();
}
