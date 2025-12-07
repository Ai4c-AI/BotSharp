namespace BotSharp.Plugin.AgentProtocol.Models;

/// <summary>
/// Represents an A2A agent card that describes the agent's capabilities
/// </summary>
public class A2AAgentCard
{
    /// <summary>
    /// Agent ID
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Agent name
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Agent description
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Agent capabilities
    /// </summary>
    public List<string> Capabilities { get; set; } = new();

    /// <summary>
    /// Agent endpoint URL
    /// </summary>
    public string Endpoint { get; set; } = string.Empty;

    /// <summary>
    /// Supported protocols
    /// </summary>
    public List<string> SupportedProtocols { get; set; } = new();

    /// <summary>
    /// Agent metadata
    /// </summary>
    public Dictionary<string, object> Metadata { get; set; } = new();

    /// <summary>
    /// Required parameters for invoking this agent
    /// </summary>
    public List<A2AParameter> RequiredParameters { get; set; } = new();

    /// <summary>
    /// Optional parameters for invoking this agent
    /// </summary>
    public List<A2AParameter> OptionalParameters { get; set; } = new();
}

/// <summary>
/// Represents a parameter for A2A agent invocation
/// </summary>
public class A2AParameter
{
    /// <summary>
    /// Parameter name
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Parameter description
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Parameter type
    /// </summary>
    public string Type { get; set; } = string.Empty;

    /// <summary>
    /// Is required
    /// </summary>
    public bool Required { get; set; }
}
