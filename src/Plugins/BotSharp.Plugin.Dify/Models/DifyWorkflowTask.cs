namespace BotSharp.Plugin.Dify.Models;

/// <summary>
/// Model for tracking async Dify workflow tasks
/// </summary>
public class DifyWorkflowTask
{
    /// <summary>
    /// Unique task identifier
    /// </summary>
    public string TaskId { get; set; } = string.Empty;

    /// <summary>
    /// Dify workflow run ID
    /// </summary>
    public string WorkflowRunId { get; set; } = string.Empty;

    /// <summary>
    /// Dify workflow ID
    /// </summary>
    public string WorkflowId { get; set; } = string.Empty;

    /// <summary>
    /// BotSharp conversation ID
    /// </summary>
    public string ConversationId { get; set; } = string.Empty;

    /// <summary>
    /// BotSharp agent ID
    /// </summary>
    public string AgentId { get; set; } = string.Empty;

    /// <summary>
    /// Message ID that triggered this workflow
    /// </summary>
    public string MessageId { get; set; } = string.Empty;

    /// <summary>
    /// Current task status
    /// </summary>
    public string Status { get; set; } = DifyWorkflowStatus.Running;

    /// <summary>
    /// Number of polling attempts made
    /// </summary>
    public int PollingAttempts { get; set; } = 0;

    /// <summary>
    /// Task creation time
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Last update time
    /// </summary>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Workflow result data (when completed)
    /// </summary>
    public string? ResultData { get; set; }

    /// <summary>
    /// Error message (if failed)
    /// </summary>
    public string? ErrorMessage { get; set; }
}
