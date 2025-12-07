namespace BotSharp.Plugin.AgentProtocol.Settings;

/// <summary>
/// Settings for Agent-to-Agent (A2A) protocol communication
/// </summary>
public class AgentProtocolSettings
{
    /// <summary>
    /// Enable A2A client functionality
    /// </summary>
    public bool EnableA2AClient { get; set; } = true;

    /// <summary>
    /// Agent registry endpoint URL for semantic routing
    /// </summary>
    public string? RegistryEndpoint { get; set; }

    /// <summary>
    /// Timeout for A2A operations in seconds
    /// </summary>
    public int TimeoutSeconds { get; set; } = 300;

    /// <summary>
    /// Enable async callbacks
    /// </summary>
    public bool EnableAsyncCallbacks { get; set; } = true;

    /// <summary>
    /// Callback endpoint URL for receiving push notifications
    /// </summary>
    public string? CallbackEndpoint { get; set; }

    /// <summary>
    /// Enable progress notifications for long-running tasks
    /// </summary>
    public bool EnableProgressNotifications { get; set; } = true;

    /// <summary>
    /// Default remote agent endpoints
    /// </summary>
    public Dictionary<string, string> RemoteAgents { get; set; } = new();
}
