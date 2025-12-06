using System.Text.Json.Serialization;

namespace BotSharp.Plugin.MicrosoftAgentFramework.Models;

/// <summary>
/// Result of MAF workflow execution
/// </summary>
public class WorkflowResult
{
    [JsonPropertyName("success")]
    public bool Success { get; set; }

    [JsonPropertyName("workflow_id")]
    public string WorkflowId { get; set; } = string.Empty;

    [JsonPropertyName("output")]
    public string Output { get; set; } = string.Empty;

    [JsonPropertyName("state")]
    public string? SerializedState { get; set; }

    [JsonPropertyName("metadata")]
    public Dictionary<string, object> Metadata { get; set; } = new();

    [JsonPropertyName("error_message")]
    public string? ErrorMessage { get; set; }

    [JsonPropertyName("execution_time_ms")]
    public long ExecutionTimeMs { get; set; }
}
