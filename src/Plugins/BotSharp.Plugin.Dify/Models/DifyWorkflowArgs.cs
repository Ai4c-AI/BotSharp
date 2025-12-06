namespace BotSharp.Plugin.Dify.Models;

/// <summary>
/// Function arguments for calling Dify workflow
/// </summary>
public class DifyWorkflowArgs
{
    /// <summary>
    /// Workflow ID to execute
    /// </summary>
    [JsonPropertyName("workflow_id")]
    public string WorkflowId { get; set; } = string.Empty;

    /// <summary>
    /// Input parameters as JSON string or dictionary
    /// </summary>
    [JsonPropertyName("inputs")]
    public string? Inputs { get; set; }

    /// <summary>
    /// Whether to execute asynchronously (default: false)
    /// </summary>
    [JsonPropertyName("async")]
    public bool Async { get; set; } = false;

    /// <summary>
    /// Conversation ID for context
    /// </summary>
    [JsonPropertyName("conversation_id")]
    public string? ConversationId { get; set; }
}
