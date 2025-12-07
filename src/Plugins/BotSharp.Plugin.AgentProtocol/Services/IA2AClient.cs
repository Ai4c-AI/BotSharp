namespace BotSharp.Plugin.AgentProtocol.Services;

/// <summary>
/// Interface for A2A client that enables agent-to-agent communication
/// </summary>
public interface IA2AClient
{
    /// <summary>
    /// Invoke a remote agent
    /// </summary>
    /// <param name="request">Invocation request</param>
    /// <returns>Invocation response</returns>
    Task<A2AInvokeResponse> InvokeAgentAsync(A2AInvokeRequest request);

    /// <summary>
    /// Get agent card from remote agent
    /// </summary>
    /// <param name="endpoint">Agent endpoint</param>
    /// <returns>Agent card</returns>
    Task<A2AAgentCard> GetAgentCardAsync(string endpoint);

    /// <summary>
    /// Register callback for async responses
    /// </summary>
    /// <param name="requestId">Request ID</param>
    /// <param name="callback">Callback action</param>
    void RegisterCallback(string requestId, Action<A2AInvokeResponse> callback);

    /// <summary>
    /// Register progress callback for long-running tasks
    /// </summary>
    /// <param name="requestId">Request ID</param>
    /// <param name="progressCallback">Progress callback action</param>
    void RegisterProgressCallback(string requestId, Action<A2AProgressNotification> progressCallback);

    /// <summary>
    /// Check if request is completed
    /// </summary>
    /// <param name="requestId">Request ID</param>
    /// <returns>True if completed</returns>
    Task<bool> IsRequestCompletedAsync(string requestId);

    /// <summary>
    /// Get request result
    /// </summary>
    /// <param name="requestId">Request ID</param>
    /// <returns>Request result</returns>
    Task<A2AInvokeResponse?> GetRequestResultAsync(string requestId);
}
