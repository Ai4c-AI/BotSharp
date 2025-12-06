namespace BotSharp.Plugin.AgentFramework.Settings;

/// <summary>
/// Configuration settings for Agent Framework (MAF) plugin
/// </summary>
public class AgentFrameworkSettings
{
    /// <summary>
    /// Enable or disable the plugin
    /// </summary>
    public bool Enabled { get; set; } = false;

    /// <summary>
    /// Timeout for HTTP requests in seconds
    /// </summary>
    public int TimeoutSeconds { get; set; } = 30;

    /// <summary>
    /// Polling interval in milliseconds for task status checks
    /// </summary>
    public int PollingIntervalMs { get; set; } = 2000;

    /// <summary>
    /// Maximum number of polling attempts before timeout
    /// </summary>
    public int MaxPollingAttempts { get; set; } = 60;

    /// <summary>
    /// Cache duration for agent cards in minutes
    /// </summary>
    public int AgentCardCacheDurationMinutes { get; set; } = 30;
}
