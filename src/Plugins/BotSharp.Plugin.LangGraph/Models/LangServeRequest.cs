using System.Text.Json.Serialization;

namespace BotSharp.Plugin.LangGraph.Models;

/// <summary>
/// Request model for LangServe API
/// </summary>
public class LangServeRequest
{
    [JsonPropertyName("input")]
    public LangServeInput Input { get; set; } = new();

    [JsonPropertyName("config")]
    public LangServeConfig Config { get; set; } = new();
}

/// <summary>
/// Input section of the request
/// </summary>
public class LangServeInput
{
    [JsonPropertyName("messages")]
    public List<LangServeMessage> Messages { get; set; } = new();
}

/// <summary>
/// Message format for LangGraph
/// </summary>
public class LangServeMessage
{
    [JsonPropertyName("type")]
    public string Type { get; set; } = "human";

    [JsonPropertyName("content")]
    public string Content { get; set; } = string.Empty;
}

/// <summary>
/// Configuration section of the request
/// </summary>
public class LangServeConfig
{
    [JsonPropertyName("configurable")]
    public Dictionary<string, string> Configurable { get; set; } = new();
}
