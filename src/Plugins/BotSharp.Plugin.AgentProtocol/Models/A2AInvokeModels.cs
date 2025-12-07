namespace BotSharp.Plugin.AgentProtocol.Models;

/// <summary>
/// Request to invoke an A2A agent
/// </summary>
public class A2AInvokeRequest
{
    /// <summary>
    /// Target agent ID
    /// </summary>
    public string AgentId { get; set; } = string.Empty;

    /// <summary>
    /// Request parameters
    /// </summary>
    public Dictionary<string, object> Parameters { get; set; } = new();

    /// <summary>
    /// Conversation context
    /// </summary>
    public List<RoleDialogModel> Context { get; set; } = new();

    /// <summary>
    /// Callback URL for async responses
    /// </summary>
    public string? CallbackUrl { get; set; }

    /// <summary>
    /// Request timeout in seconds
    /// </summary>
    public int? TimeoutSeconds { get; set; }
}

/// <summary>
/// Response from an A2A agent invocation
/// </summary>
public class A2AInvokeResponse
{
    /// <summary>
    /// Request ID for tracking
    /// </summary>
    public string RequestId { get; set; } = string.Empty;

    /// <summary>
    /// Response status
    /// </summary>
    public A2AResponseStatus Status { get; set; }

    /// <summary>
    /// Response data
    /// </summary>
    public RoleDialogModel? Response { get; set; }

    /// <summary>
    /// Error message if failed
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// Is async operation
    /// </summary>
    public bool IsAsync { get; set; }

    /// <summary>
    /// Progress percentage (0-100) for long-running tasks
    /// </summary>
    public int? Progress { get; set; }
}

/// <summary>
/// A2A response status
/// </summary>
public enum A2AResponseStatus
{
    Success,
    Pending,
    Failed,
    Timeout
}

/// <summary>
/// Progress notification from remote agent
/// </summary>
public class A2AProgressNotification
{
    /// <summary>
    /// Request ID
    /// </summary>
    public string RequestId { get; set; } = string.Empty;

    /// <summary>
    /// Progress percentage (0-100)
    /// </summary>
    public int Progress { get; set; }

    /// <summary>
    /// Progress message
    /// </summary>
    public string? Message { get; set; }

    /// <summary>
    /// Is completed
    /// </summary>
    public bool IsCompleted { get; set; }

    /// <summary>
    /// Result if completed
    /// </summary>
    public RoleDialogModel? Result { get; set; }
}
