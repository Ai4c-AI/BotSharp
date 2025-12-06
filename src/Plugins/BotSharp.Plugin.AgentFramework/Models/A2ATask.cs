using System.Text.Json.Serialization;

namespace BotSharp.Plugin.AgentFramework.Models;

/// <summary>
/// Parameters for sendTask method
/// </summary>
public class SendTaskParams
{
    /// <summary>
    /// User input/request to the agent
    /// </summary>
    [JsonPropertyName("input")]
    public string Input { get; set; } = string.Empty;

    /// <summary>
    /// Conversation context or history
    /// </summary>
    [JsonPropertyName("context")]
    public object? Context { get; set; }

    /// <summary>
    /// Additional metadata
    /// </summary>
    [JsonPropertyName("metadata")]
    public Dictionary<string, object>? Metadata { get; set; }
}

/// <summary>
/// Parameters for getTask method
/// </summary>
public class GetTaskParams
{
    /// <summary>
    /// Task ID to retrieve
    /// </summary>
    [JsonPropertyName("taskId")]
    public string TaskId { get; set; } = string.Empty;
}

/// <summary>
/// Task result from A2A service
/// </summary>
public class TaskResult
{
    /// <summary>
    /// Task ID
    /// </summary>
    [JsonPropertyName("taskId")]
    public string TaskId { get; set; } = string.Empty;

    /// <summary>
    /// Task status (queued, running, completed, failed)
    /// </summary>
    [JsonPropertyName("status")]
    public string Status { get; set; } = string.Empty;

    /// <summary>
    /// Task output/result
    /// </summary>
    [JsonPropertyName("output")]
    public string? Output { get; set; }

    /// <summary>
    /// Progress information for running tasks
    /// </summary>
    [JsonPropertyName("progress")]
    public string? Progress { get; set; }

    /// <summary>
    /// Error information if task failed
    /// </summary>
    [JsonPropertyName("error")]
    public string? Error { get; set; }
}
