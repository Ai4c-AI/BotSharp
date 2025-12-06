using System.Text.Json.Serialization;

namespace BotSharp.Plugin.AgentFramework.Models;

/// <summary>
/// Agent Card metadata following A2A protocol specification.
/// Published at /.well-known/agent-card.json
/// </summary>
public class AgentCard
{
    /// <summary>
    /// Name of the agent
    /// </summary>
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Description of the agent's capabilities
    /// </summary>
    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Version of the agent
    /// </summary>
    [JsonPropertyName("version")]
    public string Version { get; set; } = string.Empty;

    /// <summary>
    /// Agent capabilities
    /// </summary>
    [JsonPropertyName("capabilities")]
    public List<string> Capabilities { get; set; } = new();

    /// <summary>
    /// Additional metadata
    /// </summary>
    [JsonPropertyName("metadata")]
    public Dictionary<string, object>? Metadata { get; set; }
}
