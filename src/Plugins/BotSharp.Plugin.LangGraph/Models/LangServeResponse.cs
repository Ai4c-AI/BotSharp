using System.Text.Json.Serialization;

namespace BotSharp.Plugin.LangGraph.Models;

/// <summary>
/// Response model from LangServe API
/// </summary>
public class LangServeResponse
{
    [JsonPropertyName("output")]
    public LangServeOutput? Output { get; set; }

    [JsonPropertyName("status")]
    public string? Status { get; set; }

    [JsonPropertyName("__interrupt__")]
    public InterruptInfo? Interrupt { get; set; }
}

/// <summary>
/// Output section of the response
/// </summary>
public class LangServeOutput
{
    [JsonPropertyName("messages")]
    public List<LangServeMessage>? Messages { get; set; }

    [JsonPropertyName("content")]
    public string? Content { get; set; }
}

/// <summary>
/// Interrupt information for human-in-the-loop
/// </summary>
public class InterruptInfo
{
    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;

    [JsonPropertyName("message")]
    public string Message { get; set; } = string.Empty;
}

/// <summary>
/// Stream chunk from LangServe SSE
/// </summary>
public class LangServeStreamChunk
{
    [JsonPropertyName("event")]
    public string? Event { get; set; }

    [JsonPropertyName("data")]
    public string? Data { get; set; }

    [JsonPropertyName("content")]
    public string? Content { get; set; }
}
