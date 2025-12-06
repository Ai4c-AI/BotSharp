using System.Text.Json.Serialization;

namespace BotSharp.Plugin.MicrosoftAgentFramework.Models;

/// <summary>
/// Generic workflow execution request
/// </summary>
public class WorkflowExecutionRequest
{
    [JsonPropertyName("workflow_type")]
    public string WorkflowType { get; set; } = string.Empty;

    [JsonPropertyName("input_data")]
    public Dictionary<string, object> InputData { get; set; } = new();

    [JsonPropertyName("conversation_history")]
    public List<ConversationMessage> ConversationHistory { get; set; } = new();

    [JsonPropertyName("serialized_state")]
    public string? SerializedState { get; set; }

    [JsonPropertyName("timeout_seconds")]
    public int? TimeoutSeconds { get; set; }
}

/// <summary>
/// Conversation message for context passing
/// </summary>
public class ConversationMessage
{
    [JsonPropertyName("role")]
    public string Role { get; set; } = string.Empty;

    [JsonPropertyName("content")]
    public string Content { get; set; } = string.Empty;

    [JsonPropertyName("timestamp")]
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}
